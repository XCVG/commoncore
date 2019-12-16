using CommonCore.ObjectActions;
using CommonCore.World;
using System;
using UnityEngine;

namespace CommonCore.RpgGame.ObjectActions
{

    /// <summary>
    /// Spawns an actor with various options
    /// </summary>
    /// <remarks>Obsolete. Use SpawnEntitySpecial instead.</remarks>
    [Obsolete("Use SpawnEntitySpecial instead", false)]
    public class SpawnActorSpecial : ActionSpecial
    {
        public string FormId;
        public Transform SpawnParent;
        public bool OverrideTransform;
        public Vector3 OverridePosition;
        public Vector3 OverrideRotation;
        public bool ActivateObject = true;
        public ObjectSpawnEvent OnSpawnEvent;

        public string EffectId;

        private bool Locked;

        public override void Execute(ActionInvokerData data)
        {
            if (Locked)
                return;

            SpawnObjectEx();

            if (!Repeatable)
                Locked = true;
        }

        protected void SpawnObjectEx()
        {
            Transform parent;
            if (SpawnParent == null)
            {
                parent = CoreUtils.GetWorldRoot();
            }
            else
                parent = SpawnParent;

            Vector3 position = OverrideTransform ? OverridePosition : transform.position;
            Vector3 rotation = OverrideTransform ? OverrideRotation : transform.eulerAngles;

            var go = WorldUtils.SpawnEntity(FormId, null, position, rotation, parent);

            go.SetActive(ActivateObject);

            if (!string.IsNullOrEmpty(EffectId))
                WorldUtils.SpawnEffect(EffectId, position, rotation, parent);

            OnSpawnEvent.Invoke(go, this);
        }
    }
}