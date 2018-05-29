using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.State
{

    public abstract class RestorableData
    {

    }

    //dummy class for times when you don't actually want to store the data here
    public class BlankRestorableData : RestorableData
    {

    }

    public class DynamicRestorableData : RestorableData
    {
        //object properties
        public bool Active { get; set; }
        public bool Visible { get; set; }
        public string FormID { get; set; }
        public string[] Tags { get; set; }
        public string Scene { get; set; }       

        //transform properties
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }

        //rigidbody properties
        public bool IsKinematic { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 AngularVelocity { get; set; }
        public float Mass { get; set; }

        //extra data
        public Dictionary<string, System.Object> ExtraData { get; set; }

        public DynamicRestorableData()
        {
            ExtraData = new Dictionary<string, System.Object>();
        }
    }
}