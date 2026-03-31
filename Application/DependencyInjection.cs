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
            // 1. Register MediatR for the whole assembly (Automatic Handler discovery)
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
                
                // 2. Add the "Ultra Pro" Validation Pipeline Behavior
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            // 3. Register Domain Services
            services.AddSingleton(new System.Collections.Generic.List<AttractionCatalog.Domain.Modules.CatalogSearch.Entities.RuleDefinition>());
            services.AddScoped<CatalogSearchService>();
            services.AddScoped<RuleSpecificationCompiler>();

            // 4. Register all Validators from current assembly (FluentValidation)
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            return services;
        }
    }
}
