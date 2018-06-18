using CommonCore.State;
using CommonCore.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.ObjectActions
{
    public class CampaignFlagTrigger : ActionTrigger
    {
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
                ActionInvokerData d = new ActionInvokerData();
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