using System.Collections.Generic;
using AttractionCatalog.Domain.Core.Attractions.ValueObjects;
using AttractionCatalog.Domain.Modules.CatalogSearch.Enums;

namespace AttractionCatalog.Domain.Modules.CatalogSearch.Entities
{
    public class RuleDefinition
    {
        public RuleId Id { get; }
        public RuleType Type { get; }
        public int Priority { get; }
        public Effect Effect { get; }
        public Dictionary<string, object> Params { get; }

        public RuleDefinition(RuleId id, RuleType type, int priority, Effect effect, Dictionary<string, object> parameters)
        {
            Id = id;
            Type = type;
            Priority = priority;
            Effect = effect;
            Params = parameters ?? new Dictionary<string, object>();
        }
    }
}
