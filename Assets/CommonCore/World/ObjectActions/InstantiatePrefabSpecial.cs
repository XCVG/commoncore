using UnityEngine;
using System.Collections;
using System;

namespace CommonCore.ObjectActions
{
    public class InstantiatePrefabSpecial : ActionSpecial
    {
        public GameObject Prefab;
        public Transform SpawnParent;
        public bool OverrideTransform;
        public bool OverrideTransformRelative;
        public Vector3 OverridePosition;
        public Vector3 OverrideRotation;
        public Vector3 OverrideScale;
        public bool ActivateObject = true;
        public ObjectSpawnEvent OnSpawnEvent;

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
                parent = transform.root; //should grab WorldRoot
            }
            else
                parent = SpawnParent;

            GameObject go = Instantiate(Prefab, parent);
            if(OverrideTransform)
            {
                if(OverrideTransformRelative)
                {
                    go.transform.localPosition = OverridePosition;
                    go.transform.localEulerAngles = OverrideRotation;
                    go.transform.localScale = OverrideScale;
                }
                else
                {
                    go.transform.position = OverridePosition;
                    go.transform.eulerAngles = OverrideRotation;
                    go.transform.localScale = OverrideScale;
                }
            }
            else
            {
                go.transform.position = transform.position;
                go.transform.rotation = transform.rotation;
            }
            go.SetActive(ActivateObject);

            OnSpawnEvent.Invoke(go, this);
        }
    }
}