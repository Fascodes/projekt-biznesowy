using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AttractionCatalog.Application.Common.Models;
using AttractionCatalog.Application.Catalog.DTOs;
using AttractionCatalog.Domain.Core.Attractions.Aggregates;
using AttractionCatalog.Domain.Core.Attractions.ValueObjects;
using AttractionCatalog.Domain.Modules.CatalogSearch.Services;
using AttractionCatalog.Domain.Modules.CatalogSearch.ValueObjects;
using AttractionCatalog.Domain.Core.Attractions.Ports;
using MediatR;

namespace AttractionCatalog.Application.Catalog.Features.Catalog.Queries.SearchCatalog
{
    public record SearchCatalogQuery(
        DateTime Start, 
        DateTime End, 
        double? Lat, 
        double? Lon, 
        double? Radius, 
        List<Guid>? RequiredTags, 
        List<Guid>? ExcludedTags) : IRequest<Result<List<CatalogDto>>>;

    public class SearchCatalogHandler : IRequestHandler<SearchCatalogQuery, Result<List<CatalogDto>>>
    {
        private readonly IAttractionRepository _repository;
        private readonly CatalogSearchService _searchService;

        public SearchCatalogHandler(IAttractionRepository repository, CatalogSearchService searchService)
        {
            _repository = repository;
            _searchService = searchService;
        }

        public async Task<Result<List<CatalogDto>>> Handle(SearchCatalogQuery request, CancellationToken cancellationToken)
        {
            var criteria = new SearchCriteria(
                new DateRange(request.Start, request.End),
                request.Lat.HasValue ? new GeoArea(request.Lat.Value, request.Lon!.Value, request.Radius!.Value) : null,
                null,
                request.RequiredTags?.Select(id => new TagId(id)).ToList() ?? new(),
                request.ExcludedTags?.Select(id => new TagId(id)).ToList() ?? new()
            );

            var candidates = _repository.FindByCriteria(null!);
            var results = candidates.Where(c => _searchService.CheckAvailability(c, request.Start)).ToList();

            return Result<List<CatalogDto>>.Success(results.MapToDto());
        }
    }

    public static class SearchCatalogMapper
    {
        public static List<CatalogDto> MapToDto(this List<IAttractionComponent> components)
        {
            return components.Select(c => new CatalogDto
            {
                Id = c.Id.Value,
                Name = c.Name,
                IsAvailable = true // Skoro przeszły przez silnik SearchService, to są dostępne
            }).ToList();
        }
    }
}
