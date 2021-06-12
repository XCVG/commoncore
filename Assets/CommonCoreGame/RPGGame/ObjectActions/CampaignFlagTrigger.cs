using CommonCore.ObjectActions;
using CommonCore.State;
using UnityEngine;

namespace CommonCore.RpgGame.ObjectActions
{
    /// <summary>
    /// Triggers an action on a campaign flag
    /// </summary>
    public class CampaignFlagTrigger : ActionTrigger
    {
        [Header("Campaign Flag Trigger Options")]
        public string Flag;
        public bool CheckContinuous;
        public bool InvertBehaviour;

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

            if(GameState.Instance.CampaignState.HasFlag(Flag))
            {
                ActionInvokerData d = new ActionInvokerData() { Caller = this };
                Special.Invoke(d);
                Triggered = true;

                if(Persistent)
                {
                    SaveState();
                }
            }
        }

        protected override string LookupName { get
            {
                return string.Format("{0}_{1}_{2}", gameObject.name, this.GetType().Name, "Triggered");
            }
        }

    }
}