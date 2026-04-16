using AttractionCatalog.Domain.Core.Attractions.Entities;
using AttractionCatalog.Domain.Core.Attractions.Enums;
using AttractionCatalog.Domain.Core.Attractions.Ports;
using AttractionCatalog.Domain.Core.Attractions.ValueObjects;
using AttractionCatalog.Domain.Modules.CatalogSearch.Entities;
using AttractionCatalog.Domain.Modules.CatalogSearch.Enums;

namespace AttractionCatalog.Infrastructure.Seeding;

public class AttractionSeeder
{
    private readonly IAttractionRepository _repository;

    public AttractionSeeder(IAttractionRepository repository)
    {
        _repository = repository;
    }

    public async Task SeedAsync()
    {
        // Sprawdzenie, czy dane już są w bazie
        var mainZooId = new AttractionId(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        if (await _repository.FindByIdAsync(mainZooId) != null) return;

        // TAGI 
        var tagOutdoor = new Tag(new TagId(Guid.NewGuid()), "OUTDOOR", "Na zewnątrz", "Atrakcja plenerowa");
        var tagFamily = new Tag(new TagId(Guid.NewGuid()), "FAMILY", "Rodzinne", "Idealne dla rodzin z dziećmi");
        var tagNature = new Tag(new TagId(Guid.NewGuid()), "NATURE", "Przyroda", "Obcowanie ze zwierzętami i naturą");

        // 3DOSTĘPNOŚĆ I REGUŁY 
        var mondayClosedRule = new RuleDefinition(
            new RuleId(Guid.NewGuid()), 
            RuleType.Weekly, 
            priority: 10, 
            Effect.Deny, 
            new Dictionary<string, object> { { "Day", "Monday" } }
        );
        
        // Harmonogram z przypiętą regułą "Zamknięte w poniedziałki"
        var baseSchedule = new AvailabilitySchedule(basePriority: 0, rules: new[] { mondayClosedRule.Id });
        var alwaysOpenSchedule = new AvailabilitySchedule(basePriority: 0, rules: Enumerable.Empty<RuleId>());

        // Warianty wykorzystania pojedynczej atrakcji
        var standardTour = new Scenario(
            new ScenarioId(Guid.NewGuid()),
            "Samodzielne zwiedzanie po obiekcie",
            TimeSpan.FromHours(3),
            new List<Tag> { tagFamily },
            baseSchedule
        );

        var feedingVariant = new Scenario(
            new ScenarioId(Guid.NewGuid()),
            "Karmienie Egzotycznych Zwierząt (Przewodnik)",
            TimeSpan.FromMinutes(45),
            new List<Tag> { tagNature },
            alwaysOpenSchedule // Inny harmonogram dla unikalnego wariantu
        );

        var miniZoo = new SingleAttraction(
            new AttractionId(Guid.Parse("22222222-2222-2222-2222-222222222222")),
            "Mini-ZOO dla Dzieci",
            AttractionState.Catalog, // Od razu gotowe do publikacji
            new List<Tag> { tagFamily, tagNature },
            new Location(50.0542, 19.8524),
            baseSchedule, // Ograniczenie z poniedziałkami
            new List<Scenario> { standardTour } // Scenariusz wizyty
        );

        var egzotarium = new SingleAttraction(
            new AttractionId(Guid.Parse("33333333-3333-3333-3333-333333333333")),
            "Egzotarium (Pawilon)",
            AttractionState.Catalog,
            new List<Tag> { tagNature },
            new Location(50.0546, 19.8520),
            alwaysOpenSchedule,
            new List<Scenario> { feedingVariant } // Wariant z karmieniem
        );

        var zooGroup = new AttractionGroup(
            mainZooId,
            "Kompleks Ogród Zoologiczny Kraków",
            SequenceMode.Flexible, // Części z grupy można zwiedzać w dowolnej kolejności
            new List<Tag> { tagOutdoor, tagFamily, tagNature },
            baseSchedule,
            new List<IAttractionComponent> { miniZoo, egzotarium } // Pakujemy pojedyncze obiekty do zbiorczej grupy
        );

        await _repository.SaveAsync(miniZoo);
        await _repository.SaveAsync(egzotarium);
        await _repository.SaveAsync(zooGroup);
    }
}
