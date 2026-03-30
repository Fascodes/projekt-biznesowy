# System Zarządzania Atrakcjami Turystycznymi
## Plan Implementacji — Faza 1: Atrakcje, Kategorie i Relacje

---

## Opis problemu i zakres fazy 1

System umożliwia dodawanie atrakcji turystycznych (muzea, eventy, rejsy, szlaki, zwiedzanie), ich kategoryzowanie oraz definiowanie relacji między nimi. Atrakcje posiadają cykl życia (Draft → Catalog) i mogą być grupowane w złożone produkty turystyczne.

> **ZAKRES FAZY 1:** W tej fazie implementacja nie obejmuje systemów planowania wycieczek ani instancji planów. Skupiamy się na podstawowym modelu atrakcji, kategoryzacji oraz definicji logiki domenowej relacji.
> **MODUŁY WYŁĄCZONE (Out of Scope):** Architektura celowo abstrahuje od zarządzania rezerwacjami, weryfikacji pojemności w czasie rzeczywistym (Booking & Capacity) oraz obsługi płatności i cenników (Dynamic Pricing). Wymieniony w dokumencie moduł zachowuje odpowiedzialność wyłącznie Katalogu Wyszukującego (Catalog Search Service). Zwraca on informacje o ogólnym statusie otwarcia i ramowych możliwościach obiektu dla zadanego predykatu, nie śledząc utylizacji poszczególnych zasobów czy biletów.
---

## Architektura i Decyzje Projektowe (DDD & Clean Architecture)

System został zaprojektowany w oparciu o pryncypia **Domain-Driven Design (DDD)** oraz **Clean Architecture**. Gwarantuje to wysoką testowalność układu i ułatwia jego rozwój (w tym np. ekstrakcję modułów do osobnych mikroserwisów).

---

### 1. Struktura Projektu (Clean Architecture)

Logika biznesowa jest silnie odseparowana od warstwy technicznej:

- **Domain Layer**: Rdzeń systemu. Zawiera Encje, Agregaty (`SingleAttraction`), Value Objecty oraz Specyfikacje. Moduł całkowicie niezależny od technologii i frameworków.
- **Application Layer**: Obsługuje przypadki użycia (Use Cases, np. `PublishAttractionUseCase`) i odpowiada za orkiestrację obiektów domenowych.
- **Infrastructure Layer**: Realizacja aspektów technicznych (np. `InMemoryAttractionRepository`, logowanie, integracja z bazą danych typu SQL).
- **API Layer**: Kontrolery REST w warstwie prezentacji, wystawiające funkcjonalności na styku z interfejsami wierzchnimi/zewnętrznymi aplikacjami klienckimi.

---

### 2. Modułowość (Bounded Contexts)

Pomimo wspólnej bazy kodu, system jest podzielony na logiczne moduły (Bounded Contexts), co pozwala na ich niezależne rozwijanie:

- **Core/Attractions**: Zarządzanie definicją atrakcji, ich cyklem życia (Draft/Catalog), Tagami (`Tag`) służącymi do dynamicznego filtrowania bazodanowego.
- **Modules/Availability (Catalog Search)**: Silnik reguł odpowiedzialny za zaawansowane wyszukiwanie i filtrowanie atrakcji względem czasu, tagów (np. "Kryty", "Dla Dzieci") i reguł harmonogramu.
- **Modules/Relations**: Odseparowana logika powiązań między atrakcjami.

---

### 3. Kluczowe Wzorce Projektowe

#### Wzorzec Specification (DDD)
Wykorzystywany do hermetyzacji skomplikowanych reguł decyzyjnych (np. ewaluacja dostępności dla danego kontekstu, walidacja warunków do zmiany statusu na "Publikacja"), eliminując przy tym procedury warunkowe `if/else` i ułatwiając utrzymanie zasady Otwarte/Zamknięte (Open/Closed Principle) na obiektach w domenie.

```csharp
public interface ISpecification<T> {
    bool IsSatisfiedBy(T candidate);
}
```

#### Wzorzec Composite (Structural)
Umożliwia traktowanie pojedynczej atrakcji i grupy (pakietu) w identyczny sposób przez wspólny interfejs `IAttractionComponent`.

