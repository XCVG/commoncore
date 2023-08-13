using CommonCore;
using CommonCore.Config;
using CommonCore.Migrations;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

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
        context.MigrationHasChanges = false;

        //set last migrated version (any)
        var oldVersion = inputObject["LastMigratedVersion"].ToObject<VersionInfo>(context.JsonSerializer);
        var version = CoreParams.GetCurrentVersion();
        if (oldVersion != version)
        {
            inputObject["LastMigratedVersion"] = JObject.FromObject(version, context.JsonSerializer);
            context.MigrationHasChanges = true;
        }

        //migrate difficulty from GameplayConfig to base (3.1.0 -> 4.0.0)
        if (!inputObject["CustomConfigVars"].IsNullOrEmpty() &&
            ((JObject)inputObject["CustomConfigVars"]).ContainsKey("GameplayConfig" ) &&
            ((JObject)inputObject["CustomConfigVars"]["GameplayConfig"]).ContainsKey("DifficultySetting"))
        {
            var difficulty = Convert.ToInt32(inputObject["CustomConfigVars"]["GameplayConfig"]["DifficultySetting"].ToObject<long>());
            ((JObject)inputObject["CustomConfigVars"]["GameplayConfig"]).Remove("DifficultySetting");
            inputObject["Difficulty"] = difficulty;
            context.MigrationHasChanges = true;
        }

        //migrate graphics quality from Unity to ConfigState (3.1.0 -> 4.0.0)
        if(inputObject["GraphicsQuality"].IsNullOrEmpty())
        {
            var qualityLevel = QualitySettings.GetQualityLevel();
            inputObject["GraphicsQuality"] = qualityLevel;
            context.MigrationHasChanges = true;
        }

        return inputObject;
    }
}
