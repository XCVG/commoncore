using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Migrations
{
    /// <summary>
    /// Base class for migrations. Do not derive directly from this.
    /// </summary>
    public abstract class Migration
    {        
        /// <summary>
        /// Minimum input version (always inclusive)
        /// </summary>
        public abstract Version MinInputVersion { get; }

        /// <summary>
        /// Maximum input version (normally exclusive, <see cref="MigrateMaxVersion"/>)
        /// </summary>
        public abstract Version MaxInputVersion { get; }

        /// <summary>
        /// If true, will run this migration even if current version == <see cref="MaxInputVersion"/>
        /// </summary>
        public virtual bool MigrateMaxVersion => false;

        /// <summary>
        /// Override this to handle your migration
        /// </summary>
        /// <remarks>
        /// <para>Always set LastMigratedVersion to this migration's result version</para>
        /// <para>It is allowable to modify the input object and return the same object</para>
        /// </remarks>
        public abstract JObject Migrate(JObject inputObject);
    }

    /// <summary>
    /// Generic base class for migrations. Derive from this class.
    /// </summary>
    public abstract class Migration<T> : Migration where T : IMigratable
    {
         
    }
}
