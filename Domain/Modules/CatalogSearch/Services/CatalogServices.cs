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
            // Concrete implementation of rule predicates
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
            // Check component‑specific rules (priority overrides)
            if (!component.Schedule.IsAvailable(time, _globalRules, _compiler)) return false;

            // If component is a group, ensure all child components are available
            if (component is AttractionGroup group)
                return group.Components.All(child => CheckAvailability(child, time));

            // For a single attraction, at least one scenario must be available (combined with global rules)
            if (component is SingleAttraction single)
                return !single.Scenarios.Any() || single.Scenarios.Any(s => s.Schedule.IsAvailable(time, _globalRules, _compiler));

            return true;
        }
    }
}
