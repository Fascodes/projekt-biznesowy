using Microsoft.Extensions.DependencyInjection;
using AttractionCatalog.Domain.Core.Attractions.Ports;
using AttractionCatalog.Infrastructure.Core.Attractions.Adapters;

namespace AttractionCatalog.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Register repository implementations for data persistence
            services.AddSingleton<IAttractionRepository, InMemoryAttractionRepository>();

            return services;
        }
    }
}
