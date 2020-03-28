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
        private string FormId = null;
        [SerializeField]
        private Transform SpawnParent = null;
        [SerializeField]
        private bool OverrideTransform = false;
        [SerializeField]
        private Vector3 OverridePosition = Vector3.zero;
        [SerializeField]
        private Vector3 OverrideRotation = Vector3.zero;
        [SerializeField]
        private bool ActivateObject = true;
#pragma warning disable CS0649
        [SerializeField]
        private ObjectSpawnEvent OnSpawnEvent;
#pragma warning restore CS0649
        [SerializeField, Tooltip("If set, will also spawn this effect at the spawn position")]
        private string EffectId = null;

        private bool Locked;

        public override void Execute(ActionInvokerData data)
        {
            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
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