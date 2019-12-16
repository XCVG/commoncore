using CommonCore.World;
using UnityEngine;

namespace CommonCore.ObjectActions
{

    /// <summary>
    /// Spawns an effect by name
    /// </summary>
    public class SpawnEffectSpecial : ActionSpecial
    {
        [SerializeField, Tooltip("The formal ID of the effect to spawn")]
        private string EffectId;
        [SerializeField]
        private Transform SpawnParent;
        [SerializeField]
        private bool OverrideTransform;
        [SerializeField]
        private Vector3 OverridePosition;
        [SerializeField]
        private Vector3 OverrideRotation;
        [SerializeField]
        private bool ActivateObject = true;
        [SerializeField]
        private ObjectSpawnEvent OnSpawnEvent;

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

            var go = WorldUtils.SpawnEffect(EffectId, position, rotation, parent);

            go.SetActive(ActivateObject);

            OnSpawnEvent.Invoke(go, this);
        }
    }
}