using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AttractionCatalog.Domain.Core.Attractions.Aggregates;
using AttractionCatalog.Domain.Core.Attractions.Ports;
using AttractionCatalog.Domain.Core.Attractions.ValueObjects;
using AttractionCatalog.Domain.Modules.CatalogSearch;
using AttractionCatalog.Domain.Modules.CatalogSearch.ValueObjects;
using AttractionCatalog.Infrastructure.Persistence;

namespace AttractionCatalog.Infrastructure.Persistence.Repositories
{
    public class InMemoryAttractionRepository : IAttractionRepository
    {
        private readonly ConcurrentDictionary<AttractionId, IAttractionComponent> _data = new();

        public void Save(IAttractionComponent attraction) => _data[attraction.Id] = attraction;
        public IAttractionComponent FindById(AttractionId id) => _data.TryGetValue(id, out var a) ? a : null!;
        public List<IAttractionComponent> FindByCriteria(IQuerySpecification<IAttractionComponent> spec) => _data.Values.ToList();
    }
}
