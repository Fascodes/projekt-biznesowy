using Microsoft.Extensions.DependencyInjection;
using AttractionCatalog.Domain.Core.Attractions.Ports;
using AttractionCatalog.Infrastructure.Core.Attractions.Adapters;
using AttractionCatalog.Infrastructure.DataSeeding;

namespace AttractionCatalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IAttractionRepository, InMemoryAttractionRepository>();
        
        // Register seeder
        services.AddHostedService<AttractionSeeder>();
        
        return services;
    }
}
