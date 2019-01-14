using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore;
using CommonCore.State;
using CommonCore.World;
using CommonCore.ObjectActions;
using CommonCore.RpgGame.World;

namespace World.Ext.TestIsland
{

    public class TestIslandWellController : MonoBehaviour
    {
        const string MonstersKilledKey = "TestIslandWellMonsters";

        public int MonstersToKill;

        public ActionSpecialEvent SpawnerSpecial;
        public ActionSpecial DeathSpecial;

        private int MonstersKilled;

        private void Start()
        {
            object monstersPrevious;
            if(WorldUtils.GetSceneController().LocalStore.TryGetValue(MonstersKilledKey, out monstersPrevious))
            {
                MonstersKilled = (int)monstersPrevious;
            }
        }

        public void OnEnterWellZone(ActionInvokerData data)
        {

            //don't do ANYTHING if the quest isn't started (dumb but I'm too tired to handle the more complex cases of not doing this)
            if (!GameState.Instance.CampaignState.IsQuestActive("DemoQuest"))
                return;

            //we're done if...
            if (GameState.Instance.CampaignState.HasFlag("DemoWellMonstersKilled") || GameState.Instance.CampaignState.HasFlag("DemoWellMonstersSpawned"))
                return;


            //otherwise, activate spawners!
            GameState.Instance.CampaignState.AddFlag("DemoWellMonstersSpawned");
            SpawnerSpecial.Invoke(new ActionInvokerData() { Activator = RpgWorldUtils.GetPlayerController() });

            //also set quest stage
            if (GameState.Instance.CampaignState.GetQuestStage("DemoQuest") < 50)
                GameState.Instance.CampaignState.SetQuestStage("DemoQuest", 50);
            
        }

        public void OnActorSpawn(GameObject actor, ActionSpecial spawner)
        {
            var ac = actor.GetComponent<ActorController>();
            ac.OnDeathSpecial = DeathSpecial;
        }

        public void OnMonsterKilled()
        {

            MonstersKilled++;

            if (MonstersKilled >= MonstersToKill)
            {
                GameState.Instance.CampaignState.AddFlag("DemoWellMonstersKilled");
                if (GameState.Instance.CampaignState.GetQuestStage("DemoQuest") < 60)
                    GameState.Instance.CampaignState.SetQuestStage("DemoQuest", 60);
            }                

            WorldUtils.GetSceneController().LocalStore[MonstersKilledKey] = MonstersKilled;

            Debug.Log(MonstersKilled + "monsters killed");
        }

    }
}