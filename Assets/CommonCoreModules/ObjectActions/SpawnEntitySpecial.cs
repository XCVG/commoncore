using CommonCore.World;
using UnityEngine;

namespace CommonCore.ObjectActions
{

    /// <summary>
    /// Spawns an entity by name
    /// </summary>
    public class SpawnEntitySpecial : ActionSpecial
    {
        [SerializeField, Tooltip("The formal ID of the entity to spawn")]
        private string FormId;
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
        [SerializeField, Tooltip("If set, will also spawn this effect at the spawn position")]
        private string EffectId;

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