using CommonCore.Config;
using CommonCore.DelayedEvents;
using CommonCore.Scripting;
using CommonCore.State;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Linq;

namespace CommonCore.RpgGame.State
{

    //not a real module, YET
    public class ConditionalModule
    {
        //we will remove this ugly instance handling once it's a real module
        public static ConditionalModule Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ConditionalModule();

                return _Instance;
            }
        }

        private static ConditionalModule _Instance;

        private List<Type> ConditionalResolvers = new List<Type>();
        private List<Type> MicroscriptResolvers = new List<Type>();

        public void LoadBaseHandlers()
        {
            LoadHandlers(CCBase.BaseGameTypes);
        }

        public void LoadHandlersFromAddon(AddonLoadData data)
        {
            LoadHandlers(data.LoadedAssemblies.SelectMany(a => a.GetTypes()));
        }

        private void LoadHandlers(IEnumerable<Type> types)
        {
            var conditionalResolverTypes = types
                .Where(t => typeof(ConditionalResolver).IsAssignableFrom(t));
            ConditionalResolvers.AddRange(conditionalResolverTypes);

            var microscriptResolverTypes = types
                .Where(t => typeof(MicroscriptResolver).IsAssignableFrom(t));
            MicroscriptResolvers.AddRange(microscriptResolverTypes);
        }

        public ConditionalResolver GetResolverFor(Conditional conditional)
        {
            foreach(Type t in ((IEnumerable<Type>)ConditionalResolvers).Reverse())
            {
                try
                {
                    var resolver = (ConditionalResolver)Activator.CreateInstance(t, conditional);
                    if(resolver.CanResolve)
                        return resolver;
                }
                catch(Exception e)
                {
                    Debug.LogError($"[ConditionalModule] Failed to create resolver {(e.GetType().Name)}: {e.Message}");
                    if (ConfigState.Instance.UseVerboseLogging)
                        Debug.LogException(e);
                }
            }

            return null;
        }

        public MicroscriptResolver GetResolverFor(MicroscriptNode microscript)
        {
            foreach (Type t in ((IEnumerable<Type>)MicroscriptResolvers).Reverse())
            {
                try
                {
                    var resolver = (MicroscriptResolver)Activator.CreateInstance(t, microscript);
                    if (resolver.CanResolve)
                        return resolver;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ConditionalModule] Failed to create resolver {(e.GetType().Name)}: {e.Message}");
                    if (ConfigState.Instance.UseVerboseLogging)
                        Debug.LogException(e);
                }
            }

            return null;
        }
    }

}