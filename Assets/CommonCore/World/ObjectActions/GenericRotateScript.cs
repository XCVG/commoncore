using UnityEngine;
using System.Collections;

namespace CommonCore.ObjectActions
{

    public class GenericRotateScript : MonoBehaviour
    {
        public float RotationSpeed = 10.0f;
        public Vector3 RotationAxis = Vector3.one;

        void FixedUpdate()
        {
            transform.Rotate(RotationAxis, RotationSpeed * Time.fixedDeltaTime);
        }
    }
}