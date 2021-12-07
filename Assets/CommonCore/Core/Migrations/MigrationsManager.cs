using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

namespace CommonCore.Migrations
{

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

        private Dictionary<Type, List<Migration>> Migrations = new Dictionary<Type, List<Migration>>();

        public MigrationsManager()
        {

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
            var allMigrations = types.Where(t => t.IsSubclassOf(typeof(Migration)) && t.IsGenericType && t.IsConstructedGenericType);
            var groupedMigrations = allMigrations.GroupBy(t => t.GenericTypeArguments[0]);
        }
    }
}