using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore;
using CommonCore.ObjectActions;
using CommonCore.State;
using CommonCore.Rpg;
using CommonCore.World;
using CommonCore.Audio;
using CommonCore.Messaging;
using CommonCore.Dialogue;

namespace World.Ext.TestIsland
{

    public class TestIslandFilterScript : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            if (GameState.Instance.CampaignState.GetQuestStage("DemoQuest") == 210)
            {
                GetComponent<InteractableComponent>().enabled = true;
            }
        }


        public void InvokePlace(ActionInvokerData data)
        {
            if(GameState.Instance.CampaignState.GetQuestStage("DemoQuest") != 210)
            {
                return;
            }

            //place and invoke
            Transform spawnPoint = transform.Find("SpawnPoint");
            WorldUtils.SpawnObject("prop_filter", "FitlerObject", spawnPoint.position, spawnPoint.eulerAngles, null);
            GameState.Instance.CampaignState.SetQuestStage("DemoQuest", 220);
            GameState.Instance.PlayerRpgState.Inventory.RemoveItem("demo_filter", 1);

            StartCoroutine(FilterPlacedCoroutine());
        }

        IEnumerator FilterPlacedCoroutine()
        {
            //TODO: face WaterLady and initiate conversation
            var playerTransform = WorldUtils.GetPlayerObject().transform;
            var npcTransform = WorldUtils.FindObjectByTID("WaterLady").transform;
            playerTransform.forward = CoreUtils.GetFlatVectorToTarget(playerTransform.position, npcTransform.position).normalized;

            yield return new WaitForSeconds(0.5f);

            DialogueInitiator.InitiateDialogue("DemoFlavia", true, null);

            yield return null;
        }


    }
}