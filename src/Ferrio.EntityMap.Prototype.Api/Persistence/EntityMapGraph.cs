using Ferrio.EntityMap.Prototype.Api.Services.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Neo4j.Driver;
// using dotenv.net;

namespace Ferrio.EntityMap.Prototype.Api.Persistence;

public class EntityMapGraph(ILogger<EntityMapGraph> logger) : IEntityMapStorage
{
    private readonly ILogger<EntityMapGraph> _logger = logger;

    private const string dbUri = "neo4j+s://254a08ba.databases.neo4j.io";
    private const string dbUser = "neo4j";
    private const string dbPassword = "6MbH_60XVLdSl0TgSmX-cuuJLXe2dnFZpzZA5s7wWjg";

    private readonly Dictionary<Guid, string> _environmentApplicationTypes = [];

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

    // public async Task DeleteApplication(Guid applicationId)
    // {
    //     // DROP CONSTRAINT constraint_name [IF EXISTS]


    // }

    public async Task CreateApplication(Application application)
    {
        await using var driver = GetDriver();

        var result = await driver.ExecutableQuery(@"CREATE (a:Application {id: $id, name: $name, type: $type})")
                                 .WithParameters(new { id = application.Id.ToString(), name = application.Name, type = application.ApplicationType })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var summary = result.Summary;
        _logger.LogInformation("Created {NodesCreated} nodes in {Milliseconds} ms.", summary.Counters.NodesCreated, summary.ResultAvailableAfter.Milliseconds);

        foreach (var entityDefinition in application.EntityDefinitions ?? [])
        {
            await CreateEntityConstraint(driver, GetEntityLabel(application.ApplicationType, entityDefinition.EntityType));
        }
    }

    private async Task CreateEntityConstraint(IDriver driver, string entityLabel)
    {
        var result1 = await driver.ExecutableQuery(
            @"CREATE CONSTRAINT $constraintName FOR (n:" + entityLabel + ") REQUIRE n.refId IS NODE UNIQUE")
                             .WithParameters(new { constraintName = $"ux_{entityLabel}" })
                             .WithConfig(new QueryConfig(database: "neo4j"))
                             .ExecuteAsync();

        _logger.LogInformation("Created {ConstraintsAdded} constraints in {Milliseconds} ms.", result1.Summary.Counters.ConstraintsAdded, result1.Summary.ResultAvailableAfter.Milliseconds);

        var result2 = await driver.ExecutableQuery(@"CREATE CONSTRAINT $constraintName FOR (n:" + entityLabel + ") REQUIRE n.id IS NODE KEY")
                             .WithParameters(new { constraintName = $"pk_{entityLabel}" })
                             .WithConfig(new QueryConfig(database: "neo4j"))
                             .ExecuteAsync();

        _logger.LogInformation("Created {ConstraintsAdded} nodes in {Milliseconds} ms.", result2.Summary.Counters.ConstraintsAdded, result2.Summary.ResultAvailableAfter.Milliseconds);
    }

