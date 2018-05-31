using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World.Ext.MichaelCity
{

    public class CitySpawner : MonoBehaviour
    {
        public int rows = 100;
        public int cols = 100;
        public float offset = 10;
        public float buildingsize = 10;
        public bool UseRandomMaterial = true;

        public Material BaseMaterial;
        public GameObject CubePrefab;

        Transform root;

        void Start()
        {
            SpawnCubes();
        }

        void Update()
        {

        }

        private void SpawnCubes()
        {
            root = transform.root; //well that was pointful

            for (int row = 0; row < rows; row++)
            {
                float zpos = row * offset + row * buildingsize;

                for (int col = 0; col < cols; col++)
                {
                    float xpos = col * offset + col * buildingsize;

                    var go = Instantiate<GameObject>(CubePrefab, new Vector3(xpos, CubePrefab.transform.position.y, zpos), Quaternion.identity, root);
                    go.SetActive(true);

                    if(UseRandomMaterial)
                    {
                        Material mat = new Material(BaseMaterial);
                        mat.SetFloat("Smoothness", Random.Range(0, 1.0f));
                        go.GetComponent<Renderer>().material = mat;
                    }
                    go.name = string.Format("building_{0}_{1}", row, col);
                }
            }

        }
    }
}