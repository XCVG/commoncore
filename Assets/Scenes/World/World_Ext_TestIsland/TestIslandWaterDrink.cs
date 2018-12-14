using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.ObjectActions;
using CommonCore.State;
using CommonCore.Rpg;
using CommonCore.World;
using CommonCore.Audio;
using CommonCore.Messaging;

namespace World.Ext.TestIsland
{
    public class TestIslandWaterDrink : MonoBehaviour
    {
        
        void Start()
        {
            if(GameState.Instance.CampaignState.IsQuestStarted("DemoQuest"))
            {
                GetComponent<InteractableComponent>().enabled = false;
            }
        }

        void Update()
        {

        }

        public void InvokeDrink(ActionInvokerData data)
        {
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Ew! This water is disgusting!", 5.0f));
            AudioPlayer.Instance.PlaySound("demo/aurelia_drink", SoundType.Voice, false);
            GameState.Instance.CampaignState.StartQuest("DemoQuest");
            GetComponent<InteractableComponent>().enabled = false;
        }

    }
}