using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AttractionCatalog.Application;
using AttractionCatalog.Infrastructure;
using AttractionCatalog.API.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Register core application services (dependency injection)
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// Register the custom global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable the exception handling middleware
app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
