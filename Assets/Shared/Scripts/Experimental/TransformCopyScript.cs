using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Experimental
{

    /// <summary>
    /// Copies transform to another object, for when you dun goofed
    /// </summary>
    public class TransformCopyScript : MonoBehaviour
    {
        public Transform Target;
        public bool YAxisOnly = false;
        public bool CopyRotation = true;

        private void FixedUpdate()
        {
            if(Target != null)
            {
                if (YAxisOnly)
                    transform.position = new Vector3(transform.position.x, Target.position.y, transform.position.z);
                else
                    transform.position = Target.position;

                if(CopyRotation)
                    transform.rotation = Target.rotation;
            }
        }
    }
}