using CommonCore.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Experimental
{

    /// <summary>
    /// Assigns a facing sprite from a resource
    /// </summary>
    public class FacingSpriteAssignScript : MonoBehaviour
    {
        [SerializeField]
        private FacingSpriteComponent FacingSpriteComponent;
        [SerializeField]
        private string ResourcePath;

        private void Start()
        {
            FacingSpriteAsset fsa = CoreUtils.LoadResource<FacingSpriteAsset>(ResourcePath);

            if (fsa == null)
                throw new KeyNotFoundException($"Couldn't find a facing sprite for path \"{ResourcePath}\"");

            if (FacingSpriteComponent is StaticFacingSpriteComponent sfsc)
            {
                sfsc.Sprites = fsa;
            }
            else
            {
                throw new NotImplementedException("That type of FacingSpriteComponent is not supported");
            }
        }
    }
}