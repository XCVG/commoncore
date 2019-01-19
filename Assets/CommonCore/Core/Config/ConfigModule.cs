using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Messaging;

namespace CommonCore.Config
{
    /*
     * CommonCore Config Module
     * Provides settings save/load/apply, as well as maintaining PersistState
     */
    [CCExplicitModule]
    public class ConfigModule : CCModule
    {
        private static ConfigModule Instance;

        public ConfigModule()
        {
            Instance = this;

            ConfigState.Load();
            ConfigState.Save();
        }

        /// <summary>
        /// Apply the current ConfigState configuration to the game
        /// </summary>
        public void ApplyConfiguration()
        {
            //AUDIO CONFIG
            AudioListener.volume = ConfigState.Instance.SoundVolume;
            var ac = AudioSettings.GetConfiguration();
            ac.speakerMode = ConfigState.Instance.SpeakerMode;
            AudioSettings.Reset(ac);

            //VIDEO CONFIG
            if(QualitySettings.GetQualityLevel() >= QualitySettings.names.Length - 1) //only apply quality settings if set to "custom" in the launcher
            {
                QualitySettings.SetQualityLevel(ConfigState.Instance.QualityLevel, true);
                Application.targetFrameRate = ConfigState.Instance.MaxFrames;
                QualitySettings.vSyncCount = ConfigState.Instance.VsyncCount;
            }


            //INPUT CONFIG

            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("ConfigChanged"));

        }

        public static void Apply()
        {
            Instance.ApplyConfiguration();
        }

    }
}