using CommonCore;
using CommonCore.Config;
using CommonCore.Migrations;
using Newtonsoft.Json.Linq;
using System;

/// <summary>
/// ConfigState migration
/// </summary>
/// <remarks>
/// <para>This is an example of the single-migration migration strategy</para>
/// </remarks>
public class ConfigStateUnifiedMigration : Migration<ConfigState>
{
    public override Version MinInputVersion => null;

    public override Version MaxInputVersion => null;

    public override bool MigrateMaxVersion => true;

    public override JObject Migrate(JObject inputObject, MigrationContext context)
    {
        //set last migrated version (any)
        var version = CoreParams.GetCurrentVersion();
        inputObject["LastMigratedVersion"] = JObject.FromObject(version, context.JsonSerializer);

        //migrate difficulty from GameplayConfig to base (3.1.0 -> 4.0.0)
        if(!inputObject["CustomConfigVars"].IsNullOrEmpty() &&
            ((JObject)inputObject["CustomConfigVars"]).ContainsKey("GameplayConfig" ) &&
            ((JObject)inputObject["CustomConfigVars"]["GameplayConfig"]).ContainsKey("DifficultySetting"))
        {
            var difficulty = Convert.ToInt32(inputObject["CustomConfigVars"]["GameplayConfig"]["DifficultySetting"].ToObject<long>());
            ((JObject)inputObject["CustomConfigVars"]["GameplayConfig"]).Remove("DifficultySetting");
            inputObject["Difficulty"] = difficulty;
        }

        return inputObject;
    }
}
