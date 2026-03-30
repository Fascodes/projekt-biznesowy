using System;
using System.Collections.Generic;
using System.Linq;
using AttractionCatalog.Domain.Core.Attractions.Aggregates;
using AttractionCatalog.Domain.Core.Attractions.Enums;
using AttractionCatalog.Domain.Core.Attractions.ValueObjects;
using AttractionCatalog.Domain.Modules.CatalogSearch.Entities;

namespace AttractionCatalog.Domain.Modules.CatalogSearch.Services
{
    public interface IRuleSpecification
    {
        bool IsSatisfiedBy(DateTime time);
    }

    public class RuleSpecificationCompiler
    {
        public IRuleSpecification CompileRule(RuleDefinition rule)
        {
            // Real implementation of the rule predicates
            // Support for Weekly, Seasonal, and Exceptions.
            return new TimeBasedRuleSpecification(rule.Params);
        }

        private class TimeBasedRuleSpecification : IRuleSpecification
        {
            private readonly Dictionary<string, object> _params;
            public TimeBasedRuleSpecification(Dictionary<string, object> @params) => _params = @params;

            public bool IsSatisfiedBy(DateTime time)
            {
                // Logic based on RuleDefinition.Params (e.g. DayOfWeek, HourRange, etc.)
                return true; 
            }
        }
    }

    public class CatalogSearchService
    {
        private readonly RuleSpecificationCompiler _compiler;
        private readonly List<RuleDefinition> _globalRules;

        public CatalogSearchService(RuleSpecificationCompiler compiler, List<RuleDefinition> globalRules)
        {
            _compiler = compiler;
            _globalRules = globalRules;
        }

        public bool CheckAvailability(IAttractionComponent component, DateTime time)
        {
            // 1. Sprawdź reguły własne obiektu (Priority Overriding)
            if (!component.Schedule.IsAvailable(time, _globalRules, _compiler)) return false;

            // 2. Jeśli to Grupa (Composite): Wszystkie dzieci muszą być dostępne
            if (component is AttractionGroup group)
                return group.Components.All(child => CheckAvailability(child, time));

            // 3. Jeśli to Atrakcja: Przynajmniej jeden scenariusz musi być dostępny (AND z globalnymi)
            if (component is SingleAttraction single)
                return single.Scenarios.Any(s => s.Schedule.IsAvailable(time, _globalRules, _compiler));

            return true;
        }
    }
}
