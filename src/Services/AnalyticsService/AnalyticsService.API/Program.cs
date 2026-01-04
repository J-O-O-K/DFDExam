using AnalyticsService.Application.Contracts.Persistence;
using AnalyticsService.Infrastructure.Messaging;
using AnalyticsService.Infrastructure.Workers;
using AnalyticsService.Persistence.Data;
using AnalyticsService.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Analytics Service API",
        Version = "v1",
        Description = "Analytics Service - Task event tracking with MongoDB and PostgreSQL metrics",
        Contact = new()
        {
            Name = "Task Management System",
            Email = "support@taskmanagement.com"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<MongoDbService>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AnalyticsService.Application.Features.Events.Commands.LogTaskEvent.LogTaskEventCommand).Assembly));

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(AnalyticsService.Application.Profiles.MappingProfile).Assembly);
});

builder.Services.AddScoped<ITaskEventRepository, TaskEventRepository>();
builder.Services.AddScoped<IMetricsRepository, MetricsRepository>();

builder.Services.AddHostedService<RabbitMQConsumer>();
builder.Services.AddHostedService<MetricsAggregationWorker>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Migrations and custom SQL scripts
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AnalyticsDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Running database migrations...");
        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations completed");

        var sqlScriptsPath = Path.Combine(AppContext.BaseDirectory, "Migrations", "CustomSql");
        if (Directory.Exists(sqlScriptsPath))
        {
            var scripts = Directory.GetFiles(sqlScriptsPath, "*.sql").OrderBy(f => f);

            foreach (var script in scripts)
            {
                logger.LogInformation("Executing SQL script: {ScriptName}", Path.GetFileName(script));
                var sql = await File.ReadAllTextAsync(script);
                await dbContext.Database.ExecuteSqlRawAsync(sql);
                logger.LogInformation("SQL script executed successfully: {ScriptName}", Path.GetFileName(script));
            }
        }
        else
        {
            logger.LogWarning("Custom SQL scripts directory not found: {Path}", sqlScriptsPath);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Analytics Service API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();