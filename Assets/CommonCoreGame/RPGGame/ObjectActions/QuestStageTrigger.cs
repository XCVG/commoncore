﻿using CommonCore.ObjectActions;
using CommonCore.State;
using UnityEngine;

namespace CommonCore.RpgGame.ObjectActions
{

    /// <summary>
    /// Triggers an action on a quest stage
    /// </summary>
    public class QuestStageTrigger : ActionTrigger
    {
        [Header("Quest Stage Trigger Options")]
        public string Quest;
        public int Stage = 0;
        public QuestStageTriggerType Comparison = QuestStageTriggerType.Equal;

        public bool CheckContinuous;

        void Start()
        {
            if (Persistent)
            {
                RestoreState();
            }
            CheckTrigger();
        }

        void Update()
        {
            if (CheckContinuous)
                CheckTrigger();
        }

        private void CheckTrigger()
        {
            if (Triggered)
                return;

            if (CheckCondition())
            {
                ActionInvokerData d = new ActionInvokerData() { Caller = this };
                Special.Invoke(d);
                Triggered = true;

                if (Persistent)
                {
                    SaveState();
                }
            }
        }

        private bool CheckCondition()
        {
            int questStage = GameState.Instance.CampaignState.GetQuestStage(Quest);

            switch (Comparison)
            {
                case QuestStageTriggerType.Equal:
                    return questStage == Stage;
                case QuestStageTriggerType.GreaterOrEqual:
                    return questStage >= Stage;
                case QuestStageTriggerType.LessOrEqual:
                    return questStage <= Stage;
                case QuestStageTriggerType.Started:
                    return questStage > 0;
                case QuestStageTriggerType.Completed:
                    return questStage < 0;
                default:
                    return false;
            }
        }

        protected override string LookupName
        {
            get
            {
                return string.Format("{0}_{1}_{2}", gameObject.name, this.GetType().Name, "Triggered");
            }
        }

        
    }

    public enum QuestStageTriggerType
    {
        Equal, GreaterOrEqual, LessOrEqual, Started, Completed
    }
}