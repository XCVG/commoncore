using CommonCore;
using CommonCore.Config;
using CommonCore.Migrations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
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
        //set last migrated version
        var version = CoreParams.GetCurrentVersion();
        inputObject["LastMigratedVersion"] = JObject.FromObject(version, JsonSerializer.Create(CoreParams.DefaultJsonSerializerSettings));

        return inputObject;
    }
}
