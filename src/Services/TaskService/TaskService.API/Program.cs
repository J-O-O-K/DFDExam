using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskService.Application.Contracts.Messaging;
using TaskService.Application.Contracts.Persistence;
using TaskService.Infrastructure.Messaging;
using TaskService.Persistence.Data;
using TaskService.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Task Service API",
        Version = "v1",
        Description = "Task Management Service - Handles task CRUD operations with PostgreSQL advanced features",
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

builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention());

builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(TaskService.Application.Features.Tasks.Commands.CreateTask.CreateTaskCommand).Assembly));

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(TaskService.Application.Profiles.MappingProfile).Assembly);
});

builder.Services.AddScoped<ITaskRepository, TaskRepository>();

builder.Services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();

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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var dbContext = services.GetRequiredService<TaskDbContext>();
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        
        logger.LogInformation("Connection string: {ConnectionString}", connectionString);
        logger.LogInformation("Checking database connection...");
        
        // Test the connection first
        var canConnect = await dbContext.Database.CanConnectAsync();
        logger.LogInformation("Can connect to database: {CanConnect}", canConnect);
        
        if (!canConnect)
        {
            logger.LogError("Cannot connect to the database. Exiting...");
            throw new Exception("Database connection failed");
        }

        logger.LogInformation("Running database migrations...");
        logger.LogInformation("Pending migrations: {Migrations}", 
            string.Join(", ", await dbContext.Database.GetPendingMigrationsAsync()));
        
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations completed successfully");
        
        // Verify tables exist
        var tableCheck = await dbContext.Database.ExecuteSqlRawAsync(
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'tasks'");
        logger.LogInformation("Table check result: {Result}", tableCheck);

        var sqlScriptsPath = Path.Combine(AppContext.BaseDirectory, "Migrations", "CustomSql");
        logger.LogInformation("Looking for SQL scripts in: {Path}", sqlScriptsPath);
        logger.LogInformation("Directory exists: {Exists}", Directory.Exists(sqlScriptsPath));
        
        if (Directory.Exists(sqlScriptsPath))
        {
            var scripts = Directory.GetFiles(sqlScriptsPath, "*.sql").OrderBy(f => f);
            logger.LogInformation("Found {Count} SQL scripts", scripts.Count());

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
        logger.LogError(ex, "FATAL: An error occurred while migrating the database. Application will not start.");
        logger.LogError("Exception details: {Message}", ex.Message);
        logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
        
        if (ex.InnerException != null)
        {
            logger.LogError("Inner exception: {InnerMessage}", ex.InnerException.Message);
        }
        
        // Don't start the application if migration fails
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Service API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
