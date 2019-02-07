using CommonCore.Config;
using CommonCore.DebugLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonCore.Input
{
    /*
     * CommonCore Input Module
     * Initializes and manages mapped input system
     */
    public class InputModule : CCModule
    {

        //TODO integrate with LockPause and handle mouse capture here?

        public InputModule()
        {
            //set null mapper
            MappedInput.SetMapper(new NullInputMapper());

            //find all mappers and set them in MappedInput
            var mapperTypes = CCBase.BaseGameTypes
                .Where(t => typeof(InputMapper).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .Where(t => t.Name != "NullInputMapper")
                .ToArray();
            foreach(Type t in mapperTypes)
            {
                MappedInput.Mappers.Add(t.Name, t);
            }

            Log("Mappers: " + mapperTypes.ToNiceString());

            //attempt to set configured or default mapper
            try
            {
                if (!string.IsNullOrEmpty(ConfigState.Instance.InputMapper) && MappedInput.Mappers.ContainsKey(ConfigState.Instance.InputMapper))
                {
                    Log("Setting configured mapper " + ConfigState.Instance.InputMapper);
                    MappedInput.SetMapper(ConfigState.Instance.InputMapper);
                }
                else
                {
                    Log("Setting default mapper UnityInputMapper");
                    MappedInput.SetMapper(new UnityInputMapper());
                }
                
            }
            catch(Exception e)
            {
                LogError("Failed to load mapper!");
                LogException(e);
            }
        }

    }
}