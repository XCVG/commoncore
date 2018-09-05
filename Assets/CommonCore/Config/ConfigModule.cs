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
            PersistState.Load();
            ConfigState.Save();
            PersistState.Save();
            Debug.Log("Config module loaded!");
        }

        public void ApplyConfiguration()
        {
            //TODO apply configuration changes

            //AUDIO CONFIG
            AudioListener.volume = ConfigState.Instance.SoundVolume;
            var ac = AudioSettings.GetConfiguration();
            ac.speakerMode = ConfigState.Instance.SpeakerMode;
            AudioSettings.Reset(ac);

            //VIDEO CONFIG
            QualitySettings.SetQualityLevel(ConfigState.Instance.QualityLevel, true);

            //INPUT CONFIG

            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("ConfigChanged"));

        }

        public static void Apply()
        {
            Instance.ApplyConfiguration();
        }

    }
}