using Microsoft.Extensions.DependencyInjection;
using MediatR;
using FluentValidation;
using AttractionCatalog.Application.Common.Behaviors;
using AttractionCatalog.Domain.Modules.CatalogSearch.Services;

namespace AttractionCatalog.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register MediatR handlers from this assembly
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
                
                // Add validation pipeline behavior
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            // Register domain services
            services.AddSingleton(new System.Collections.Generic.List<AttractionCatalog.Domain.Modules.CatalogSearch.Entities.RuleDefinition>());
            services.AddScoped<CatalogSearchService>();
            services.AddScoped<RuleSpecificationCompiler>();

            // Register all FluentValidation validators from this assembly
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            return services;
        }
    }
}
