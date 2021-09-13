using CommonCore.ObjectActions;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.State;
using CommonCore.State;
using CommonCore.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.ObjectActions
{

    /// <summary>
    /// Conditional action special filter, only passes through if the conditional succeeds
    /// </summary>
    public class ConditionalFilter : ActionSpecial
    {
        public ActionSpecialEvent Special;

        [SerializeField, Tooltip("If set, will pass through when activated by non-player")]
        private bool PassthroughNonPlayerAction = false;
        [SerializeField, Tooltip("If set, will evaluate conditional when activated by non-player")]
        private bool EvaluateNonPlayerAction = false;
        [SerializeField]
        private EditorConditional Conditional = default;
        [SerializeField]
        private bool Consume = false;
        [SerializeField, Tooltip("If set, will disable non-repeatable action even if the condition failed.")]
        private bool LockEvenOnFail = false;

        private bool Locked;

        public override void Execute(ActionInvokerData data)
        {
            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
                return;

            if(EvaluateNonPlayerAction || (data.Activator != null && WorldUtils.IsPlayer(data.Activator.gameObject)))
            {
                if (Conditional.Parse().Evaluate())
                {
                    Special.Invoke(data);
                }

                if (!Repeatable)
                    Locked = true;
            }
            else if (PassthroughNonPlayerAction)
            {
                Special.Invoke(data);

                if (!Repeatable) //should we do this?
                    Locked = true;
            }

            if (!Repeatable && LockEvenOnFail)
                Locked = true;
        }
    }
}