using System;
using System.Linq.Expressions;
using AttractionCatalog.Domain.Core.Attractions.Aggregates;
using AttractionCatalog.Domain.Core.Attractions.Ports;
using AttractionCatalog.Domain.Core.Attractions.ValueObjects;
using AttractionCatalog.Domain.Modules.CatalogSearch.ValueObjects;

namespace AttractionCatalog.Infrastructure.Specifications
{
    public class LocationQuerySpecification : IQuerySpecification<IAttractionComponent>
    {
        private readonly GeoArea _area;

        public LocationQuerySpecification(GeoArea area)
        {
            _area = area;
        }

        public Expression<Func<IAttractionComponent, bool>> ToExpression()
        {
            return component => 
                component is SingleAttraction && 
                // Tu używamy metody DistanceToKm z kroku 1
                new Location(((SingleAttraction)component).Location.Latitude, ((SingleAttraction)component).Location.Longitude)
                    .DistanceToKm(new Location(_area.CenterLatitude, _area.CenterLongitude)) <= _area.RadiusKm;
        }
    }
}
