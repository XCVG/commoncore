using CommonCore.LockPause;
using CommonCore.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// Entity that pushes an Actor along a path of NavigationNodes
    /// </summary>
    public class PathFollowerController : BaseController
    {
        [Header("Path Follower")]
        public ActorController TargetActor;
        public string TargetActorName;
        public NavigationNode StartNode;
        public NavigationNode TargetNode;
        public LoopOption Loop = LoopOption.Auto;
        public float ArriveThreshold = 1f;

        private FollowerState State = FollowerState.Idle;

        public override void Start()
        {
            base.Start();

            if(TargetActor != null && !string.IsNullOrEmpty(TargetActorName))
            {
                Debug.LogWarning($"[{nameof(PathFollowerController)}] TargetActorName is set, but TargetActor is also set. TargetActor will be used.");
            }
            else if(TargetActor == null)
            {
                TargetActor = WorldUtils.FindEntityByTID(TargetActorName) as ActorController;
            }

            if (State == FollowerState.Idle)
                State = FollowerState.Waiting;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if(State == FollowerState.Started || State == FollowerState.Starting)
            {
                State = FollowerState.Waiting;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if(State == FollowerState.Started && TargetActor != null)
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
                case FollowerState.Waiting:
                    State = FollowerState.Starting;
                    break;
                case FollowerState.Starting:
                    StartFollowingPath();
                    break;
                case FollowerState.Started:
                    FollowPath();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Starts or restarts following, optionally resetting parameters
        /// </summary>
        public void Restart(ActorController targetActor, NavigationNode startNode, NavigationNode targetNode)
        {
            if(targetActor != null)
                TargetActor = targetActor;

            if (startNode != null)
                StartNode = startNode;

            if (targetNode != null)
                TargetNode = targetNode;

            State = FollowerState.Waiting;
        }

        private void StartFollowingPath()
        {
            if (TargetActor == null && !string.IsNullOrEmpty(TargetActorName))
            {
                TargetActor = WorldUtils.FindEntityByTID(TargetActorName) as ActorController;
            }

            if(TargetActor == null || StartNode == null)
            {
                Debug.LogError($"[{nameof(PathFollowerController)}] Missing TargetActor or StartNode, cannot start following path");

                State = FollowerState.Stopped;
                return;
            }

            if(TargetNode == null)
            {
                TargetNode = StartNode;
            }

            //setup and start actor moving
            TargetActor.ForceEnterState(ActorAiState.ScriptedMoveTo);
            TargetActor.Target = TargetNode.transform;
            TargetActor.MovementComponent.MovementTarget = TargetNode.transform.position;
            TargetActor.LockAiState = true;

            State = FollowerState.Started;
        }

        private void FollowPath()
        {
            if(TargetActor == null)
            {
                Debug.LogWarning($"[{nameof(PathFollowerController)}] TargetActor has gone null, cannot continue following path");

                State = FollowerState.Stopped;
                return;
            }

            if (!TargetActor.isActiveAndEnabled)
                return;

            float threshold = Mathf.Min(ArriveThreshold, TargetActor.MovementComponent.TargetThreshold, TargetNode.DistanceThreshold);

            if((TargetNode.transform.position - TargetActor.transform.position).GetFlatVector().magnitude <= threshold)
            {
                if(TargetNode.EndNode || TargetNode.NextNode == null || TargetNode.NextNode == StartNode)
                {
                    //if it is an end node (either explicitly labelled, nextNode = null or nextNode = startNode)
                    //loop or don't loop based on settings
                    if(Loop == LoopOption.Never)
                    {
                        TargetNode = null;
                    }
                    else if(TargetNode.NextNode == null && Loop == LoopOption.Always)
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

                if(TargetNode != null)
                {
                    TargetActor.ForceEnterState(ActorAiState.ScriptedMoveTo);
                    TargetActor.Target = TargetNode.transform;
                    TargetActor.MovementComponent.MovementTarget = TargetNode.transform.position;
                    TargetActor.LockAiState = true;
                }
                else
                {
                    StopFollowingPath();
                }

            }
        }

        private void StopFollowingPath()
        {
            if(TargetActor != null)
            {
                TargetActor.Target = null;
                TargetActor.LockAiState = false;
                TargetActor.EnterState(ActorAiState.Idle);
            }                       

            State = FollowerState.Stopped;
        }

        public override Dictionary<string, object> CommitEntityData()
        {
            var data = base.CommitEntityData();

            data["PathFollowerTargetActor"] = TargetActor.Ref()?.name ?? TargetActorName;
            data["PathFollowerStartNode"] = StartNode.name;
            data["PathFollowerTargetNode"] = TargetNode.Ref()?.name ?? "";
            data["PathFollowerLoop"] = (int)Loop;
            data["PathFollowerState"] = (int)State;

            return data;
        }

        public override void RestoreEntityData(Dictionary<string, object> data)
        {
            base.RestoreEntityData(data);

            State = (FollowerState)Convert.ToInt32(data["PathFollowerState"]);
            Loop = (LoopOption)Convert.ToInt32(data["PathFollowerLoop"]);

            if (StartNode == null || (!string.IsNullOrEmpty(data["PathFollowerStartNode"] as string) && StartNode.name != (string)data["PathFollowerStartNode"]))
                StartNode = FindNode((string)data["PathFollowerStartNode"]);
            if (TargetNode == null || (!string.IsNullOrEmpty(data["PathFollowerTargetNode"] as string) && TargetNode.name != (string)data["PathFollowerTargetNode"]))
                TargetNode = FindNode((string)data["PathFollowerTargetNode"]);

            if(TargetActor == null || (!string.IsNullOrEmpty(data["PathFollowerTargetActor"] as string) && TargetActor.name != (string)data["PathFollowerTargetActor"]))
            {
                TargetActorName = (string)data["PathFollowerTargetActor"];

                TargetActor = WorldUtils.FindEntityByTID(TargetActorName) as ActorController;
            }
        }

        private NavigationNode FindNode(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var transforms = SceneUtils.FindDeepChildren(CoreUtils.GetWorldRoot(), name);
            foreach(var transform in transforms)
            {
                var node = transform.GetComponent<NavigationNode>();
                if (node != null)
                    return node;
            }

            return null;
        }

        private enum FollowerState
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