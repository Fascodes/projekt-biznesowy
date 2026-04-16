using AttractionCatalog.Domain.Core.Attractions.Entities;
using AttractionCatalog.Domain.Core.Attractions.Enums;
using AttractionCatalog.Domain.Core.Attractions.Ports;
using AttractionCatalog.Domain.Core.Attractions.ValueObjects;
using AttractionCatalog.Domain.Modules.CatalogSearch.Entities;
using Microsoft.Extensions.Hosting;

namespace AttractionCatalog.Infrastructure.DataSeeding;

public sealed class AttractionSeeder : IHostedService
{
    private readonly IAttractionRepository _repository;

    public AttractionSeeder(IAttractionRepository repository)
    {
        _repository = repository;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Przykładowe tagi
        var tagFamily = new Tag(new TagId(Guid.NewGuid()), "RODZINA", "Przyjazne dla rodziny", "Znakomite miejsce dla rodzin z dziećmi");
        var tagHistory = new Tag(new TagId(Guid.NewGuid()), "HISTORIA", "Miejsce historyczne", "Zabytek o ogromnym znaczeniu historycznym");

        // Domyślny harmonogram dostępności
        var schedule = new AvailabilitySchedule(100, new List<RuleId>());

        // Scenariusz dla Wawelu
        var scenarioWawel = new Scenario(
            new ScenarioId(Guid.NewGuid()), 
            "Standardowe zwiedzanie zamku", 
            TimeSpan.FromHours(2), 
            new List<Tag> { tagFamily, tagHistory }, 
            schedule);

        // Atrakcja 1: Wawel
        var wawel = new SingleAttraction(
            new AttractionId(Guid.NewGuid()),
            "Zamek Królewski na Wawelu",
            AttractionState.Catalog, // Opublikowana
            new List<Tag> { tagFamily, tagHistory },
            new Location(50.0543, 19.9358), // Współrzędne Wawelu
            schedule,
            new List<Scenario> { scenarioWawel });

        // Scenariusz dla Sukiennic
        var scenarioSukiennice = new Scenario(
            new ScenarioId(Guid.NewGuid()), 
            "Zwiedzanie Podziemi", 
            TimeSpan.FromHours(1.5), 
            new List<Tag> { tagHistory }, 
            schedule);

        // Atrakcja 2: Sukiennice
        var sukiennice = new SingleAttraction(
            new AttractionId(Guid.NewGuid()),
            "Sukiennice i Rynek Główny",
            AttractionState.Catalog, // Opublikowana
            new List<Tag> { tagHistory },
            new Location(50.0617, 19.9373), // Sukiennice
            schedule,
            new List<Scenario> { scenarioSukiennice });

        // Zapis testowych danych do repozytorium (InMemory)
        await _repository.SaveAsync(wawel, cancellationToken);
        await _repository.SaveAsync(sukiennice, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
