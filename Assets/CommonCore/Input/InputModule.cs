using CommonCore.DebugLog;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Input
{
    /*
     * CommonCore Input Module
     * Initializes and manages mapped input system
     */
    public class InputModule : CCModule
    {

        //TODO integrate with LockPause and handle mouse capture here

        public InputModule()
        {
            //set null mapper
            MappedInput.SetMapper(new NullInputMapper());

            //attempt to set default (Unity) mapper
            try
            {
                MappedInput.SetMapper(new UnityInputMapper());
            }
            catch(Exception e)
            {
                CDebug.Log(e);
            }

            CDebug.Log("Input module loaded!");
        }

    }
}