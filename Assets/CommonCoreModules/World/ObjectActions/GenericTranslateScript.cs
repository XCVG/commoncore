using UnityEngine;
using System.Collections;

namespace CommonCore.ObjectActions
{

    public class GenericTranslateScript : MonoBehaviour
    {
        public Vector3 Velocity = Vector3.zero;
        public bool Relative = false;

        void FixedUpdate()
        {
            transform.Translate(Velocity * Time.fixedDeltaTime, Relative ? Space.Self : Space.World);
        }
    }
}