using System;
using System.Linq.Expressions;
using AttractionCatalog.Domain.Core.Attractions;
using AttractionCatalog.Domain.Modules.CatalogSearch;

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
                component is SingleAttraction s && 
                // Tu używamy metody DistanceToKm z kroku 1
                new Location(s.Location.Latitude, s.Location.Longitude)
                    .DistanceToKm(new Location(_area.CenterLatitude, _area.CenterLongitude)) <= _area.RadiusKm;
        }
    }
}
