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
        context.MigrationHasChanges = false;

        //set last migrated version
        var oldVersion = inputObject["LastMigratedVersion"].ToObject<VersionInfo>(context.JsonSerializer);
        var version = CoreParams.GetCurrentVersion();
        if(oldVersion != version)
        {
            inputObject["LastMigratedVersion"] = JObject.FromObject(version, context.JsonSerializer);
            context.MigrationHasChanges = true;
        }
        

        return inputObject;
    }
}