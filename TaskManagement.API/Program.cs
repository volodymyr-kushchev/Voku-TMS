using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TaskManagement.API.Middleware;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Extensions;
using TaskManagement.API.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

ConfigureWebHost(builder);
ConfigureLogging(builder);
ConfigureServices(builder);

var app = builder.Build();

ConfigureMiddleware(app);
ConfigureHealthChecks(app);
ConfigureDatabase(app);

app.Run();

static void ConfigureWebHost(WebApplicationBuilder builder)
{
    var urls = builder.Configuration.GetSection("Urls");
    if (urls.Exists())
    {
        builder.WebHost.UseUrls(urls["Http"] ?? throw new InvalidOperationException("HTTP URL is not configured. Please add 'Urls:Http' to your configuration."));
    }
}

static void ConfigureLogging(WebApplicationBuilder builder)
{
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"] ?? throw new InvalidOperationException("Seq server URL is not configured. Please add 'Seq:ServerUrl' to your configuration."))
        .CreateLogger();

    builder.Host.UseSerilog();
}

static void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddControllers();
    builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

    ConfigureApiVersioning(builder.Services);
    ConfigureHealthChecksServices(builder);
    ConfigureMassTransit(builder);
    ConfigureSwagger(builder.Services);

    builder.Services.AddInfrastructureServices(builder.Configuration);
}

static void ConfigureApiVersioning(IServiceCollection services)
{
    services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    });

    services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
}

static void ConfigureHealthChecksServices(WebApplicationBuilder builder)
{
    builder.Services.AddHealthChecks()
        .AddCheck<CustomHealthCheck>("custom", tags: ["custom"])
        .AddDbContextCheck<ApplicationDbContext>(
            name: "database",
            tags: ["db", "sql", "infrastructure"])
        .AddRabbitMQ(
            rabbitConnectionString: builder.Configuration.GetConnectionString("RabbitMQ") ?? throw new InvalidOperationException("RabbitMQ connection string is missing."),
            name: "rabbitmq",
            tags: ["messaging", "infrastructure"],
            timeout: TimeSpan.FromSeconds(30))
        .AddSqlServer(
            connectionString: builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Database connection string is missing."),
            name: "sqlserver",
            tags: ["db", "sql", "infrastructure"]);

    builder.Services.AddHealthChecksUI(setup =>
    {
        setup.SetEvaluationTimeInSeconds(60);
        setup.MaximumHistoryEntriesPerEndpoint(50);
        setup.SetApiMaxActiveRequests(3);
        setup.AddHealthCheckEndpoint("API", "http://api/health");
    }).AddInMemoryStorage();
}

static void ConfigureMassTransit(WebApplicationBuilder builder)
{
    builder.Services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            var connectionString = builder.Configuration.GetConnectionString("RabbitMQ") ?? throw new InvalidOperationException("RabbitMQ connection string is missing.");
            cfg.Host(new Uri(connectionString), h =>
            {
                h.Username("guest");
                h.Password("guest");
                h.Heartbeat(TimeSpan.FromSeconds(30));
                h.RequestedConnectionTimeout(TimeSpan.FromSeconds(30));
                h.UseCluster(c => { });
            });
            
            cfg.UseMessageRetry(r => r.Interval(10, TimeSpan.FromSeconds(5)));
            cfg.ConfigureEndpoints(context);
        });
    });
}

static void ConfigureSwagger(IServiceCollection services)
{
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}

static void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "swagger/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
            options.RoutePrefix = "swagger";
        });
    }
    else
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }

    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.UseStaticFiles();
    app.MapControllers();
}

static void ConfigureHealthChecks(WebApplication app)
{
    app.UseHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        AllowCachingResponses = false,
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        }
    });

    app.UseHealthChecksUI(options =>
    {
        options.UIPath = "/health-ui";
        options.ApiPath = "/health-api";
    });
}

static void ConfigureDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

public partial class Program { }
