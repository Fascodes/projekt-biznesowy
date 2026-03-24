# System Zarządzania Atrakcjami Turystycznymi
## Plan Implementacji — Faza 1: Atrakcje, Kategorie i Relacje

---

## Opis problemu i zakres fazy 1

System umożliwia dodawanie atrakcji turystycznych (muzea, eventy, rejsy, szlaki, zwiedzanie), ich kategoryzowanie oraz definiowanie relacji między nimi. Atrakcje posiadają cykl życia (Draft → Catalog) i mogą być grupowane w złożone produkty turystyczne.

> **ZAKRES FAZY 1:** W tej fazie NIE implementujemy planowania wycieczek ani instancji planu. Skupiamy się wyłącznie na modelu atrakcji, ich kategoriach i relacjach między nimi.

---

## Kluczowe decyzje projektowe

### 1. Wzorzec Composite

Umożliwia traktowanie pojedynczej atrakcji i grupy w identyczny sposób przez wspólny interfejs.

```csharp
public interface IAttractionComponent
{
    AttractionId Id { get; }
    string Name { get; }
}

public class SingleAttraction : IAttractionComponent { ... }

public class AttractionGroup : IAttractionComponent
{
    private Dictionary<AttractionId, GroupComponentMetadata> _children;
    public SequenceMode SequenceMode { get; }  // STRICT | FLEXIBLE | NONE
}
```

> **TIP:** Ustawiając `FLEXIBLE`, pozwalamy na dowolną kolejność zwiedzania. Ustawiając `STRICT`, wymuszamy konkretną ścieżkę (np. najpierw wejście, potem komnata).

---

### 2. Identyfikacja i cykl życia (Draft → Catalog)

Każdy element posiada silnie typowany Value Object (np. `AttractionId`) owijający `Guid`.

**Stany atrakcji:**
- `DRAFT` — atrakcja jest w edycji. Posiada metadane i reguły, ale nie jest publicznie dostępna.
- `CATALOG` — atrakcja zweryfikowana. Może być wyświetlana i wyszukiwana przez użytkowników.
- `ARCHIVED` — atrakcja wycofana z katalogu.

> **TIP:** Katalog nie jest statyczną listą. `DiscoveryService` pobiera wszystkie atrakcje w stanie `CATALOG` i filtruje je dynamicznie przez `AvailabilityRules`.

---

### 3. Relacje: na poziomie modelu domenowego

Relacje są oddzielnymi encjami w domenie — nie są hardkodowane w klasach atrakcji.

- **Źle (poziom atrakcji):** klasa `Museum` ma w kodzie `requires(Ferry)`. To hardkodowanie.
- **Dobrze (poziom modelu):** relacje są osobnymi encjami.

```csharp
public class AttractionRelation
{
    public AttractionId From { get; }
    public AttractionId To { get; }
    public RelationType Type { get; }  // REQUIRES | EXCLUDES | RECOMMENDED_AFTER
}
```

> **TIP:** Dzięki temu logika relacji jest odseparowana od atrakcji. Dodanie nowego typu relacji nie wymaga zmian w klasach atrakcji.

---

### 4. Wzorzec Specification

Specyfikacje pozwalają na walidację bez zaśmiecania encji logiką. Implementowane jako czyste klasy domenowe.

```csharp
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T candidate);
    ISpecification<T> And(ISpecification<T> other);
    ISpecification<T> Or(ISpecification<T> other);
    ISpecification<T> Not();
}

// Sprawdza czy atrakcja jest w katalogu
public class IsInCatalogSpec : CompositeSpecification<SingleAttraction>
{
    public override bool IsSatisfiedBy(SingleAttraction a)
        => a.State == AttractionState.Catalog;
}
```

---

### 5. Model Data-Driven Rules (dostępność)

Aby uniknąć hardkodowania reguł, logika dostępności jest przechowywana jako dane. Każda reguła to osobna encja.

```csharp
public class RuleDefinition
{
    public RuleId Id { get; }
    public RuleType Type { get; }      // WEEKLY | SEASONAL | DATE_EXCEPTION
    public Dictionary<string, object> Params { get; }
    public int Priority { get; }
    public Effect Effect { get; }       // ALLOW | DENY
}
```

