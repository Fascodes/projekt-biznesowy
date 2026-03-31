using System;
using System.Collections.Generic;
using AttractionCatalog.Domain.Core.Attractions.Aggregates;
using AttractionCatalog.Domain.Core.Attractions.Enums;
using AttractionCatalog.Domain.Core.Attractions.ValueObjects;
using AttractionCatalog.Domain.Modules.CatalogSearch.Entities;
using AttractionCatalog.Domain.Modules.CatalogSearch.Enums;
using AttractionCatalog.Domain.Modules.CatalogSearch.Services;
using Xunit;

namespace AttractionCatalog.Tests.Unit.Domain.Core.Attractions
{
    public class RecursiveAvailabilityTests
    {
        [Fact]
        public void Group_Should_Be_Unavailable_If_OnlyOne_Child_Is_Unavailable()
        {
            // GIVEN: A deep hierarchy of attractions (Composite Pattern)
            var compiler = new RuleSpecificationCompiler();
            var allowRule = new RuleId(Guid.NewGuid());
            var denyRule = new RuleId(Guid.NewGuid());
            var rulesRepo = new List<RuleDefinition> 
            { 
                new(allowRule, RuleType.Weekly, 10, Effect.Allow, new()), 
                new(denyRule, RuleType.Exception, 100, Effect.Deny, new()) 
            };

            var availableSchedule = new AvailabilitySchedule(0, new List<RuleId> { allowRule });
            var unavailableSchedule = new AvailabilitySchedule(0, new List<RuleId> { denyRule });

            var child1 = new SingleAttraction(new(Guid.NewGuid()), "Child 1", AttractionState.Catalog, new(), new(0, 0), availableSchedule, new());
            var child2 = new SingleAttraction(new(Guid.NewGuid()), "Child 2", AttractionState.Catalog, new(), new(0, 0), unavailableSchedule, new());
            
            var group = new AttractionGroup(new(Guid.NewGuid()), "Root Group", SequenceMode.FLEXIBLE, new(), availableSchedule, new() { child1, child2 });

            var searchService = new CatalogSearchService(compiler, rulesRepo);

            // WHEN: Checking availability for the entire group
            bool result = searchService.CheckAvailability(group, DateTime.Now);

            // THEN: The entire group is unavailable because Child 2 is blocked (Recursive Logic)
            Assert.False(result);
        }
    }
}
