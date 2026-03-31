using System;

namespace AttractionCatalog.Application.Catalog.DTOs
{
    public class CatalogDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsAvailable { get; set; }
    }
}
