using System;
using System.Collections.Generic;
using System.Linq;
using AttractionCatalog.Domain.Core.Attractions.ValueObjects;
using AttractionCatalog.Domain.Modules.CatalogSearch.Enums;
using AttractionCatalog.Domain.Modules.CatalogSearch.Services;

namespace AttractionCatalog.Domain.Modules.CatalogSearch.Entities
{
    public class AvailabilitySchedule
    {
        public int BasePriority { get; }
        private readonly List<RuleId> _activeRuleIds;

        public AvailabilitySchedule(int basePriority, IEnumerable<RuleId> rules)
        {
            BasePriority = basePriority;
            _activeRuleIds = rules.ToList();
        }

        public bool IsAvailable(DateTime time, IEnumerable<RuleDefinition> allRules, RuleSpecificationCompiler compiler)
        {
            // Implementation of the "Priority Summation / Overriding" requested in the README
            // Rule with the highest (BasePriority + RulePriority) wins
            var applicableRules = allRules
                .Where(r => _activeRuleIds.Contains(r.Id))
                .Select(r => new { Rule = r, Priority = BasePriority + r.Priority })
                .OrderByDescending(r => r.Priority)
                .ToList();

            foreach (var r in applicableRules)
            {
                var spec = compiler.CompileRule(r.Rule);
                if (spec.IsSatisfiedBy(time))
                {
                    // High-priority rule found - its effect is FINAL for this time point
                    return r.Rule.Effect == Effect.Allow;
                }
            }

            // Default behavior if no rules match
            return true;
        }
    }
}
