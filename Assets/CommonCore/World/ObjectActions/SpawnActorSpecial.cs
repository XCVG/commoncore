using UnityEngine;
using System.Collections;
using System;
using CommonCore.World;

namespace CommonCore.ObjectActions
{
    public class SpawnActorSpecial : ActionSpecial
    {
        public string FormId;
        public Transform SpawnParent;
        public bool OverrideTransform;
        public Vector3 OverridePosition;
        public Vector3 OverrideRotation;
        public bool ActivateObject = true;

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
                parent = CCBaseUtil.GetWorldRoot();
            }
            else
                parent = SpawnParent;

            Vector3 position = OverrideTransform ? OverridePosition : transform.position;
            Vector3 rotation = OverrideTransform ? OverrideRotation : transform.eulerAngles;

            var go = WorldUtils.SpawnObject(FormId, null, position, rotation, parent);

            go.SetActive(ActivateObject);

            if (!string.IsNullOrEmpty(EffectId))
                WorldUtils.SpawnEffect(EffectId, position, rotation, parent);
        }
    }
}