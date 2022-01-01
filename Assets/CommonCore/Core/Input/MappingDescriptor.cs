using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.UI;
using Humanizer;
using System.Linq;
using System.Collections.Immutable;

namespace CommonCore.Input
{
    /// <summary>
    /// Describes an axis or button mapping
    /// </summary>
    public class MappingDescriptor
    {
        public IReadOnlyList<SingleMappingDescriptor> Mappings { get; protected set; }

        public MappingDescriptor()
        {
            Mappings = ImmutableArray.Create<SingleMappingDescriptor>();
        }

        public MappingDescriptor(IEnumerable<SingleMappingDescriptor> mappings)
        {
            Mappings = mappings.ToList();
        }

        //convenience methods
        public string GetShortPrimaryMapping()
        {
            if (Mappings.Count > 0)
                return Mappings[0].ShortName;
            return string.Empty;
        }

        public string GetLongPrimaryMapping()
        {
            if (Mappings.Count > 0)
                return Mappings[0].LongName;
            return string.Empty;
        }

        public string GetShortMappings()
        {
            return Mappings.Humanize(m => m.ShortName, "or");
        }

        public string GetLongMappings()
        {
            return Mappings.Humanize(m => m.LongName, "or");
        }
    }

    /// <summary>
    /// Describes a single axis or button mapping
    /// </summary>
    public class SingleMappingDescriptor
    {
        public string Id { get; protected set; }
        public virtual string ShortName { get; protected set; }
        public virtual string LongName { get; protected set; }

        public SingleMappingDescriptor(string id, string shortName, string longName)
        {
            Id = id;
            ShortName = shortName;
            LongName = longName;
        }
    }

}