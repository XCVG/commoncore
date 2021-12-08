using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using CommonCore.DebugLog;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace CommonCore.Migrations
{

    /// <summary>
    /// Manager for migrations
    /// </summary>
    /// <remarks>
    /// <para>The reason this is a lot less commoncore-y than other things (eg it is not a Module) is because I might reuse it elsewhere</para>
    /// </remarks>
    public class MigrationsManager
    {
        public static MigrationsManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new MigrationsManager();

                return _Instance;
            }
        }

        private static MigrationsManager _Instance;

        private Dictionary<Type, List<Type>> Migrations = new Dictionary<Type, List<Type>>();

        public MigrationsManager()
        {

        }

        public JObject MigrateToLatest<T>(JObject inputObject)
        {
            return MigrateToLatest(inputObject, typeof(T));
        }

        public JObject MigrateToLatest(JObject inputObject, Type type)
        {
            throw new NotImplementedException();

            //logic will be something like this:
            //-get last migrated version
            //-go through our list of migrations
            //-check for a migration that can migrate that version
            //-run that migration
            //-remove that migration from the list and repeat
        }

        private Version ParseLastMigratedVersion(JObject inputObject)
        {
            var lmv = inputObject["LastMigratedVersion"];

            if(lmv.Type == JTokenType.String)
            {
                return lmv.ToObject<Version>(JsonSerializer.Create(CoreParams.DefaultJsonSerializerSettings));
            }
            else if(lmv.Type == JTokenType.Object && ((JObject)lmv).ContainsKey("Major"))
            {
                return lmv.ToObject<Version>(JsonSerializer.Create(CoreParams.DefaultJsonSerializerSettings));
            }
            else if(lmv.Type == JTokenType.Object && ((JObject)lmv).ContainsKey("GameVersion"))
            {
                var vInfo = lmv.ToObject<VersionInfo>(JsonSerializer.Create(CoreParams.DefaultJsonSerializerSettings));
                return vInfo.GameVersion;
            }

            throw new NotSupportedException(); //TODO better exception
        }

        public void LoadMigrationsFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            LoadMigrationsFromTypes(assemblies.SelectMany(a => a.GetTypes()));
        }

        public void LoadMigrationsFromAssembly(Assembly assembly)
        {
            LoadMigrationsFromTypes(assembly.GetTypes());
        }

        public void LoadMigrationsFromTypes(IEnumerable<Type> types)
        {
            var allMigrations = types.Where(t => t.IsSubclassOf(typeof(Migration)) && !t.IsGenericTypeDefinition);
            var l = allMigrations.ToList();
            var groupedMigrations = allMigrations.GroupBy(t => GetMigrationTargetType(t));
            
            foreach(var migrationGroup in groupedMigrations)
            {
                if (migrationGroup.Key == default)
                    continue;

                if (!Migrations.ContainsKey(migrationGroup.Key))
                    Migrations.Add(migrationGroup.Key, new List<Type>());
                foreach(var migration in migrationGroup)
                {
                    Migrations[migrationGroup.Key].Add(migration);
                }
            }

            //Debug.Log(DebugUtils.JsonStringify(Migrations));
        }

        private static Type GetMigrationTargetType(Type migrationType)
        {
            for(Type baseType = migrationType.BaseType; baseType != null; baseType = baseType.BaseType)
            {
                if(baseType.IsGenericType)
                {
                    var generic = baseType.GetGenericTypeDefinition();
                    if (generic == typeof(Migration<>))
                    {
                        return baseType.GenericTypeArguments[0];
                    }
                }
            }

            return default;
        }
    }
}