using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExampleScene
{

    public class ExampleCubeRotate : MonoBehaviour
    {
        public Transform CubeObject;
        public float CubeRotateRate;

        private void Start()
        {
            if (CubeObject == null)
                CubeObject = transform;
            
        }

        private void Update()
        {
            CubeObject.Rotate(Vector3.up, CubeRotateRate * Time.deltaTime);
        }
    }
}