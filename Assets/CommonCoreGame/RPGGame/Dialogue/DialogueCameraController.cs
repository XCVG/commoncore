using CommonCore.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.Dialogue
{

    /// <summary>
    /// Controller for the dialogue camera
    /// </summary>
    public class DialogueCameraController : MonoBehaviour
    {
        [SerializeField]
        private Camera Camera = null;

        private void Start()
        {
            if (Camera == null)
                Camera = GetComponentInChildren<Camera>();
        }

        public void Deactivate()
        {
            Camera.gameObject.SetActive(false);
        }

        public void Activate(string targetString)
        {
            //decode string and point camera
            if(targetString.StartsWith("Player", StringComparison.OrdinalIgnoreCase))
            {
                //face player
                Transform playerTarget = WorldUtils.GetPlayerObject().transform;
                float distance = 1.0f;

                var dct = playerTarget.GetComponentInChildren<DialogueCameraTarget>();
                if (dct != null)
                {
                    playerTarget = dct.transform;

                    if(dct.DistanceOverride > 0)
                        distance = dct.DistanceOverride;
                }

                PointToTarget(playerTarget.position, playerTarget.forward, distance);

            }
            else if(targetString.StartsWith("FaceTarget", StringComparison.OrdinalIgnoreCase))
            {
                string tid;
                if (targetString.IndexOf(':') >= 0)
                    tid = targetString.Substring(targetString.IndexOf(':') + 1);
                else
                    tid = DialogueController.CurrentTarget; //"face current target"

                //face target
                Transform t = WorldUtils.FindObjectByTID(tid).transform;
                float distance = 1.0f;

                var dct = t.GetComponentInChildren<DialogueCameraTarget>();
                if (dct != null)
                {
                    t = dct.transform;

                    if (dct.DistanceOverride > 0)
                        distance = dct.DistanceOverride;
                }

                PointToTarget(t.position, t.forward, distance);
            }
            else if(targetString.StartsWith("CopyTransform", StringComparison.OrdinalIgnoreCase))
            {
                //copy transform
                string tid = targetString.Substring(targetString.IndexOf(':') + 1);
                Transform t = WorldUtils.FindObjectByTID(tid).transform;

                transform.position = t.position;
                transform.rotation = t.rotation;

            }
            else
            {
                throw new NotSupportedException();
            }

            Camera.gameObject.SetActive(true);
        }

        private void PointToTarget(Vector3 targetPosition, Vector3 targetForward, float distance)
        {
            Vector3 flatTargetForward = new Vector3(targetForward.x, 0, targetForward.z);
            Vector3 dirTargetToCamera = flatTargetForward.normalized;
            Vector3 cameraPos = targetPosition + dirTargetToCamera * distance;
            cameraPos.y = targetPosition.y;

            transform.position = cameraPos;
            transform.forward = -dirTargetToCamera;
        }

    }
}