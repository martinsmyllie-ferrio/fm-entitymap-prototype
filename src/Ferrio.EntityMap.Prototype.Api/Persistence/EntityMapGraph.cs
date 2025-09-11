using Ferrio.EntityMap.Prototype.Api.Services.Models;
using Neo4j.Driver;
// using dotenv.net;

namespace Ferrio.EntityMap.Prototype.Api.Persistence;

public class EntityMapGraph(ILogger<EntityMapGraph> logger) : IEntityMapStorage
{
    private readonly ILogger<EntityMapGraph> _logger = logger;

    private const string dbUri = "neo4j+s://254a08ba.databases.neo4j.io";
    private const string dbUser = "neo4j";
    private const string dbPassword = "6MbH_60XVLdSl0TgSmX-cuuJLXe2dnFZpzZA5s7wWjg";

    //     private void InitializeCredentials()
    //     {
    //         DotEnv.Load(options: new DotEnvOptions(
    //             envFilePaths: new[] {"Neo4j-254a08ba-Created-2025-09-11.txt"},
    //             ignoreExceptions: false,
    //             overwriteExistingVars: false
    //         ));

    //         string? dbUri = System.Environment.GetEnvironmentVariable("NEO4J_URI");
    //         string? dbUser = System.Environment.GetEnvironmentVariable("NEO4J_USERNAME");
    //         string? dbPassword = System.Environment.GetEnvironmentVariable("NEO4J_PASSWORD");

    // // await using var driver = GraphDatabase.Driver(dbUri, AuthTokens.Basic(dbUser, dbPassword));
    // // await driver.VerifyConnectivityAsync();
    // // Console.WriteLine("Connection established.");
    //     }

    public async Task TestConnectionAsync()
    {
        await using var driver = GetDriver();
        await driver.VerifyConnectivityAsync();
        _logger.LogInformation("Neo4j connection established.");
    }

    public async Task CreateApplication(Application application)
    {
        await using var driver = GetDriver();

        var result = await driver.ExecutableQuery(@"CREATE (a:Application {id: $id, name: $name, type: $type})")
                                 .WithParameters(new { id = application.Id.ToString(), name = application.Name, type = application.ApplicationType })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var summary = result.Summary;
        _logger.LogInformation("Created {NodesCreated} nodes in {Milliseconds} ms.", summary.Counters.NodesCreated, summary.ResultAvailableAfter.Milliseconds);
    }

    public async Task CreateApplicationEnvironment(Guid applicationId, Services.Models.Environment environment)
    {
        await using var driver = GetDriver();

        var result = await driver.ExecutableQuery(@"
            MATCH (a:Application {id: $appId})
            CREATE (e:Environment {id: $envId, name: $name})-[:BELONGS_TO]->(a)")
                                 .WithParameters(new { appId = applicationId.ToString(), envId = environment.Id.ToString(), name = environment.Name })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var summary = result.Summary;

        _logger.LogInformation("Created {NodesCreated} nodes in {Milliseconds} ms.", summary.Counters.NodesCreated, summary.ResultAvailableAfter.Milliseconds);
    }

    public async Task CreateEntity(Guid environmentId, Entity entity)
    {
        await using var driver = GetDriver();

        var result = string.IsNullOrEmpty(entity.ParentReferenceId)
            ? await driver.ExecutableQuery(@"
                MATCH (env:Environment {id: $envId})
                CREATE (e:$($entityLabel) {id: $entityId, refId: $refId, name: $name})-[:BELONGS_TO]->(env)")
                                     .WithParameters(new { envId = environmentId.ToString(), entityId = entity.Id.ToString(), entityLabel = entity.EntityType, refId = entity.ReferenceId, name = entity.Name })
                                     .WithConfig(new QueryConfig(database: "neo4j"))
                                     .ExecuteAsync()
            : await driver.ExecutableQuery(@"
                MATCH (env:Environment {id: $envId}), (parent) WHERE parent.refId=$parentRefId
                CREATE (e:$($entityLabel) {id: $entityId, refId: $refId, name: $name})-[:BELONGS_TO]->(env), (e)-[:PARENT]->(parent)")
                                 .WithParameters(new
                                 {
                                     parentRefId = entity.ParentReferenceId,
                                     envId = environmentId.ToString(),
                                     entityId = entity.Id.ToString(),
                                     entityLabel = entity.EntityType,
                                     refId = entity.ReferenceId,
                                     name = entity.Name
                                 })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var summary = result.Summary;

        _logger.LogInformation("Created {NodesCreated} nodes in {Milliseconds} ms.", summary.Counters.NodesCreated, summary.ResultAvailableAfter.Milliseconds);
    }

    private static IDriver GetDriver()
    {
        return GraphDatabase.Driver(dbUri, AuthTokens.Basic(dbUser, dbPassword));
    }
}