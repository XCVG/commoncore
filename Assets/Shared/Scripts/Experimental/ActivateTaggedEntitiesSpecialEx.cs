using CommonCore.ObjectActions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.RpgGame.World;
using CommonCore.World;
using CommonCore;

namespace CommonCore.Experimental
{

    /// <summary>
    /// Activates all entities with tag
    /// </summary>
    public class ActivateTaggedEntitiesSpecialEx : ActionSpecial
    {
        public string Tag;
        public bool UnlockActorState;
        public bool DeactivateIfActive;

        private bool Locked = false;

        public override void Execute(ActionInvokerData data)
        {
            if (Locked)
                return;

            var entities = WorldUtils.FindEntitiesWithTag(Tag);
            foreach(var entity in entities)
            {
                if(entity.gameObject.activeSelf && DeactivateIfActive)
                {
                    entity.gameObject.SetActive(false);
                    continue;
                }    
                entity.gameObject.SetActive(true);
                if(entity is ActorController ac && UnlockActorState)
                {
                    ac.LockAiState = false;
                    if(ac.CurrentAiState != ActorAiState.Dead)
                        ac.EnterState(ac.BaseAiState);
                }
            }

            if (!Repeatable)
                Locked = true;
        }
    }
}