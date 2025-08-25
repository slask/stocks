using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StocksAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints();

// Configure Auth0 Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["AUTH0_DOMAIN"];
        options.Audience = builder.Configuration["AUTH0_AUDIENCE"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "username",
            RoleClaimType = "stocks/roles", // This is important for roles
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };
    });


// Configure role-based authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("Employee", policy => 
        policy.RequireRole("Employee", "Admin"));
});


builder.Services.AddDbContext<StocksDbContext>(options =>
{
    var connectionString = builder.Configuration["DB_CONNECTION_STRING"] ?? throw new InvalidOperationException("Connection string not found.");

    options.UseNpgsql(connectionString);
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
});


builder.Services.AddSpaStaticFiles(options => { options.RootPath = "client-app/stocks/dist"; });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        bld => bld.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Apply database migrations on startup
await ApplyMigrationsAsync(app);


app.UseDeveloperExceptionPage();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();


app.UseFastEndpoints(c =>
{
    c.Serializer.Options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    c.Endpoints.RoutePrefix = "api";
});

app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    appBuilder =>
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseSpaStaticFiles();
        }

        appBuilder.UseSpa(spa =>
        {
            if (app.Environment.IsDevelopment())
            {
                spa.Options.SourcePath = "client-app/";
                spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
            }
        });
    });

app.Run();

// Method to apply database migrations
async static Task ApplyMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<StocksDbContext>();

        logger.LogInformation("Checking for pending database migrations...");

        var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();

        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying {Count} pending migrations: {Migrations}",
                pendingMigrations.Count, string.Join(", ", pendingMigrations));

            await dbContext.Database.MigrateAsync();

            logger.LogInformation("Database migrations applied successfully");
        }
        else
        {
            logger.LogInformation("Database is up to date. No migrations to apply");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations");
        throw;
    }
}