```csharp
public interface IAttractionComponent {
    AttractionId Id { get; }
    string Name { get; }
}

public class AttractionGroup : IAttractionComponent {
    public SequenceMode SequenceMode { get; }  // STRICT | FLEXIBLE | NONE
}
```

#### Wzorzec Builder (Creational)
Zapewnia bezpieczne formowanie złożonych obiektów (`AttractionGroup`). Budowniczy izoluje logikę walidacji (np. sprawdzanie czy składowe są w statusie `CATALOG` lub `INTERNAL`, czy czasy wycieczek ze sobą nie kolidują) od warstwy aplikacyjnej. Gwarantuje, że nie powołamy do życia "nielegalnego" z punktu widzenia biznesu pakietu.

```csharp
var group = new AttractionGroupBuilder(groupId)
    .WithName("Kraków Pass")
    .AddComponent(wawelId)
    .AddComponent(museumId)
    .WithSequenceMode(SequenceMode.FLEXIBLE)
    .Build(); // Tutaj zachodzi ostateczna walidacja domenowa
```

---

#### 4. Cykl życia i Identyfikacja (Draft → Catalog)

Każdy z kluczowych obiektów posiada jednoznaczny identyfikator reprezentowany przez wzorzec **Value Object** (np. `AttractionId`, będący wewnątrz typem `Guid`). Uniemożliwia to kompilatorowi dopuszczenie do błędów typu (np. przekazanie ID z grupy w miejście ID na atrakcję).

**Dostępne stany obiektu typu Atrakcja:**
- `DRAFT` — obiekt w procesie wdrażania i edycji opisu merytorycznego.
- `CATALOG` — obiekt zweryfikowany pod kątem logicznym oraz biznesowym, otwarty do bezpośredniego zakupu.
- `INTERNAL` — obiekt zweryfikowany, funkcjonalny biletowo i gotowy na wizyty, ale **niedostępny do samodzielnego zakupu** (sprzedawany wyłącznie w ramach Pakietów/Grup).
- `ARCHIVED` — obiekt objęty archiwizacją po wycofaniu z oferty turystycznej.

---

### 5. Relacje (Modules/Relations)

Relacje są oddzielnymi encjami, co pozwala na ich walidację (np. brak cykli) i nadawanie im metadanych bez modyfikacji klas atrakcji.

```csharp
public class AttractionRelation {
    public AttractionId From { get; }
    public AttractionId To { get; }
    public RelationType Type { get; }  // REQUIRES | EXCLUDES | RECOMMENDED_AFTER
}
```

---

### 6. Model Data-Driven Rules & Catalog Search (Modules/Availability)

Aby wyeliminować hardkodowanie logiki biznesowej, reguły dostępności obiektów są przechowywane persystentnie jako ustrukturyzowane wiersze danych. Zapewnia to dynamikę modyfikacji parametrów usług bez przerw w działaniu systemu. Architektura implementuje mechanizm **Priority-Based Rule Overriding**.

```csharp
public class RuleDefinition {
    public RuleId Id { get; }
    public RuleType Type { get; }       // WEEKLY | SEASONAL | EXCEPTION
    public int Priority { get; }        // Priorytet ewaluacji reguły (lokalny)
    public Effect Effect { get; }       // ALLOW | DENY
    public Dictionary<string, object> Params { get; } // Parametry (np. ramy czasowe, dni tygodnia)
}

public class AvailabilitySchedule {
    public int BasePriority { get; }    // Bazowy priorytet dla danego poziomu (np. Scenario > Attraction)
    public List<RuleId> ActiveRules { get; }
}
```

**Rozwiązywanie Konkurencji Reguł (Hierarchia):**
Podczas kompilacji zapytań system agreguje reguły z całej hierarchii (Group -> Attraction -> Scenario). Efektywny priorytet reguły wyliczany jest jako suma `BasePriority` harmonogramu oraz `Priority` samej reguły. Pozwala to na precyzyjne nadpisywanie nadrzędnych reguł (np. globalnego otwarcia całej atrakcji) przez bardziej specyficzne wyjątki z poziomu konkretnego scenariusza (np. chwilowe wyłączenie jednej trasy z powodu usterki).

