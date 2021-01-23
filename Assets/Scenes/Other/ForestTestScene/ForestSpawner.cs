using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore;
using CommonCore.World;

namespace ForestTestScene
{

    public class ForestSpawner : MonoBehaviour
    {
        [SerializeField]
        private string TreePrefab = "prop_testtree";
        [SerializeField]
        private int Rows = 10;
        [SerializeField]
        private int Columns = 10;
        [SerializeField]
        private float Spacing = 1f;
        
        private void Start()
        {
            SpawnTrees();
        }

        private void SpawnTrees()
        {
            float halfWidth = Columns * Spacing / 2;
            float halfDepth = Rows * Spacing / 2;

            Vector3 startPos = transform.position + new Vector3(-halfWidth, 0, -halfDepth);

            for(int row = 0; row < Rows; row++)
            {
                for(int col = 0; col < Columns; col++)
                {
                    Vector3 pos = startPos + new Vector3(col * Spacing, 0, row * Spacing);
                    WorldUtils.SpawnEntity(TreePrefab, $"tree_{row}_{col}", pos, Quaternion.identity, null);
                }
            }
        }
        
    }
}