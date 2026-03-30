using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AttractionCatalog.Application.Common.Models;
using AttractionCatalog.Domain.Core.Attractions.Aggregates;
using AttractionCatalog.Domain.Core.Attractions.Enums;
using AttractionCatalog.Domain.Core.Attractions.ValueObjects;
using AttractionCatalog.Domain.Modules.CatalogSearch.Entities;
using AttractionCatalog.Domain.Core.Attractions.Ports;
using FluentValidation;
using MediatR;

namespace AttractionCatalog.Application.Catalog.Features.Attractions.Commands.CreateAttraction
{
    public record CreateAttractionCommand(string Name, Location Location, List<Guid> TagIds) : IRequest<Result>;

    public class CreateAttractionValidator : AbstractValidator<CreateAttractionCommand>
    {
        public CreateAttractionValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Location).NotNull();
        }
    }

    public class CreateAttractionHandler : IRequestHandler<CreateAttractionCommand, Result>
    {
        private readonly IAttractionRepository _repository;
        public CreateAttractionHandler(IAttractionRepository repository) => _repository = repository;

        public async Task<Result> Handle(CreateAttractionCommand request, CancellationToken cancellationToken)
        {
            var attraction = new SingleAttraction(
                new AttractionId(Guid.NewGuid()),
                request.Name,
                AttractionState.Draft,
                new(), request.Location,
                new AvailabilitySchedule(0, new List<RuleId>()),
                new());

            attraction.Publish(); // Zmiana stanu na CATALOG
            _repository.Save(attraction);
            return Result.Success();
        }
    }
}
