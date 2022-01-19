using CommonCore.State;
using CommonCore.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.ObjectActions
{
    /// <summary>
    /// Activator action special filter, only passes through if the activator fulfills certain conditions
    /// </summary>
    /// <remarks>
    /// <para>Probably should have been in CommonCore.Experimental</para>
    /// </remarks>
    public class ActivatorFilter : ActionSpecial
    {
        public ActionSpecialEvent Special;
        
        [SerializeField, Tooltip("If set, player will always be allowed to activate, overriding all conditions except if-alive check")]
        private bool AlwaysAllowPlayer = false;
        [SerializeField, Tooltip("If set, checks if the activator is an actor and fails if it is not")]
        private bool CheckIfActor = false;
        [SerializeField, Tooltip("If set, checks if the activator is considered alive")]
        private bool CheckIfAlive = false;
        [SerializeField, Tooltip("If set, always allows null activator, otherwise always rejects")]
        private bool AllowNullActivator = false;
        [SerializeField, Tooltip("If list is non-empty, reject all activators not in the list")]
        private string[] AllowedTIDs = null;
        [SerializeField, Tooltip("If set, will disable non-repeatable action even if the condition failed.")]
        private bool LockEvenOnFail = false;

        private bool Locked;        

        public override void Execute(ActionInvokerData data)
        {
            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
                return;

            if (data.Activator != null)
            {
                if(AlwaysAllowPlayer && WorldUtils.IsPlayer(data.Activator.gameObject) && (!CheckIfAlive || WorldUtils.IsEntityAlive(data.Activator)))
                    FinishExecute(data);

                if(!CheckIfActor || (WorldUtils.IsActor(data.Activator.gameObject)))
                {
                    if ((!CheckIfAlive || WorldUtils.IsEntityAlive(data.Activator)))
                    {
                        if(CheckIfAllowedTID(data))
                            FinishExecute(data);
                    }
                        
                }
            }
            else
            {
                if (AllowNullActivator)
                    FinishExecute(data);
            }

            if (!Repeatable && LockEvenOnFail)
                Locked = true;
        }

        private void FinishExecute(ActionInvokerData data)
        {
            Special.Invoke(data);

            if (!Repeatable)
                Locked = true;
        }

        private bool CheckIfAllowedTID(ActionInvokerData data)
        {
            if (AllowedTIDs == null || AllowedTIDs.Length == 0)
                return true;

            var name = data.Activator.gameObject.name;

            foreach (string tid in AllowedTIDs)
            {
                if (name == tid)
                    return true;
            }

            return false;
        }
    }
}