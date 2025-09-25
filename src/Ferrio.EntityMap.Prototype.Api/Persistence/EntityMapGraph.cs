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

    public async Task CreateTenant(Tenant tenant)
    {
        await using var driver = GetDriver();

        var result = await driver.ExecutableQuery(@"CREATE (t:Tenant {id: $id, name: $name})")
                                 .WithParameters(new { id = tenant.TenantId.ToString(), name = tenant.Name })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var summary = result.Summary;

        _logger.LogInformation("Created {NodesCreated} nodes in {Milliseconds} ms.", summary.Counters.NodesCreated, summary.ResultAvailableAfter.Milliseconds);
    }

    public async Task CreateDomain(Guid tenantId, Domain domain)
    {
        await using var driver = GetDriver();

        var result = await driver.ExecutableQuery(@"
            MATCH (t:Tenant {id: $tenantId})
            CREATE (d:Domain {id: $id, name: $name})-[r:BELONGS_TO_TENANT]->(t)")
                                 .WithParameters(new
                                 {
                                     tenantId = tenantId.ToString(),
                                     id = domain.DomainId.ToString(),
                                     name = domain.Name
                                 })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var summary = result.Summary;

        _logger.LogInformation("Created {NodesCreated} nodes in {Milliseconds} ms.", summary.Counters.NodesCreated, summary.ResultAvailableAfter.Milliseconds);

        if (domain.EntityDefinitions is not null)
        {
            foreach (var entityDefinition in domain.EntityDefinitions)
            {
                var entityDefResult = await driver.ExecutableQuery(@"
                    MATCH (d:Domain {id: $domainId})
                    CREATE (ed:EntityDefinition {id: $id, name: $name, entityType: $entityType, parentEntityType: $parentEntityType})-[r:DEFINED_BY]->(d)")
                                     .WithParameters(new
                                     {
                                         domainId = domain.DomainId.ToString(),
                                         id = entityDefinition.EntityDefinitionId.ToString(),
                                         name = entityDefinition.Name,
                                         entityType = entityDefinition.EntityType,
                                         parentEntityType = entityDefinition.ParentEntityType
                                     })
                                     .WithConfig(new QueryConfig(database: "neo4j"))
                                     .ExecuteAsync();

                var entityDefSummary = entityDefResult.Summary;

                _logger.LogInformation("Created {NodesCreated} nodes in {Milliseconds} ms.", entityDefSummary.Counters.NodesCreated, entityDefSummary.ResultAvailableAfter.Milliseconds);
            }
        }
    }

    public async Task CreateApplication(Guid tenantId, Guid domainId, Application application)
    {
        await using var driver = GetDriver();

        var result = await driver.ExecutableQuery(@"
            MATCH (t:Tenant {id: $tenantId})
            MATCH (d:Domain {id: $domainId})
            CREATE (a:Application {id: $id, name: $name, type: $type})-[:BELONGS_TO_TENANT]->(t)
            CREATE (a)-[:BELONGS_TO_DOMAIN]->(d)")
                                 .WithParameters(new
                                 {
                                     tenantId = tenantId.ToString(),
                                     domainId = domainId.ToString(),
                                     id = application.ApplicationId.ToString(),
                                     name = application.Name,
                                     type = application.ApplicationType
                                 })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var summary = result.Summary;
        _logger.LogInformation("Created {NodesCreated} nodes in {Milliseconds} ms.", summary.Counters.NodesCreated, summary.ResultAvailableAfter.Milliseconds);
    }

    private async Task CreateEntityPrimaryKeyConstraint(IDriver driver, string entityLabel)
    {
        var result = await driver.ExecutableQuery(@"CREATE CONSTRAINT $constraintName FOR (n:" + entityLabel + ") REQUIRE n.id IS NODE KEY")
                             .WithParameters(new { constraintName = $"pk_{entityLabel}" })
                             .WithConfig(new QueryConfig(database: "neo4j"))
                             .ExecuteAsync();

        _logger.LogInformation("Created {ConstraintsAdded} nodes in {Milliseconds} ms.", result.Summary.Counters.ConstraintsAdded, result.Summary.ResultAvailableAfter.Milliseconds);
    }

    public async Task CreateApplicationEnvironment(Guid tenantId, Guid applicationId, Services.Models.Environment environment)
    {
        await using var driver = GetDriver();

        var result = await driver.ExecutableQuery(@"
            MATCH (t:Tenant {id: $tenantId})
            MATCH (a:Application {id: $appId})-[:BELONGS_TO_DOMAIN]->(d:Domain)
            CREATE (e:Environment {id: $envId, name: $name})-[:INSTANCE_OF_APPLICATION]->(a)
            CREATE (e)-[:BELONGS_TO_TENANT]->(t)
            RETURN a.type")
                                 .WithParameters(new
                                 {
                                     tenantId = tenantId.ToString(),
                                     appId = applicationId.ToString(),
                                     envId = environment.Id.ToString(),
                                     name = environment.Name
                                 })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var summary = result.Summary;

        var appType = (string)result.Result[0]["a.type"];

        _logger.LogInformation("Created {NodesCreated} nodes in {Milliseconds} ms.", summary.Counters.NodesCreated, summary.ResultAvailableAfter.Milliseconds);

        _environmentApplicationTypes[environment.Id] = appType;


        // sort entity types
        var result2 = await driver.ExecutableQuery(@"
            MATCH (a:Application {id: $appId})-[:BELONGS_TO_DOMAIN]->(d:Domain)
            MATCH (d)<-[:DEFINED_BY]-(ed:EntityDefinition)
            RETURN a.type, ed.entityType")
                                 .WithParameters(new
                                 {
                                     appId = applicationId.ToString()
                                 })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var entityTypes = result2.Result.Select(r => (string)r["ed.entityType"]).ToHashSet();

        foreach (var entityType in entityTypes ?? [])
        {
            var envEntityTypeLabel = GetEnvironmentEntityTypeLabel(environment.Id, entityType);

            await CreateEntityPrimaryKeyConstraint(driver, envEntityTypeLabel);
        }
    }

    public async Task CreateEntity(Guid tenantId, Guid environmentId, Entity entity)
    {
        await using var driver = GetDriver();

        // Validate entity type exists in domain for environment
        var parentEntityType = await ValidateEntityType(driver, environmentId, entity.EntityType);

        if (entity.Parent is not null && entity.Parent.EntityType != parentEntityType)
        {
            throw new Exception($"Entity type '{entity.EntityType}' must have parent entity type '{parentEntityType}' as defined in the domain for environment '{environmentId}'.");
        }
        else if (entity.Parent is null && parentEntityType is not null)
        {
            throw new Exception($"Entity type '{entity.EntityType}' must have a parent entity of type '{parentEntityType}' as defined in the domain for environment '{environmentId}'.");
        }

        var applicationType = await GetApplicationType(driver, environmentId);

        var (appEntityTypeLabel, envEntityTypeLabel) = GetNodeLabels(applicationType, environmentId, entity.EntityType);

        var result = entity.Parent is null
            ? await driver.ExecutableQuery(@"
                MATCH (t:Tenant {id: $tenantId})
                MATCH (env:Environment {id: $envId})
                CREATE (e:$($envEntityType):$($appEntityType):$($entityType) {id: $entityId, name: $name})-[:BELONGS_TO_ENVIRONMENT]->(env)
                CREATE (e)-[:PARENT]->(env)
                CREATE (e)-[:BELONGS_TO_TENANT]->(t)")
                                     .WithParameters(new
                                     {
                                         tenantId = tenantId.ToString(),
                                         envId = environmentId.ToString(),
                                         entityId = entity.Id.ToString(),
                                         envEntityType = envEntityTypeLabel,
                                         appEntityType = appEntityTypeLabel,
                                         entityType = entity.EntityType.ToPascalCase(),
                                         name = entity.Name
                                     })
                                     .WithConfig(new QueryConfig(database: "neo4j"))
                                     .ExecuteAsync()
            : await driver.ExecutableQuery(@"
                MATCH (t:Tenant {id: $tenantId})
                MATCH (t)<-[:BELONGS_TO_TENANT]-(env:Environment WHERE env.id = $envId)<-[:BELONGS_TO_ENVIRONMENT]-(parent:$($parentEntityType) WHERE parent.id = $parentId)
                CREATE (e:$($envEntityType):$($appEntityType):$($entityType) {id: $entityId, name: $name})
                CREATE (e)-[:BELONGS_TO_ENVIRONMENT]->(env)
                CREATE (e)-[:PARENT]->(parent)
                CREATE (e)-[:BELONGS_TO_TENANT]->(t)")
                                 .WithParameters(new
                                 {
                                     tenantId = tenantId.ToString(),
                                     parentId = entity.Parent.Id,
                                     parentEntityType = entity.Parent.EntityType.ToPascalCase(),
                                     envId = environmentId.ToString(),
                                     entityId = entity.Id.ToString(),
                                     envEntityType = envEntityTypeLabel,
                                     appEntityType = appEntityTypeLabel,
                                     entityType = entity.EntityType.ToPascalCase(),
                                     name = entity.Name
                                 })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var summary = result.Summary;

        _logger.LogInformation("Created {NodesCreated} nodes in {Milliseconds} ms.", summary.Counters.NodesCreated, summary.ResultAvailableAfter.Milliseconds);
    }

    public async Task CreateEntityMap(Guid tenantId, CreateEntityMap entityMap)
    {
        await using var driver = GetDriver();

        var sourceEnvironmentEntityTypeLabel = GetEnvironmentEntityTypeLabel(entityMap.SourceEnvironmentId, entityMap.SourceType);
        var targetEnvironmentEntityTypeLabel = GetEnvironmentEntityTypeLabel(entityMap.TargetEnvironmentId, entityMap.TargetType);

        var result = await driver.ExecutableQuery(@"
            MATCH (t:Tenant {id: $tenantId})
            MATCH (x:$($srcEnvEntityType))-[:BELONGS_TO_TENANT]->(t) WHERE x.id=$srcId
            MATCH (y:$($tgtEnvEntityType))-[:BELONGS_TO_TENANT]->(t) WHERE y.id=$tgtId
            CREATE (x)-[r:MAPS_TO { created: $created }]->(y)")
                                 .WithParameters(new
                                 {
                                     tenantId = tenantId.ToString(),
                                     srcEnvEntityType = sourceEnvironmentEntityTypeLabel,
                                     tgtEnvEntityType = targetEnvironmentEntityTypeLabel,
                                     srcId = entityMap.SourceEntityId,
                                     tgtId = entityMap.TargetEntityId,
                                     created = DateTime.UtcNow
                                 })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var summary = result.Summary;

        _logger.LogInformation("Created {RelationshipsCreated} relationships in {Milliseconds} ms.", summary.Counters.RelationshipsCreated, summary.ResultAvailableAfter.Milliseconds);
    }

    public async Task CreateEntityPairWithMap(Guid tenantId, MappedEntities mappedEntities)
    {
        await using var driver = GetDriver();

        var (srcAppEntityTypeLabel, srcEnvEntityTypeLabel) = GetNodeLabels(await GetApplicationType(driver, mappedEntities.SourceEnvironmentId), mappedEntities.SourceEnvironmentId, mappedEntities.SourceEntity.EntityType);
        var (tgtAppEntityTypeLabel, tgtEnvEntityTypeLabel) = GetNodeLabels(await GetApplicationType(driver, mappedEntities.TargetEnvironmentId), mappedEntities.TargetEnvironmentId, mappedEntities.TargetEntity.EntityType);

        if (mappedEntities.SourceEntity.Parent != null && mappedEntities.TargetEntity.Parent != null)
        {
            var result = await driver.ExecutableQuery(@"
                MATCH (t:Tenant {id: $tenantId})
                MATCH (srcEnv:Environment)-[:BELONGS_TO_TENANT]->(t) WHERE srcEnv.id=$srcEnvId
                MATCH (tgtEnv:Environment)-[:BELONGS_TO_TENANT]->(t) WHERE tgtEnv.id=$tgtEnvId
                MATCH (srcEnv)<-[:BELONGS_TO_ENVIRONMENT]-(srcParent:$($srcParentEntityType))-[:BELONGS_TO_TENANT]->(t) WHERE srcParent.id = $srcParentId
                MATCH (tgtEnv)<-[:BELONGS_TO_ENVIRONMENT]-(tgtParent:$($tgtParentEntityType))-[:BELONGS_TO_TENANT]->(t) WHERE tgtParent.id = $tgtParentId
                CREATE (src:$($srcEnvEntityType):$($srcAppEntityType):$($srcEntityType) {id:$srcId, name:$srcName})-[:MAPS_TO {created: $created}]->(tgt:$($tgtEnvEntityType):$($tgtAppEntityType):$($tgtEntityType) {id:$tgtId, name:$tgtName})
                CREATE (src)-[:BELONGS_TO_TENANT]->(t)
                CREATE (tgt)-[:BELONGS_TO_TENANT]->(t)
                CREATE (src)-[:BELONGS_TO_ENVIRONMENT]->(srcEnv)
                CREATE (tgt)-[:BELONGS_TO_ENVIRONMENT]->(tgtEnv)
                CREATE (src)-[:PARENT]->(srcParent)
                CREATE (tgt)-[:PARENT]->(tgtParent)")
                                        .WithParameters(new
                                        {
                                            tenantId = tenantId.ToString(),
                                            srcEnvId = mappedEntities.SourceEnvironmentId.ToString(),
                                            tgtEnvId = mappedEntities.TargetEnvironmentId.ToString(),
                                            srcParentId = mappedEntities.SourceEntity.Parent.Id,
                                            srcParentEntityType = mappedEntities.SourceEntity.Parent.EntityType.ToPascalCase(),
                                            tgtParentId = mappedEntities.TargetEntity.Parent.Id,
                                            tgtParentEntityType = mappedEntities.TargetEntity.Parent.EntityType.ToPascalCase(),
                                            srcEnvEntityType = srcEnvEntityTypeLabel,
                                            srcAppEntityType = srcAppEntityTypeLabel,
                                            srcEntityType = mappedEntities.SourceEntity.EntityType.ToPascalCase(),
                                            tgtEnvEntityType = tgtEnvEntityTypeLabel,
                                            tgtAppEntityType = tgtAppEntityTypeLabel,
                                            tgtEntityType = mappedEntities.TargetEntity.EntityType.ToPascalCase(),
                                            srcId = mappedEntities.SourceEntity.Id,
                                            tgtId = mappedEntities.TargetEntity.Id,
                                            srcName = mappedEntities.SourceEntity.Name,
                                            tgtName = mappedEntities.TargetEntity.Name,
                                            created = DateTime.UtcNow
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
                CREATE (src:$($srcEnvEntityType):$($srcAppEntityType):$($srcEntityType) {id:$srcId, name:$srcName})-[:MAPS_TO]->(tgt:$($tgtEnvEntityType):$($tgtAppEntityType):$($tgtEntityType) {id:$tgtId, name:$tgtName})
                CREATE (src)-[:PARENT]->(srcEnv)
                CREATE (tgt)-[:PARENT]->(tgtEnv)
                CREATE (src)-[:BELONGS_TO_ENVIRONMENT]->(srcEnv)
                CREATE (tgt)-[:BELONGS_TO_ENVIRONMENT]->(tgtEnv)")
                                        .WithParameters(new
                                        {
                                            srcEnvId = mappedEntities.SourceEnvironmentId.ToString(),
                                            tgtEnvId = mappedEntities.TargetEnvironmentId.ToString(),
                                            srcEnvEntityType = srcEnvEntityTypeLabel,
                                            srcAppEntityType = srcAppEntityTypeLabel,
                                            srcEntityType = mappedEntities.SourceEntity.EntityType.ToPascalCase(),
                                            tgtEnvEntityType = tgtEnvEntityTypeLabel,
                                            tgtAppEntityType = tgtAppEntityTypeLabel,
                                            tgtEntityType = mappedEntities.TargetEntity.EntityType.ToPascalCase(),
                                            srcId = mappedEntities.SourceEntity.Id.ToString(),
                                            tgtId = mappedEntities.TargetEntity.Id.ToString(),
                                            srcName = mappedEntities.SourceEntity.Name,
                                            tgtName = mappedEntities.TargetEntity.Name,
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
            kvp => GetApplicationEntityTypeLabel(applicationType, kvp.Key),
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


    public async Task CreateEnvironmentSettings(Guid tenantId, Guid environmentId, Dictionary<string, string> settings)
    {
        await using var driver = GetDriver();
        var applicationType = await GetApplicationType(driver, environmentId);

        var result = await driver.ExecutableQuery(@"
            MATCH (t:Tenant {id: $tenantId})
            MATCH (env:Environment {id: $envId})-[:BELONGS_TO_TENANT]->(t)
            MERGE (s:Settings)-[:SETTINGS_FOR]->(env)
                ON CREATE SET s = $settings
                ON MATCH SET s += $settings")
                                 .WithParameters(new
                                 {
                                     tenantId = tenantId.ToString(),
                                     envId = environmentId.ToString(),
                                     settings = settings
                                 })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var summary = result.Summary;

        _logger.LogInformation("Updated {PropertiesSet} properties in {Milliseconds} ms.", summary.Counters.PropertiesSet, summary.ResultAvailableAfter.Milliseconds);
    }

    public async Task CreateEntitySettings(Guid tenantId, Guid environmentId, string entityType, string entityId, Dictionary<string, string> settings)
    {
        await using var driver = GetDriver();
        var applicationType = await GetApplicationType(driver, environmentId);

        var result = await driver.ExecutableQuery(@"
            MATCH (t:Tenant {id: $tenantId})
            MATCH (env:Environment {id: $envId})-[:BELONGS_TO_TENANT]->(t)
            MATCH (e:$($entityType))-[:BELONGS_TO_ENVIRONMENT]->(env) WHERE e.id=$entityId
            MERGE (s:Settings)-[:SETTINGS_FOR]->(e)
                ON CREATE SET s = $settings
                ON MATCH SET s += $settings")
                                 .WithParameters(new
                                 {
                                     tenantId = tenantId.ToString(),
                                     envId = environmentId.ToString(),
                                     entityId = entityId,
                                     entityType = entityType.ToPascalCase(),
                                     settings = settings
                                 })
                                 .WithConfig(new QueryConfig(database: "neo4j"))
                                 .ExecuteAsync();

        var summary = result.Summary;

        _logger.LogInformation("Updated {PropertiesSet} properties in {Milliseconds} ms.", summary.Counters.PropertiesSet, summary.ResultAvailableAfter.Milliseconds);
    }

    public async Task<string> GetEntitySetting(Guid tenantId, Guid environmentId, string entityType, string entityId, string settingName)
    {
        await using var driver = GetDriver();

        var result = await driver.ExecutableQuery(@"
            MATCH (t:Tenant {id: $tenantId})
            MATCH (env:Environment {id: $envId})-[:BELONGS_TO_TENANT]->(t)
            MATCH (e:$($entityType))-[:BELONGS_TO_ENVIRONMENT]->(env) WHERE e.id=$entityId
            MATCH p = SHORTEST 1 (e)-[:PARENT|SETTINGS_FOR]-+(s:Settings WHERE s[$settingName] IS NOT NULL)
            RETURN length(p) as length, s[$settingName] AS " + settingName)
                                .WithParameters(new
                                {
                                    tenantId = tenantId.ToString(),
                                    envId = environmentId.ToString(),
                                    entityId = entityId,
                                    entityType = entityType.ToPascalCase(),
                                    settingName = settingName
                                })
                                .WithConfig(new QueryConfig(database: "neo4j"))
                                .ExecuteAsync();

        var summary = result.Summary;

        var shortestPath = 0L;

        var settingValue = string.Empty;

        foreach (var record in result.Result)
        {
            var length = (long)record["length"];

            if (shortestPath == 0 || length < shortestPath)
            {
                shortestPath = length;

                settingValue = (string)record[settingName];
            }
        }

        // _logger.LogInformation("Retrieved records in {Milliseconds} ms.", summary.Counters., summary.ResultAvailableAfter.Milliseconds);

        return settingValue;
    }

    private async Task<string> GetApplicationType(IDriver driver, Guid environmentId)
    {
        if (_environmentApplicationTypes.TryGetValue(environmentId, out var applicationType))
        {
            return applicationType;
        }

        var result = await driver.ExecutableQuery(@"
                MATCH (e:Environment)-[r:INSTANCE_OF_APPLICATION]->(a:Application) WHERE e.id=$envId
                RETURN a.type")
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

    private static string GetApplicationEntityTypeLabel(string applicationType, string entityType)
    {
        return $"{applicationType.ToPascalCase()}{entityType.ToPascalCase()}".Replace(" ", "").Replace("_", "").Replace("-", "");
    }

    private static string GetEnvironmentEntityTypeLabel(Guid environmentId, string entityType)
    {
        return $"{entityType.ToPascalCase()}{environmentId:N}".Replace(" ", "").Replace("_", "").Replace("-", "");
    }

    private static Tuple<string, string> GetNodeLabels(string applicationType, Guid environmentId, string entityType)
    {
        var appEntityTypeLabel = GetApplicationEntityTypeLabel(applicationType, entityType);
        var envEntityTypeLabel = GetEnvironmentEntityTypeLabel(environmentId, entityType);

        return new Tuple<string, string>(appEntityTypeLabel, envEntityTypeLabel);
    }

    private async Task<string> ValidateEntityType(IDriver driver, Guid environmentId, string entityType)
    {
        var result = await driver.ExecutableQuery(@"
                MATCH p = SHORTEST 1 (e:Environment WHERE e.id = $envId)-[]-+(ed:EntityDefinition WHERE ed.entityType = $entityType) RETURN length(p) AS length, ed.parentEntityType AS parentEntityType")
                                .WithParameters(new
                                {
                                    envId = environmentId.ToString(),
                                    entityType = entityType
                                })
                                .WithConfig(new QueryConfig(database: "neo4j"))
                                .ExecuteAsync();

        var summary = result.Summary;

        if (result.Result.Count == 0)
        {
            throw new Exception($"Entity type '{entityType}' is not defined in the domain for environment '{environmentId}'.");
        }

        var shortestPath = 0L;

        string parentEntityType = null;

        foreach (var record in result.Result)
        {
            var length = (long)record["length"];

            if (shortestPath == 0 || length < shortestPath)
            {
                shortestPath = length;

                parentEntityType = (string)record["parentEntityType"];
            }
        }

        return parentEntityType;
    }
}

public static class StringExtensions
{
    public static string ToPascalCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        if (str.Length == 1)
            return char.ToUpperInvariant(str[0]).ToString();

        return char.ToUpperInvariant(str[0]) + str[1..];
    }
}