@startuml
skinparam classAttributeIconSize 0
skinparam packageStyle rectangle
skinparam linetype ortho

package "Domain Layer (Core Logic)" {
    
    interface IAttractionComponent {
        + Id : AttractionId
        + Name : String
        + Tags : List<Tag>
        + Schedule : AvailabilitySchedule
    }

    class SingleAttraction {
        + Id : AttractionId
        + Name : String
        + State : AttractionState (Draft/Catalog/Internal)
        + Tags : List<Tag>
        + Schedule : AvailabilitySchedule
    }

    class Tag {
        + Id : TagId
        + Code : string
        + DisplayName : string
        + Description : string
    }

    class AttractionGroup {
        + Id : AttractionId
        + Name : String
        + SequenceMode : SequenceMode
        + Tags : List<Tag>
        + Schedule : AvailabilitySchedule
    }

    class AttractionRelation {
        + From : AttractionId
        + To : AttractionId
        + Type : RelationType (Requires/Excludes)
    }

    class Scenario {
        + Id : ScenarioId
        + Name : String
        + Duration : TimeSpan
        + Tags : List<Tag>
        + Schedule : AvailabilitySchedule
    }

    class AvailabilitySchedule {
        + BasePriority : int
        + ActiveRules : List<RuleId>
        + IsAvailable(time: DateRange) : bool
    }

    note right of AvailabilitySchedule
      Dla Grupy: IsAvailable = 
      Rules.Allow AND All(Children.IsAvailable)
    end note

    class RuleDefinition {
        + Id : RuleId
        + Type : RuleType (Weekly/Seasonal/Exception)
        + Priority : int
        + Effect : Effect (Allow/Deny)
        + Params : Dictionary<string, object>
    }

    class SearchCriteria {
        + TimeRange : DateRange
        + Location : Location
        + RequiredDuration : TimeSpan
        + RequiredTags : List<TagId>
        + ExcludedTags : List<TagId>
    }

    interface "ISpecification<SearchCriteria>" as ISpecification {
        + IsSatisfiedBy(candidate : SearchCriteria) : bool
    }

    class RuleSpecificationCompiler {
        + Compile(rules: List<RuleDefinition>) : ISpecification<SearchCriteria>
    }

    class AvailabilityResult {
        + ComponentId : AttractionId
        + IsAvailable : bool
        + AvailableScenarios : List<ScenarioId>
        + AppliedRules : List<RuleId>
    }

    class CatalogSearchService <<DomainService>> {
        + FindAvailableAttractions(criteria: SearchCriteria, attractions: List<IAttractionComponent>) : List<AvailabilityResult>
    }

    IAttractionComponent <|.. SingleAttraction
    IAttractionComponent <|.. AttractionGroup
    AttractionGroup o--> IAttractionComponent : "Agreguje Komponenty (Katalog/Internal)"
    
    SingleAttraction *--> Scenario : "Posiada Warianty (Scenariusze)"
    SingleAttraction *--> AvailabilitySchedule : "Posiada Profil (Globalny)"
    Scenario *--> AvailabilitySchedule : "Posiada Profil (Konkretny)"
    AvailabilitySchedule *--> RuleDefinition : "Zestaw reguł ewaluacyjnych"
    
    AttractionRelation ..> IAttractionComponent : "Wiąże (Requires/Excludes)"
    
    CatalogSearchService ..> RuleSpecificationCompiler : "Ocenia atrakcje"
    CatalogSearchService ..> AvailabilityResult : "Zwraca wynik dostępności"
    RuleSpecificationCompiler ..> ISpecification : "Tworzy filtry dla wyszukiwania"
    ISpecification ..> SearchCriteria : "Weryfikuje kryteria"
}

package "Application Layer (Use Cases)" {
    
    class CreateAttractionUseCase {
        + Execute(request : CreateDto)
    }
    
    class PublishAttractionUseCase {
        + Execute(id : AttractionId)
    }
    
    class CreateAttractionGroupUseCase {
        + Execute(request : GroupDto)
    }

    class SearchCatalogUseCase {
        + Execute(request : SearchQueryDto) : List<CatalogDto>
    }

    class AttractionGroupBuilder {
        + WithName(name : string) : Builder
        + AddComponent(id : AttractionId) : Builder
        + WithSequenceMode(mode : SequenceMode) : Builder
        + Build() : AttractionGroup
    }

    CreateAttractionUseCase ..> SingleAttraction : "tworzy stan Draft"
    PublishAttractionUseCase ..> ISpecification : "waliduje (CanBePublishedSpec)"
    CreateAttractionGroupUseCase ..> AttractionGroupBuilder : "używa"
    AttractionGroupBuilder ..> AttractionGroup : "buduje"
    SearchCatalogUseCase ..> CatalogSearchService : "deleguje wyszukiwanie"
}

package "Infrastructure Layer" {
    interface IAttractionRepository {
        + Save(attraction : IAttractionComponent)
        + FindById(id : AttractionId) : IAttractionComponent
    }
    
    class InMemoryAttractionRepository {
        - _data : ConcurrentDictionary
    }
    
    IAttractionRepository <|.. InMemoryAttractionRepository
}

package "API Layer" {
    class AttractionController {
        + Post()
        + Publish()
        + Get()
    }

    class CatalogController {
        + Search()
    }
    
    AttractionController ..> CreateAttractionUseCase : "wywołuje (HTTP POST)"
    AttractionController ..> PublishAttractionUseCase : "wywołuje (HTTP PUT)"
    CatalogController ..> SearchCatalogUseCase : "wywołuje (HTTP GET /search)"
}

' Zależności Architektoniczne
"Application Layer (Use Cases)" ..> "Domain Layer (Core Logic)" : "Zależy od domeny"
"Infrastructure Layer" ..> "Domain Layer (Core Logic)" : "Implementuje bazę dla"
"API Layer" ..> "Application Layer (Use Cases)" : "Orkiestracja"
"Application Layer (Use Cases)" ..> IAttractionRepository : "Wstrzykiwanie Zależności (DI)"

@enduml
