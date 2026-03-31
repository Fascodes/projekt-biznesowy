using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AttractionCatalog.Domain.Common.Models;
using AttractionCatalog.Domain.Core.Attractions.Aggregates;
using AttractionCatalog.Domain.Core.Attractions.ValueObjects;
using AttractionCatalog.Domain.Core.Attractions.Ports;
using AttractionCatalog.Domain.Modules.CatalogSearch.ValueObjects;

namespace AttractionCatalog.Infrastructure.Core.Attractions.Adapters
{
    public class InMemoryAttractionRepository : IAttractionRepository
    {
        private readonly ConcurrentDictionary<AttractionId, IAttractionComponent> _data = new();

        public void Save(IAttractionComponent attraction)
        {
            _data[attraction.Id] = attraction;

            // Dispatch domain events after a successful save 
            if (attraction is AggregateRoot aggregate)
            {
                DispatchEvents(aggregate);
            }
        }

        public IAttractionComponent FindById(AttractionId id) => _data.TryGetValue(id, out var a) ? a : null!;
        
        public List<IAttractionComponent> FindByCriteria(IQuerySpecification<IAttractionComponent> spec)
        {
            // TODO: apply specification filtering
            return _data.Values.ToList();
        }

        private void DispatchEvents(AggregateRoot aggregate)
        {
            foreach (var @event in aggregate.Events)
            {
                // In a production system, this would publish to a Message Bus (RabbitMQ/Kafka)
                // or a MediatR INotificationHandler.
                System.Diagnostics.Debug.WriteLine($"Domain Event Dispatched: {@event.GetType().Name} at {@event.OccurredOn}");
            }
            aggregate.ClearEvents();
        }
    }
}
