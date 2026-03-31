using System;
using System.Collections.Generic;
using AttractionCatalog.Domain.Modules.CatalogSearch.Entities;
using AttractionCatalog.Domain.Modules.CatalogSearch.Enums;
using AttractionCatalog.Domain.Modules.CatalogSearch.Services;
using AttractionCatalog.Domain.Core.Attractions.ValueObjects;
using Xunit;

namespace AttractionCatalog.Tests.Unit.Domain.Modules.CatalogSearch
{
    public class RuleResolutionTests
    {
        [Theory]
        [InlineData(0, 10, 100, false)]  // Entity(0) + Rule1(10) < Entity(0) + Rule2(100) -> DENY wins
        [InlineData(100, 50, 10, true)] // Entity(100) + Rule1(50) > Entity(100) + Rule2(10) -> ALLOW wins
        [InlineData(0, 50, 50, true)]   // Same priority: First one (Allow) in chronological order wins
        public void Logic_Should_Respect_Combined_Priorities(int baseP, int r1P, int r2P, bool expected)
        {
            // Arrange
            var compiler = new RuleSpecificationCompiler();
            var allowId = new RuleId(Guid.NewGuid());
            var denyId = new RuleId(Guid.NewGuid());

            var allowRule = new RuleDefinition(allowId, RuleType.Weekly, r1P, Effect.Allow, new());
            var denyRule = new RuleDefinition(denyId, RuleType.Exception, r2P, Effect.Deny, new());

            var schedule = new AvailabilitySchedule(baseP, new List<RuleId> { allowId, denyId });
            var rulesRepo = new List<RuleDefinition> { allowRule, denyRule };

            // Act
            bool result = schedule.IsAvailable(DateTime.Now, rulesRepo, compiler);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