---

### 6.1. System Dynamicznego Koszyka Cech (SearchCriteria i Tagi)

Elastyczne filtrowanie obiektów odbywa się z użyciem zapytań opartych na obiekcie `SearchCriteria`, operującym m.in. na zbiorach tagów (relacja Entity-Attribute-Value/Property Bag).

```csharp
public class SearchCriteria {
    public DateRange TimeRange { get; }
    public Location? Location { get; }          // Opcjonalny (Value Object) – filtr geograficzny
    public TimeSpan? RequiredDuration { get; }  // Opcjonalny – filtr czasu trwania
    public List<TagId> RequiredTags { get; }    // Predykaty inkluzywne (wymagane)
    public List<TagId> ExcludedTags { get; }    // Predykaty ekskluzywne (wykluczone)
}

// Location jako Value Object (DDD) – bez tożsamości, porównywany przez wartości pól
public record Location(double Latitude, double Longitude, double RadiusKm, string City);
```

**Pre-Filtrowanie na Poziomie Bazy Danych (IQuerySpecification):**
Aby uniknąć ładowania wszystkich atrakcji do pamięci RAM, architektura zawiera interfejs `IQuerySpecification<IAttractionComponent>`. Jego implementacja (`LocationQuerySpecification`) jest stosowana **warunkowo** — wyłącznie gdy `SearchCriteria.Location != null`. Jeśli kryterium lokalizacji nie zostaje przekazane, system pracuje na pełnym zbiorze atrakcji w stanie `CATALOG`.
---

### 6.2. Dostępność Grupy (Composite Logic)

W przypadku obiektów typu `AttractionGroup`, proces weryfikacji dostępności jest złożony i opiera się na **iloczynie logicznym** stanów wszystkich komponentów składowych oraz własnych reguł grupy.

**Algorytm oceny grupy:**
1. **Reguły Własne Grupy:** System w pierwszej kolejności ewaluuje `AvailabilitySchedule` przypisany bezpośrednio do grupy (np. "Pakiet dostępny tylko w weekendy").
2. **Dostępność Składowych:** Jeżeli grupa jest dopuszczona przez własne reguły, system rekurencyjnie sprawdza `Schedule.IsAvailable()` dla każdego powiązanego `IAttractionComponent`.
3. **Wynik Końcowy:** Grupa jest dostępna tylko wtedy, gdy jej własny harmonogram pozwala na operację (`ALLOW`) ORAZ każda z podrzędnych atrakcji posiada przynajmniej jeden dostępny wariant (Scenariusz).

---

### 7. Scenariusze (Warianty obiektu fizycznego)

Encja `SingleAttraction` może agregować w sobie kolekcję tzw. **Scenariuszy** (`Scenario`). Służą one do modelowania wielowariantowych sposobów eksploatacji danego miejsca.

**Granica `SingleAttraction` a `Scenario`:**
- **SingleAttraction:** Stanowi logiczny punkt docelowy. Definiuje stany nadrzędne (np. lokalizację fizyczną, globalne tagi przynależności do kategorii oraz bezwzględne reguły dostępności narzucone na obiekt z góry).
- **Scenario:** Abstrakcja reprezentująca wybrany wariant usługowy realizowany w danym punkcie (np. konkretna fizyczna trasa lub zbiór wytyczonych przejść). Posiada własny estymowany czas trwania (`Duration`), tagi uszczegóławiające oraz własny obiekt logiki harmonogramu (`AvailabilitySchedule`).
- **Mechanizm Odpytywania o Dostępność:** Obiekt atrakcji/scenariusza nie implementuje bezpośrednio logiki `IsAvailable`. Zamiast tego posiada on referencję do obiektu `AvailabilitySchedule`. Proces weryfikacji odbywa się poprzez delegację: `component.Schedule.IsAvailable(time)`.
- **Przecięcie Reguł (Intersection):** Proces kompilacji dostępności obiektu realizowany jest z wykorzystaniem iloczynu logicznego (`AND`) pomiędzy poszczególnymi poziomami harmonogramów. By wariant (Scenariusz) obiektu ukazał się w wyszukiwarce docelowej dla danego czasu, obydwie ewaluacje (`SingleAttraction.Schedule.IsAvailable()` ORAZ `Scenario.Schedule.IsAvailable()`) muszą zakończyć się statusem ALLOW. Tagi i cechy podlegają z kolei procesowi łączenia w dedykowanej warstwie projekcji (odczytu).

