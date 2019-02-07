using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore;
using CommonCore.State;
using CommonCore.World;
using CommonCore.Messaging;
using CommonCore.Audio;
using CommonCore.ObjectActions;

namespace World.Ext.TestIsland
{

    public class TestIslandJunkScript : MonoBehaviour
    {
        public string Flag;        
        public string GrantItem;

        public string SubtitleText;
        public string VoicePath;

        public string AltSubtitleText;
        public string AltVoicePath;

        // Use this for initialization
        void Start()
        {

        }

        public void InvokeExamine(ActionInvokerData data)
        {
            //hardcoded alt "not started" case
            if(!GameState.Instance.CampaignState.HasFlag("DemoMechanicStartedFetchQuest"))
            {
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Looks like a piece of shuttle debris.", 5.0f));
                AudioPlayer.Instance.PlaySound("demo/aurelia_junk", SoundType.Voice, false);

                return;
            }

            if(!string.IsNullOrEmpty(Flag) && SharedUtils.GetSceneController().LocalStore.ContainsKey(Flag))
            {
                //abort
                if(!string.IsNullOrEmpty(AltSubtitleText))
                    QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage(AltSubtitleText, 5.0f));
                if (!string.IsNullOrEmpty(AltVoicePath))
                    AudioPlayer.Instance.PlaySound(AltVoicePath, SoundType.Voice, false);

                return;
            }

            if(!string.IsNullOrEmpty(GrantItem))
            {
                GameState.Instance.PlayerRpgState.Inventory.AddItem(GrantItem, 1);
            }

            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage(SubtitleText, 5.0f));
            AudioPlayer.Instance.PlaySound(VoicePath, SoundType.Voice, false);

            if (!string.IsNullOrEmpty(Flag))
            {
                SharedUtils.GetSceneController().LocalStore.Add(Flag, true);
            }
        }
    }
}