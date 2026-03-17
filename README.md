# Projekt biznesowy - Wachowicz, Zawartka, Łyko, Zięba

![Diagram1](diagram1.png)

## 1. Archetypy i wzorzec Kompozytu

System wykorzystuje wzorzec Kompozyt, aby umożliwić zarządzanie pojedynczymi atrakcjami i grupami atrakcji w spójny sposób.

###1 .1 Klasa bazowa: Attraction

Opis: Abstrakcyjna klasa reprezentująca pojedynczą atrakcję lub grupę atrakcji.

Atrybuty:

Id – unikalny identyfikator

Name – nazwa atrakcji

Description – opis

Metadata – dodatkowe, niezmienne informacje

Metody:

GetPrice() – oblicza cenę atrakcji

Validate() – sprawdza zgodność z regułami biznesowymi

### 1.2 Interfejs: IAttractionComponent

Cel: Zapewnienie spójnego kontraktu dla pojedynczych atrakcji (SingleAttraction) i grup (AttractionGroup).

### 1.3 Liść: SingleAttraction

Opis: Pojedyncza, niepodzielna atrakcja.

Atrybuty dodatkowe:

Duration – czas trwania

Location – miejsce

SeasonAvailability – okres dostępności

### 1.4 Kompozyt: AttractionGroup

Opis: Grupa atrakcji, mogąca zawierać inne komponenty (SingleAttraction lub AttractionGroup).

Metody:

Add(child) – dodaje atrakcję do grupy

Remove(child) – usuwa atrakcję z grupy

## 2. Stany atrakcji i katalog

### 2.1 Draft

Opis: Wstępny szkic atrakcji.

Metoda: DefineMetadata() – definiuje niezmienne cechy i wymagania.

Przejście: Po zakończeniu draftu powstaje Attraction.

### 2.2 CatalogItem

Opis: Pojedyncza oferta w katalogu.

Atrybuty:

Odwołanie do Attraction (pojedynczej lub grupowej)

Price – cena

AvailableFrom / AvailableTo – okres dostępności

### 2.3 Catalog

Opis: Lista wszystkich ofert.

Metody:

AddItem() – dodaje ofertę do katalogu

RemoveItem() – usuwa ofertę z katalogu

### 2.4 Instance

Opis: Konkretna rezerwacja utworzona na podstawie katalogu.

Atrybuty:

Date – data rezerwacji

UserId – identyfikator użytkownika

SelectedOptions – wybrane warianty

FinalPrice – cena końcowa

Odwołanie: Do CatalogItem, czyli konkretnej oferty

## 3. Relacje między atrakcjami

### 3.1 Klasa Relationship

Opis: Reprezentuje powiązania między atrakcjami.

Atrybuty:

From i To – wskazują powiązane atrakcje

Type – typ zależności (Mandatory / Optional)

Metoda: ValidateRelationship() – sprawdza poprawność zależności między atrakcjami

## 4. Podsumowanie architektury

Wzorzec Kompozyt umożliwia elastyczne zarządzanie pojedynczymi atrakcjami i grupami.

Stany atrakcji (Draft, Attraction, CatalogItem) ułatwiają kontrolę cyklu życia produktu.

Katalog i rezerwacje (Catalog, Instance) wspierają proces sprzedaży i rezerwacji.

Relacje między atrakcjami zapewniają integralność i zgodność oferty.