```yaml
SingleAttraction: "Muzeum Historyczne"
  Global_Rules:
    - [EXCEPTION] [DENY] [Priority: 100] [1 Maja]
  Scenario_A: "Zwiedzanie Pełne"
    Specific_Rules: [WEEKLY] [ALLOW] [Priority: 10] [Sob-Ndz]
    Tags: [Audio-guide]
  Scenario_B: "Wystawa Czasowa"
    Specific_Rules: [WEEKLY] [ALLOW] [Priority: 10] [Pn-Pt]
    Tags: [Edukacyjne]
```

---

### 8. Porównanie relacji i grup

| Mechanizm | Zastosowanie | Przykład Obrazowy |
| :--- | :--- | :--- |
| **Specyfikacja** | Hermetyzacja logiki reguł biznesowych | `IsAvailableSpec` (Weekend + Sezon Letni) |
| **Scenariusz** | Grupowanie konfiguracji w ramach pojedynczego obiektu domenowego | Kopalnia Wieliczka: "Trasa Turystyczna" vs "Trasa Górnicza" |
| **Grupa (Composite)** | Tworzenie złożonych produktów z wielu niezależnych obiektów | Pakiet: Wawel + Muzeum + Rejs |

**Zasada alokacji:**
- Różne metody eksploracji pojedynczej lokalizacji przypisywane są do wariantu obiektu logiki biznesowej jako **Scenariusz**.
- Zestaw niezależnych lokalizacji oferowany jako całościowy produkt modelowany jest jako **Grupa**.


---


---

---

## Warstwy Implementacyjne (Szczegóły)

### Application Layer

**`CreateAttractionUseCase.cs`**
Orkiestruje: walidacja DTO → stworzenie `SingleAttraction` w stanie `Draft` → zapis przez repo.

**`PublishAttractionUseCase.cs`**
Pobiera atrakcję → wywołuje `Publish()` → zapis. Waliduje czy atrakcja spełnia wymagania do publikacji.

**`CreateAttractionGroupUseCase.cs`**
Wykorzystuje wzorzec **Builder (`AttractionGroupBuilder`)** gwarantując weryfikację logiki stanów (`CATALOG`, `INTERNAL`) przed zestawieniem finalnej encji paczki usług.

**`SearchCatalogUseCase.cs`**
Główny komponent obsługujący logikę aplikacyjną odczytu (Dedykowany Read-Model). Analizuje zdefiniowane obiekty wizyt poprzez serwis aplikacyjny i mapuje rezultaty do specyfikacji obiektów transferu danych (DTO), uwzględniając wymogi tagowania i chronologii ze struktur `SearchCriteria`.

---

### API Layer

**`AttractionController.cs`**
Punkty dostępowe RESTful API:
- `POST /attractions` — Instancjonowanie nowego obiektu domenowego (stan `Draft`)
- `PUT /attractions/{id}/publish` — Transformacja stanu we wbudowanej domenie (`Draft → Catalog`)

**`CatalogController.cs`**
- `GET /catalog/search` — Endpoint integrujący parametryzowane wejście z elastycznym predykatem wyszukującym `SearchCatalogUseCase`.

**`dto/`**
Obiekty transferowe. Zgodnie z pryncypiami Czystej Architektury są całkowicie pozbawione zachowań biznesowych (Anemic Domain Models, funkcjonujące wyłącznie jako struktury Request/Response).

---

### Infrastructure Layer

**`InMemoryAttractionRepository.cs`**
Implementuje `IAttractionRepository` — na potrzeby PoC używa `ConcurrentDictionary`.


---

## UML

<img width="2064" height="1099" alt="image" src="https://github.com/user-attachments/assets/af82f265-b742-4f79-9608-da6f5bbb06f4" />

