using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// Parameters for shell/casing ejection. Near-POD class, does nothing without a RangedWeaponViewModelScript to go with it.
    /// </summary>
    public class ShellEjectionComponent : MonoBehaviour
    {
        //the rotation and position of this object (the ejection point) will be used for the shell
        //the forward transform of the first child of the ejection point will be used as the ejection direction

        public float ShellScale = 1;
        public float ShellVelocity = 1;
        public float ShellTorque = 1;

        //these will be multiplied by rnd(-1,1) and added on
        public float ShellRandomVelocity = 0;
        public float ShellRandomTorque = 0;

    }
}