using CommonCore;
using CommonCore.Migrations;
using CommonCore.State;
using Newtonsoft.Json.Linq;
using System;

/// <summary>
/// PersistState migration
/// </summary>
/// <remarks>
/// <para>This is an example of the single-migration migration strategy</para>
/// </remarks>
public class PersistStateUnifiedMigration : Migration<PersistState>
{
    public override Version MinInputVersion => null;

    public override Version MaxInputVersion => null;

    public override bool MigrateMaxVersion => true;

    public override JObject Migrate(JObject inputObject, MigrationContext context)
    {
        //set last migrated version
        var version = CoreParams.GetCurrentVersion();
        inputObject["LastMigratedVersion"] = JObject.FromObject(version, context.JsonSerializer);

        return inputObject;
    }
}