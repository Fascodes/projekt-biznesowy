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
            // The rule with the highest combined priority wins
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
                    // Once a high‑priority rule matches, its effect determines availability
                    return r.Rule.Effect == Effect.Allow;
                }
            }

            // If no rule matches, default to available
            return true;
        }
    }
}
