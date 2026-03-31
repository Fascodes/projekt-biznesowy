using System;
using System.Threading.Tasks;
using AttractionCatalog.Application.Catalog.Features.Attractions.Commands.CreateAttraction;
using Microsoft.AspNetCore.Mvc;

namespace AttractionCatalog.API.Controllers
{
    public class AttractionController : ApiControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAttractionCommand command)
        {
            await Mediator.Send(command);
            return Ok();
        }

        [HttpPut("{id}/publish")]
        public async Task<IActionResult> Publish(Guid id)
        {
            // TODO: Publish command here
            return Ok();
        }
    }
}
