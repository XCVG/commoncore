﻿using CommonCore.RpgGame.World;
using CommonCore.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Experimental
{


    /// <summary>
    /// Gross hack that "tethers" a follower to the player
    /// </summary>
    [RequireComponent(typeof(ActorController))]
    public class ActorFollowerTetherEx : MonoBehaviour
    {

        [SerializeField]
        private ActorController ActorController = null;

        public BaseController TetherTarget = null;
        public bool TetherToPlayer = true;

        [SerializeField]
        private float MaxDistance = 25f;
        [SerializeField]
        private float MinDistance = 15f;
        [SerializeField]
        private float FollowEndDistance = 5f;


        private PlayerController PlayerController;

        private void Start()
        {
            if (ActorController == null)
                ActorController = GetComponent<ActorController>();
        }

        private void Update()
        {
            if(TetherToPlayer)
            {
                if (PlayerController == null)
                    PlayerController = RpgWorldUtils.GetPlayerController();

                if (PlayerController == null)
                    return; //can't tether if there's nothing to tether to

                TetherTarget = PlayerController;
            }

            if (TetherTarget == null)
                return;

            switch (ActorController.CurrentAiState)
            {
                case ActorAiState.Idle:
                case ActorAiState.Wandering:
                    //chase the player instead of remaining in either of these states
                    if ((ActorController.transform.position - TetherTarget.transform.position).magnitude > MinDistance && ActorController.Target == null)
                    {
                        UpdateMovementTarget();
                        ActorController.EnterState(ActorAiState.ScriptedMoveTo);
                    }                    
                    break;
                case ActorAiState.Chasing:
                    //break off the chase if too far away
                    if((ActorController.transform.position - TetherTarget.transform.position).magnitude > MaxDistance)
                    {
                        ActorController.Target = null;
                        UpdateMovementTarget();
                        ActorController.EnterState(ActorAiState.ScriptedMoveTo);
                    }
                    break;
                case ActorAiState.ScriptedMoveTo:
                    //update movement target
                    //return AI to normal if close enough to player
                    UpdateMovementTarget();
                    if ((ActorController.transform.position - TetherTarget.transform.position).magnitude < FollowEndDistance)
                    {
                        ActorController.AudioComponent.StopMoveSound(); //hopefully fixed
                        ActorController.EnterState(ActorController.BaseAiState);
                    }
                    break;
            }
        }

        private void UpdateMovementTarget()
        {
            var movementComponent = ActorController.MovementComponent;
            movementComponent.MovementTarget = TetherTarget.transform.position;
        }
    }
}