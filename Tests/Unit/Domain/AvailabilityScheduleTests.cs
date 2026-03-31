using System;
using System.Collections.Generic;
using AttractionCatalog.Domain.Core.Attractions.ValueObjects;
using AttractionCatalog.Domain.Modules.CatalogSearch;
using AttractionCatalog.Domain.Modules.CatalogSearch.Entities;
using AttractionCatalog.Domain.Modules.CatalogSearch.Services;
using AttractionCatalog.Domain.Modules.CatalogSearch.ValueObjects;
using Xunit;

namespace AttractionCatalog.Tests.Unit.Domain
{
    public class AvailabilityScheduleTests
    {
        [Fact]
        public void Schedule_Should_Allow_Available_Time()
        {
            // Arrange
            var schedule = new AvailabilitySchedule(100, new List<RuleId>());
            var compiler = new RuleSpecificationCompiler();
            var criteria = new SearchCriteria(new DateRange(DateTime.Now, DateTime.Now.AddDays(1)), null, null, new List<TagId>(), new List<TagId>());

            // Act
            bool result = schedule.IsAvailable(DateTime.Now, new List<RuleDefinition>(), compiler);

            // Assert
            Assert.True(result);
        }
    }
}
