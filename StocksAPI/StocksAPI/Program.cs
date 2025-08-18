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

builder.Services.AddSpaStaticFiles(options => { options.RootPath = "client-app/stocks/dist"; });

var app = builder.Build();


app.UseStaticFiles();
app.UseSpaStaticFiles();

// if (app.Environment.IsDevelopment())
// {
//     app.UseSpaStaticFiles();
// }

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseFastEndpoints(c =>
{
    c.Serializer.Options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    c.Endpoints.RoutePrefix = "api";
});
app.UseEndpoints(_ => { });

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "client-app/stocks";
    if (app.Environment.IsDevelopment())
    {
        spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
    }
    else
    {
       // spa.Options.DefaultPage = "/site/wwwroot/wwwroot/index.html";
    }
});
//
// app.UseWhen(
//     context => !context.Request.Path.StartsWithSegments("/api"),
//     appBuilder =>
//     {
//         appBuilder.UseSpaStaticFiles();
//         appBuilder.UseSpa(spa =>
//         {
//             spa.Options.DefaultPage= "/wwwroot/wwwroot/index.html";
//             spa.Options.SourcePath = "client-app/stocks/dist";
//             if (app.Environment.IsDevelopment())
//             {
//                 spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
//             }
//         });
//     });

app.Run();
