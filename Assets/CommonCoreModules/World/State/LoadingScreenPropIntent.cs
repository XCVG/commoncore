using UnityEngine;
using CommonCore.State;

namespace CommonCore.World
{

    /// <summary>
    /// Intent that sets the prop override on the loading screen
    /// </summary>
    public class LoadingScreenPropIntent : Intent
    {
        public string PropName { get; private set; }


        public LoadingScreenPropIntent(string propName)
        {
            PropName = propName;
        }

        public override void LoadingExecute()
        {
            Valid = false;
        }
    }
}