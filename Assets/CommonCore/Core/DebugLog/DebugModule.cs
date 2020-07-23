using UnityEngine;
using CommonCore.Config;

namespace CommonCore.DebugLog
{
    /// <summary>
    /// CommonCore Debug/Log Module. Provides some debug logging/misc services.
    /// </summary>
    [CCExplicitModule]
    public class DebugModule : CCModule
    {
        public DebugModule()
        {
            FPSCounter.Initialize();
        }

        public override void OnFrameUpdate()
        {
            if (!CCBase.Initialized)
                return;

            if(ConfigState.Instance.ScreenshotKey != default)
            {
                if(UnityEngine.Input.GetKeyDown(ConfigState.Instance.ScreenshotKey))
                {
                    DebugUtils.TakeScreenshot();
                }
            }
        }

    }
}