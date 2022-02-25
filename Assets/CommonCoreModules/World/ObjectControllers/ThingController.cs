using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{

    //ThingController: generic controller for generic things (phys props, superclass for inventory)
    public class ThingController : BaseController, IAmPushable
    {
        [Header("Thing Options"), SerializeField, Tooltip("If set, redirects IamPushable.Push to attached rigidbody")]
        private bool ThunkPhysicsToRigidbody = false;

        public virtual void Push(Vector3 impulse)
        {
            if (!ThunkPhysicsToRigidbody)
                return;

            var rb = GetComponent<Rigidbody>();

            if (rb == null)
                return;

            rb.AddForce(impulse, ForceMode.Impulse);
        }
    }
}
