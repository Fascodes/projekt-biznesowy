using System;
using System.Collections.Generic;
using AttractionCatalog.Domain.Core.Attractions.Aggregates;
using AttractionCatalog.Domain.Core.Attractions.Enums;
using AttractionCatalog.Domain.Core.Attractions.ValueObjects;
using AttractionCatalog.Domain.Modules.CatalogSearch.Entities;
using Xunit;

namespace AttractionCatalog.Tests.Unit.Domain
{
    public class AttractionGroupTests
    {
        [Fact]
        public void Group_With_Multiple_Components_Should_Aggregate_Logic()
        {
            var schedule = new AvailabilitySchedule(0, new List<RuleId>());
            var components = new List<IAttractionComponent>
            {
                new SingleAttraction(new AttractionId(Guid.NewGuid()), "Muzeum", AttractionState.Catalog, new(), new Location(0,0), schedule, new()),
            };

            var group = new AttractionGroup(new AttractionId(Guid.NewGuid()), "Pass", SequenceMode.FLEXIBLE, new(), schedule, components);

            Assert.Single(group.Components);
        }
    }
}
