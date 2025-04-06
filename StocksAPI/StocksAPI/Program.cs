using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StocksAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints();

// Register DbContext with SQLite
builder.Services.AddDbContext<StocksDbContext>(options =>
{
    options.UseSqlite("Data Source=stocks.db");
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseFastEndpoints(c =>
{
    c.Serializer.Options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

app.Run();
