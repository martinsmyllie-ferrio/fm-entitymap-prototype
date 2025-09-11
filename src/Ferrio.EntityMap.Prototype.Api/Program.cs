using Ferrio.EntityMap.Prototype.Api.Persistence;
using Ferrio.EntityMap.Prototype.Api.Services;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(theme: ConsoleTheme.None)
    .CreateBootstrapLogger();

ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);
var minWorkerThreads = Math.Max(workerThreads, 32);
ThreadPool.SetMinThreads(minWorkerThreads, completionPortThreads);

Log.Logger.Information("Minimum worker threads set to {MinWorkerThreads}", minWorkerThreads);


try
{
    Log.Logger.Information("Application is starting...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.ConfigureServices((context, services) =>
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddSingleton<IEntityMapStorage, EntityMapGraph>();
        services.AddSingleton<IApplicationService, ApplicationService>();
        services.AddSingleton<IEnvironmentService, EnvironmentService>();
        
        services.AddSwaggerGen(options =>
            {
                options.SupportNonNullableReferenceTypes();
                options.UseAllOfToExtendReferenceSchemas();
                options.UseAllOfForInheritance();

                options.CustomOperationIds(c => c.ActionDescriptor.RouteValues["action"]);

                options.SwaggerDoc("v1",
                    new()
                    {
                        Title = "Ferrio Entity Map Prototype API",
                        Version = "v1",
                        Contact = new() { Name = "Martin" }
                    });
            });
    });

    var app = builder.Build();

    app.UseSwagger(c => c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0);
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1"));
    app.MapControllers();
    app.Run();
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Console.WriteLine(e.StackTrace);
    Log.Fatal(e, "An unhandled exception occurred during bootstrapping");
}
finally
{
    Log.CloseAndFlush();
}