    public async Task CreateApplicationEnvironment(Guid applicationId, Services.Models.Environment environment)
    {
        await using var driver = GetDriver();

        var result = await driver.ExecutableQuery(@"
            MATCH (a:Application {id: $appId})
            CREATE (e:Environment {id: $envId, name: $name})-[:BELONGS_TO]->(a)
            RETURN a.type")
                                 .WithParameters(new { appId = applicationId.ToString(), envId = environment.Id.ToString(), name = environment.Name })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var summary = result.Summary;

        var appType = (string)result.Result[0]["a.type"];

        _logger.LogInformation("Created {NodesCreated} nodes in {Milliseconds} ms.", summary.Counters.NodesCreated, summary.ResultAvailableAfter.Milliseconds);

        _environmentApplicationTypes[environment.Id] = appType;
    }

    public async Task CreateEntity(Guid environmentId, Entity entity)
    {
        await using var driver = GetDriver();
        var applicationType = await GetApplicationType(driver, environmentId);

        var entityLabel = GetEntityLabel(applicationType, entity.EntityType);

        var result = string.IsNullOrEmpty(entity.ParentReferenceId)
            ? await driver.ExecutableQuery(@"
                MATCH (env:Environment {id: $envId})
                CREATE (e:$($entityLabel) {id: $entityId, refId: $refId, name: $name})-[:BELONGS_TO]->(env)")
                                     .WithParameters(new { envId = environmentId.ToString(), entityId = entity.Id.ToString(), entityLabel = entityLabel, refId = entity.ReferenceId, name = entity.Name })
                                     .WithConfig(new QueryConfig(database: "neo4j"))
                                     .ExecuteAsync()
            : await driver.ExecutableQuery(@"
                MATCH (env:Environment)<-[:BELONGS_TO]-(parent) WHERE env.id = $envId AND parent.refId = $parentRefId
                CREATE (e:$($entityLabel) {id: $entityId, refId: $refId, name: $name}), (e)-[:BELONGS_TO]->(env), (e)-[:PARENT]->(parent)")
                                 .WithParameters(new
                                 {
                                     parentRefId = entity.ParentReferenceId,
                                     envId = environmentId.ToString(),
                                     entityId = entity.Id.ToString(),
                                     entityLabel = entityLabel,
                                     refId = entity.ReferenceId,
                                     name = entity.Name
                                 })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var summary = result.Summary;

        _logger.LogInformation("Created {NodesCreated} nodes in {Milliseconds} ms.", summary.Counters.NodesCreated, summary.ResultAvailableAfter.Milliseconds);
    }

    public async Task CreateEntityMap(CreateEntityMap entityMap)
    {
        await using var driver = GetDriver();

        var sourceEntityLabel = GetEntityLabel(await GetApplicationType(driver, entityMap.SourceEnvironmentId), entityMap.SourceType);
        var targetEntityLabel = GetEntityLabel(await GetApplicationType(driver, entityMap.TargetEnvironmentId), entityMap.TargetType);

        var result = await driver.ExecutableQuery(@"
            MATCH (j:$($srcLabel)) WHERE j.refId=$srcRefId MATCH (k:$($tgtLabel)) WHERE k.refId=$tgtRefId CREATE (j)-[r:MAPS_TO]->(k)")
                                 .WithParameters(new
                                 {
                                     srcLabel = sourceEntityLabel,
                                     tgtLabel = targetEntityLabel,
                                     srcRefId = entityMap.SourceReferenceId,
                                     tgtRefId = entityMap.TargetReferenceId,
                                 })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var summary = result.Summary;

        _logger.LogInformation("Created {RelationshipsCreated} relationships in {Milliseconds} ms.", summary.Counters.RelationshipsCreated, summary.ResultAvailableAfter.Milliseconds);
    }

    public async Task CreateEntityPairWithMap(MappedEntities mappedEntities)
    {
        await using var driver = GetDriver();

        var sourceEntityLabel = GetEntityLabel(await GetApplicationType(driver, mappedEntities.SourceEnvironmentId), mappedEntities.SourceEntity.EntityType);
        var targetEntityLabel = GetEntityLabel(await GetApplicationType(driver, mappedEntities.TargetEnvironmentId), mappedEntities.TargetEntity.EntityType);

        if (!string.IsNullOrEmpty(mappedEntities.SourceEntity.ParentReferenceId) && !string.IsNullOrEmpty(mappedEntities.TargetEntity.ParentReferenceId))
        {
            var result = await driver.ExecutableQuery(@"
                MATCH (srcEnv:Environment) WHERE srcEnv.id=$srcEnvId
                MATCH (tgtEnv:Environment) WHERE tgtEnv.id=$tgtEnvId
                MATCH (srcEnv)<-[:BELONGS_TO]-(srcParent) WHERE srcParent.refId = $srcParentRefId
                MATCH (tgtEnv)<-[:BELONGS_TO]-(tgtParent) WHERE tgtParent.refId = $tgtParentRefId
                CREATE (src:$($srcLabel) {id:$srcId, name:$srcName, refId:$srcRefId})-[:MAPS_TO]->(tgt:$($tgtLabel) {id:$tgtId, name:$tgtName, refId:$tgtRefId})
                CREATE (src)-[:BELONGS_TO]->(srcEnv)
                CREATE (tgt)-[:BELONGS_TO]->(tgtEnv)
                CREATE (src)-[:PARENT]->(srcParent)
                CREATE (tgt)-[:PARENT]->(tgtParent)")
                                        .WithParameters(new
                                        {
                                            srcEnvId = mappedEntities.SourceEnvironmentId.ToString(),
                                            tgtEnvId = mappedEntities.TargetEnvironmentId.ToString(),
                                            srcParentRefId = mappedEntities.SourceEntity.ParentReferenceId,
                                            tgtParentRefId = mappedEntities.TargetEntity.ParentReferenceId,
                                            srcLabel = sourceEntityLabel,
                                            tgtLabel = targetEntityLabel,
                                            srcId = mappedEntities.SourceEntity.Id.ToString(),
                                            tgtId = mappedEntities.TargetEntity.Id.ToString(),
                                            srcName = mappedEntities.SourceEntity.Name,
                                            tgtName = mappedEntities.TargetEntity.Name,
                                            srcRefId = mappedEntities.SourceEntity.ReferenceId,
                                            tgtRefId = mappedEntities.TargetEntity.ReferenceId,
                                        })
                                        .WithConfig(new QueryConfig(database: "neo4j"))
                                        .ExecuteAsync();

            var summary = result.Summary;

            _logger.LogInformation("Created {NodesCreated} nodes and {RelationshipsCreated} relationships in {Milliseconds} ms.",
                summary.Counters.NodesCreated,
                summary.Counters.RelationshipsCreated,
                summary.ResultAvailableAfter.Milliseconds);
        }
        else
        {
            var result = await driver.ExecutableQuery(@"
                MATCH (srcEnv:Environment) WHERE srcEnv.Id=$srcEnvId
                MATCH (tgtEnv:Environment) WHERE tgtEnv.Id=$tgtEnvId
                CREATE (src:$($srcLabel) {id:$srcId, name:$srcName, refId:$srcRefId})-[:MAPS_TO]->(tgt:$($tgtLabel) {id:$tgtId, name:$tgtName, refId:$tgtRefId})
                CREATE (src)-[:BELONGS_TO]->(srcEnv)
                CREATE (tgt)-[:BELONGS_TO]->(tgtEnv)")
                                        .WithParameters(new
                                        {
                                            srcEnvId = mappedEntities.SourceEnvironmentId.ToString(),
                                            tgtEnvId = mappedEntities.TargetEnvironmentId.ToString(),
                                            srcLabel = sourceEntityLabel,
                                            tgtLabel = targetEntityLabel,
                                            srcId = mappedEntities.SourceEntity.Id.ToString(),
                                            tgtId = mappedEntities.TargetEntity.Id.ToString(),
                                            srcName = mappedEntities.SourceEntity.Name,
                                            tgtName = mappedEntities.TargetEntity.Name,
                                            srcRefId = mappedEntities.SourceEntity.ReferenceId,
                                            tgtRefId = mappedEntities.TargetEntity.ReferenceId,
                                        })
                                        .WithConfig(new QueryConfig(database: "neo4j"))
                                        .ExecuteAsync();

            var summary = result.Summary;

            _logger.LogInformation("Created {NodesCreated} nodes and {RelationshipsCreated} relationships in {Milliseconds} ms.",
                summary.Counters.NodesCreated,
                summary.Counters.RelationshipsCreated,
                summary.ResultAvailableAfter.Milliseconds);
        }
    }

    public async Task CreateEnvironmentCapabilityMap(Guid sourceEnvironmentId, Guid targetEnvironmentId, Dictionary<string, bool> capabilities)
    {
        await using var driver = GetDriver();

        var applicationType = await GetApplicationType(driver, sourceEnvironmentId);

        var entityTypePublishingCapabilities = capabilities.ToDictionary(
            kvp => GetEntityLabel(applicationType, kvp.Key),
            kvp => kvp.Value);

        var result = await driver.ExecutableQuery(@"
            MATCH(src: Environment) WHERE src.id = $srcId MATCH(tgt: Environment) WHERE tgt.id = $tgtId
            CREATE (src)-[:$($relationship) $capabilityProps]->(tgt)")
                                 .WithParameters(new
                                 {
                                     srcId = sourceEnvironmentId.ToString(),
                                     tgtId = targetEnvironmentId.ToString(),
                                     relationship = $"{applicationType}_PUBLISH_CAPABILITY".ToUpperInvariant(),
                                     capabilityProps = entityTypePublishingCapabilities,
                                 })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var summary = result.Summary;

        _logger.LogInformation("Created {RelationshipsCreated} relationships in {Milliseconds} ms.", summary.Counters.RelationshipsCreated, summary.ResultAvailableAfter.Milliseconds);
    }

    private async Task<string> GetApplicationType(IDriver driver, Guid environmentId)
    {
        if (_environmentApplicationTypes.TryGetValue(environmentId, out var applicationType))
        {
            return applicationType;
        }

        var result = await driver.ExecutableQuery(@"
                MATCH (e:Environment)-[r:BELONGS_TO]->(a:Application) WHERE e.id=$envId
                Return a.type")
                                .WithParameters(new { envId = environmentId.ToString() })
                                .WithConfig(new QueryConfig(database: "neo4j"))
                                .ExecuteAsync();

        var appType = (string)result.Result[0]["a.type"];

        _environmentApplicationTypes.TryAdd(environmentId, appType);

        return appType;
    }

    private static IDriver GetDriver()
    {
        return GraphDatabase.Driver(dbUri, AuthTokens.Basic(dbUser, dbPassword));
    }

    private static string GetEntityLabel(string applicationType, string entityType)
    {
        return $"{applicationType}_{entityType}".Replace(" ", "").ToLowerInvariant();
    }
}