**Kluczowe cechy:**
- **Natychmiastowa aktualizacja** — zmiana reguły przez API od razu wpływa na `DiscoveryService`.
- **Udostępnianie reguł** — jedna reguła (np. „zamknięte 1 listopada") może obowiązywać wiele atrakcji.

---

### 6. Scenariusze (warianty atrakcji)

Jedna atrakcja może mieć wiele Scenariuszy — konfigurowalnych wariantów z osobnym czasem trwania, listą POI i regułami dostępności.

```
Attraction: "Zwiedzanie Wawelu"
  Scenario: "Trasa A – komnaty królewskie"
      Duration: 90min
      PointsOfInterest: ["Sala Poselska", "Komnata Królewska", "Kaplica"]
      Availability: Pn-Pt 09:00-17:00, Sb 10:00-14:00
                    Wyjątek: 2024-12-24 CLOSED
                    Sezony: SPRING, SUMMER, AUTUMN

  Scenario: "Trasa B – skarbiec + zbrojownia"
      Duration: 60min
      PointsOfInterest: ["Skarbiec", "Zbrojownia"]
      Availability: Sb-Nd 10:00-15:00, ALL seasons
```

> **TIP:** Czas trwania jest na poziomie `Scenario`, nie `Attraction` — różne trasy zajmują różny czas.

---

### Relacje (`domain/relation/`)

**`AttractionRelation.cs`**
Encja relacji: `From`, `To`, `Type` (`Requires` / `Excludes` / `RecommendedAfter`).

**`IAttractionRelationRepository.cs`**
Port repozytorium relacji — `FindBySource(AttractionId)`, `FindAll()`.

---

### Specyfikacje (`domain/specification/`)

**`ISpecification<T>.cs`**
Interfejs Specification z metodami `And()`, `Or()`, `Not()`.

**`CompositeSpecification<T>.cs`**
Abstrakcyjna klasa bazowa implementująca `And()`, `Or()`, `Not()` — konkretne specyfikacje implementują tylko `IsSatisfiedBy()`.

**`IsInCatalogSpec.cs`**
Sprawdza `attraction.State == Catalog`.

**`IsAvailableInSeasonSpec.cs`**
Sprawdza czy którykolwiek `Scenario` ma dostępność dla danego sezonu.

**`IsAvailableAtTimeSpec.cs`**
Sprawdza czy którykolwiek `Scenario` jest dostępny dla podanego `DateTime`.

---

## Application Layer

**`CreateAttractionUseCase.cs`**
Orkiestruje: walidacja DTO → stworzenie `SingleAttraction` w stanie `Draft` → zapis przez repo.

**`PublishAttractionUseCase.cs`**
Pobiera atrakcję → wywołuje `Publish()` → zapis. Waliduje czy atrakcja spełnia wymagania do publikacji.

**`CreateAttractionGroupUseCase.cs`**
Tworzy `AttractionGroup` z podanych ID komponentów. Sprawdza czy wszystkie komponenty istnieją.

**`AddAttractionRelationUseCase.cs`**
Dodaje relację między dwiema atrakcjami. Waliduje brak cykli dla relacji `REQUIRES`.

**`AddScenarioUseCase.cs`**
Dodaje `Scenario` do istniejącej atrakcji. Tylko atrakcje w stanie `Draft` mogą być modyfikowane.

---

## API Layer

**`AttractionController.cs`**
REST endpoints:
- `POST /attractions` — tworzenie atrakcji (stan: `Draft`)
- `PUT /attractions/{id}/publish` — zmiana stanu `Draft → Catalog`
- `POST /attractions/{id}/scenarios` — dodanie scenariusza
- `POST /attraction-groups` — tworzenie grupy
- `GET /attractions` — lista z filtrem `?state=CATALOG&season=WINTER`

**`AttractionRelationController.cs`**
REST endpoints:
- `POST /attraction-relations` — dodanie relacji (`from`, `to`, `type`)
- `GET /attraction-relations?source={id}` — relacje wychodzące z atrakcji

**`dto/`**
Request/Response DTOs — brak logiki, tylko dane. Przykłady: `CreateAttractionRequest`, `AttractionResponse`, `AddRelationRequest`.

---

## Infrastructure Layer

**`InMemoryAttractionRepository.cs`**
Implementuje `IAttractionRepository` — na potrzeby PoC używa `ConcurrentDictionary<AttractionId, SingleAttraction>`.

**`InMemoryAttractionRelationRepository.cs`**
Implementuje `IAttractionRelationRepository` — przechowuje listę relacji w pamięci.

---

## UML

<img width="2064" height="1099" alt="image" src="https://github.com/user-attachments/assets/af82f265-b742-4f79-9608-da6f5bbb06f4" />

