using CommonCore.DebugLog;
using CommonCore.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommonCore.World
{
    public abstract class DynamicRestorableComponent : RestorableComponent
    {
        public override RestorableData Save()
        {
            DynamicRestorableData data = new DynamicRestorableData();

            BaseController controller = GetComponent<BaseController>();
            if (!controller)
            {
                Debug.LogWarning("Object " + name + " has no controller!");
            }

            //save object properties
            data.Active = gameObject.activeSelf;
            data.Scene = SceneManager.GetActiveScene().name;
            if (controller)
            {
                data.Visible = controller.GetVisibility();
                data.Tags = new List<string>(controller.Tags).ToArray();
                data.FormID = controller.FormID;
            }
            else
            {
                data.Visible = true;
            }


            //save transform
            data.Position = transform.position;
            data.Rotation = transform.rotation;
            data.Scale = transform.localScale;

            //save rigidbody if existant
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            if (rigidbody)
            {
                data.Velocity = rigidbody.velocity;
                data.AngularVelocity = rigidbody.angularVelocity;
                data.Mass = rigidbody.mass;
                data.IsKinematic = rigidbody.isKinematic;
            }
            else
            {
                //eh, do nothing
            }

            //save extra data
            if (controller)
                data.ExtraData = controller.GetExtraData();

            return data;
        }

        public override void Restore(RestorableData rdata)
        {

            DynamicRestorableData data = rdata as DynamicRestorableData;

            if (data == null)
            {
                Debug.LogError("Object " + name + " has invalid data!");
                return;
            }

            BaseController controller = GetComponent<BaseController>();
            if (!controller)
            {
                Debug.LogWarning("Object " + name + " has no controller!");
            }

            //restore object properties
            gameObject.SetActive(data.Active);
            if (controller)
            {
                controller.SetVisibility(data.Visible);
                controller.Tags = new HashSet<string>(data.Tags);

                if (controller.FormID != data.FormID)
                    Debug.LogWarning(string.Format("Saved form ID does not match (saved:{0} , object: {1})", data.FormID, controller.FormID));
            }

            //restore transform
            transform.position = data.Position;
            transform.rotation = data.Rotation;
            transform.localScale = data.Scale;

            //restore rigidbody
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            if (rigidbody)
            {
                rigidbody.velocity = data.Velocity;
                rigidbody.angularVelocity = data.AngularVelocity;
                rigidbody.mass = data.Mass;
                rigidbody.isKinematic = data.IsKinematic;
            }
            else
            {
                //eh, do nothing
            }

            //restore extradata
            if (controller)
                controller.SetExtraData(data.ExtraData);

        }
    }
}
