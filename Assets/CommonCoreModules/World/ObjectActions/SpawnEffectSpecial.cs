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
        private string EffectId = null;
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
#pragma warning disable CS0649 //I kinda don't want to default assign this
        [SerializeField]
        private ObjectSpawnEvent OnSpawnEvent;
#pragma warning restore CS0649

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

            var go = WorldUtils.SpawnEffect(EffectId, position, rotation, parent, true);

            go.SetActive(ActivateObject);

            OnSpawnEvent.Invoke(go, this);
        }
    }
}