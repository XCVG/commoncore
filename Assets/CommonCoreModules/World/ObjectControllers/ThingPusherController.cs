using CommonCore.LockPause;
using CommonCore.ObjectActions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{
    /// <summary>
    /// Entity that pushes an object along a path of NavigationNodes
    /// </summary>
    /// <remarks>
    /// <para>Note that while this can be set up with a RestorableComponent to restore state, weird stuff will happen if the target object does not also restore its state somehow</para>
    /// </remarks>
    public class ThingPusherController : BaseController
    {
        [Header("Thing Pusher")]
        public Transform TargetObject;
        public string TargetObjectName;
        public NavigationNode StartNode;
        public NavigationNode TargetNode;
        public LoopOption Loop = LoopOption.Auto;
        [Tooltip("EXPERIMENTAL, activate NavigationNodeReachedTriggers when hit")]
        public bool ActivateTriggers = false;

        [Header("Dynamics")]
        public float ArriveThreshold = 1f;
        public float Velocity = 5f;
        [Tooltip("If set to -1, will snap to movement vector, if set to 0, will not rotate to align")]
        public float RotationSpeed = -1f;
        public bool Use3DMovement = false;

        private PusherState State = PusherState.Idle;

        public override void Start()
        {
            base.Start();

            if (TargetObject != null && !string.IsNullOrEmpty(TargetObjectName))
            {
                Debug.LogWarning($"[{nameof(ThingPusherController)}] TargetObjectName is set, but TargetObject is also set. TargetObject will be used.");
            }
            else if (TargetObject == null)
            {
                TargetObject = WorldUtils.FindObjectByTID(TargetObjectName).Ref()?.transform;
            }

            if (State == PusherState.Idle)
                State = PusherState.Waiting;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if (State == PusherState.Started || State == PusherState.Starting)
            {
                State = PusherState.Waiting;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (State == PusherState.Started && TargetObject != null)
            {
                StopFollowingPath();
            }
        }

        public override void Update()
        {
            base.Update();

            if (LockPauseModule.IsPaused())
                return;

            switch (State)
            {
                case PusherState.Waiting:
                    State = PusherState.Starting;
                    break;
                case PusherState.Starting:
                    StartFollowingPath();
                    break;
                case PusherState.Started:
                    FollowPath();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Starts or restarts following, optionally resetting parameters
        /// </summary>
        public void Restart(Transform targetObject, NavigationNode startNode, NavigationNode targetNode)
        {
            if (targetObject != null)
                TargetObject = targetObject;

            if (startNode != null)
                StartNode = startNode;

            if (targetNode != null)
                TargetNode = targetNode;

            State = PusherState.Waiting;
        }

        private void StartFollowingPath()
        {
            if (TargetObject == null && !string.IsNullOrEmpty(TargetObjectName))
            {
                TargetObject = WorldUtils.FindObjectByTID(TargetObjectName).Ref()?.transform;
            }

            if (TargetObject == null || StartNode == null)
            {
                Debug.LogError($"[{nameof(ThingPusherController)}] Missing TargetObject or StartNode, cannot start following path");

                State = PusherState.Stopped;
                return;
            }

            if (TargetNode == null)
            {
                TargetNode = StartNode;
            }

            if(RotationSpeed < 0)
            {
                var vecToTarget = (TargetNode.transform.position - TargetObject.position);
                if (!Use3DMovement)
                    vecToTarget = vecToTarget.GetFlatVector().GetSpaceVector();
                TargetObject.transform.forward = vecToTarget.normalized;
            }

            State = PusherState.Started;
        }

        private void FollowPath()
        {
            if (TargetObject == null)
            {
                Debug.LogWarning($"[{nameof(ThingPusherController)}] TargetObject has gone null, cannot continue following path");

                State = PusherState.Stopped;
                return;
            }

            if (!TargetObject.gameObject.activeInHierarchy)
                return;

            float threshold = Mathf.Min(ArriveThreshold, TargetNode.DistanceThreshold);

            Vector3 vecToTarget = (TargetNode.transform.position - TargetObject.position);
            if (!Use3DMovement)
                vecToTarget = vecToTarget.GetFlatVector().GetSpaceVector();
            float distToTarget = vecToTarget.magnitude;

            if (distToTarget <= threshold)
            {
                var trigger = TargetNode.GetComponent<NavigationNodeReachedTrigger>();
                if (ActivateTriggers && trigger != null)
                {
                    trigger.Activate(this);
                }

                if (TargetNode.EndNode || TargetNode.NextNode == null || TargetNode.NextNode == StartNode)
                {
                    //if it is an end node (either explicitly labelled, nextNode = null or nextNode = startNode)
                    //loop or don't loop based on settings
                    if (Loop == LoopOption.Never)
                    {
                        TargetNode = null;
                    }
                    else if (TargetNode.NextNode == null && Loop == LoopOption.Always)
                    {
                        TargetNode = StartNode;
                    }
                    else
                    {
                        TargetNode = TargetNode.NextNode;
                    }

                }
                else
                {
                    TargetNode = TargetNode.NextNode;
                }

                if (TargetNode != null)
                {
                    if (RotationSpeed < 0)
                    {
                        TargetObject.transform.forward = vecToTarget.normalized;
                    }
                }
                else
                {
                    StopFollowingPath();
                }

            }
            else
            {
                //move!
                Vector3 dir = vecToTarget.normalized;
                Vector3 moveVec = dir * Mathf.Min(distToTarget, Time.deltaTime * Velocity);
                TargetObject.Translate(moveVec, Space.World);

                if (RotationSpeed > 0)
                {
                    var targetRotation = Quaternion.LookRotation(dir, Vector3.up);
                    TargetObject.rotation = Quaternion.RotateTowards(TargetObject.rotation, targetRotation, RotationSpeed * Time.deltaTime);
                }

            }
        }

        private void StopFollowingPath()
        {
            State = PusherState.Stopped;
        }

        public override Dictionary<string, object> CommitEntityData()
        {
            var data = base.CommitEntityData();

            data["ThingPusherTargetObject"] = TargetObject.Ref()?.name ?? TargetObjectName;
            data["ThingPusherStartNode"] = StartNode.name;
            data["ThingPusherTargetNode"] = TargetNode.Ref()?.name ?? "";
            data["ThingPusherLoop"] = (int)Loop;
            data["ThingPusherState"] = (int)State;

            return data;
        }

        public override void RestoreEntityData(Dictionary<string, object> data)
        {
            base.RestoreEntityData(data);

            State = (PusherState)Convert.ToInt32(data["ThingPusherState"]);
            Loop = (LoopOption)Convert.ToInt32(data["ThingPusherLoop"]);

            if (StartNode == null || (!string.IsNullOrEmpty(data["ThingPusherStartNode"] as string) && StartNode.name != (string)data["ThingPusherStartNode"]))
                StartNode = FindNode((string)data["ThingPusherStartNode"]);
            if (TargetNode == null || (!string.IsNullOrEmpty(data["ThingPusherTargetNode"] as string) && TargetNode.name != (string)data["ThingPusherTargetNode"]))
                TargetNode = FindNode((string)data["ThingPusherTargetNode"]);

            if (TargetObject == null || (!string.IsNullOrEmpty(data["ThingPusherTargetObject"] as string) && TargetObject.name != (string)data["ThingPusherTargetObject"]))
            {
                TargetObjectName = (string)data["ThingPusherTargetObject"];

                TargetObject = WorldUtils.FindObjectByTID(TargetObjectName).Ref()?.transform;
            }
        }

        private NavigationNode FindNode(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (StartNode != null)
            {
                NavigationNode node = StartNode;
                while (node.gameObject.name != name && node.NextNode != null)
                {
                    node = node.NextNode;
                }
                if (node.gameObject.name == name)
                    return node;
            }

            var transforms = SceneUtils.FindDeepChildren(CoreUtils.GetWorldRoot(), name);
            foreach (var transform in transforms)
            {
                var node = transform.GetComponent<NavigationNode>();
                if (node != null)
                    return node;
            }

            return null;
        }

        private enum PusherState
        {
            Idle, Waiting, Starting, Started, Stopped
        }

        [Serializable]
        public enum LoopOption
        {
            Auto, Always, Never
        }
    }
}


