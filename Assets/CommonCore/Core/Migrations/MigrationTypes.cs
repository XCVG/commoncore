using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Migrations
{
    public class MigrationContext
    {
        public JsonSerializer JsonSerializer { get; internal set; }
        public bool MigrationHasChanges { get; set; } = true; //set to true for backwards compatibility
    }

    public class MigrationFailedException : Exception
    {
        internal MigrationFailedException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }

    public class MigrationIncompleteException : Exception
    {
        public Version TargetVersion { get; private set; }
        public Version ResultVersion { get; private set; }
        public Type Type { get; private set; }

        internal MigrationIncompleteException(Version targetVersion, Version resultVersion, Type type)
        {
            TargetVersion = targetVersion;
            ResultVersion = resultVersion;
            Type = type;
        }

        public override string Message => $"Failed to fully migrate {Type?.Name} (targeting {TargetVersion}, reached {ResultVersion})";
    }
}
