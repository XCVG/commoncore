using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.World;
using CommonCore.RpgGame.Rpg;

namespace CommonCore.RpgGame.World
{
    public enum ActorInteractionType
    {
        None, Special, AmbientMonologue, Dialogue, Script
    }

    public enum ActorAnimState
    {
        Idle, Dead, Dying, Walking, Running, Hurting, Talking, Shooting, Punching, Pickup
    }

    public enum ActorAiState
    {
        Idle, Dead, Wandering, Chasing, Hurting, Attacking, Covering, Fleeing, ScriptedMoveTo, ScriptedAction
    }

    public enum ActorBodyPart
    {
        Unspecified, Torso, Head, LeftArm, LeftLeg, RightArm, RightLeg, Tail
    }

    public enum ActorDifficultyHandling
    {
        None,
        /// <summary>
        /// Apply Actor* difficulty scaling
        /// </summary>
        AsActor,
        /// <summary>
        /// Apply Follower* and Actor* difficulty scaling
        /// </summary>
        AsFollower,
        /// <summary>
        /// Apply only Actor* difficult scaling
        /// </summary>
        AsFollowerOnly
    }

    //this is in flux, we may change what data we store in the future
    public class ActorExtraData
    {
        //state information (very important, so we save most of it)
        public ActorAiState CurrentAiState { get; set; }
        public ActorAiState LastAiState { get; set; }
        public bool LockAiState { get; set; }
        public ActorAnimState CurrentAnimState { get; set; }
        public bool LockAnimState { get; set; }
        public string SavedTarget { get; set; }
        public Vector3 AltTarget { get; set; }
        public float TimeInState { get; set; }

        //health 
        public float Health { get; set; }
        public bool BeenHit { get; set; }

        //navigation
        public bool IsRunning { get; set; }

        //interaction
        public bool InteractionForceDisabled { get; set; }

        //extended hit info
        public ActorHitInfo? LastHit { get; set; }
        public float LastHitDamage { get; set; }
        public bool WasExtremeDeath { get; set; }

        public ActorExtraData()
        {

        }
    }

    public struct ActorDamageHandlerResult
    {
        public ActorHitInfo? HitInfo; //if this is null, we consider damage fully handled and exit immediately
        public float? DamageTaken; //if this has a value, bypass default dt/dr, location etc handling and apply immediately
        public bool? ExtremeDeath; //if this has a value, bypass default extreme death handling and use this
        public bool? TookPain; //if this has a value, bypass default pain handling and use this
    }

    public struct DeathStateActorAnimationArgs
    {
        public bool ExtremeDeath;
        public int DamageType;
        public int DamageEffector;
        public int HitLocation;
        public int HitMaterial;
    }

    /// <summary>
    /// Interface representing a component on an object that receives actor entity event calls
    /// </summary>
    public interface IReceiveActorEntityEvents : IReceiveDamageableEntityEvents
    {
        void ChangeState(ActorAiState oldState, ActorAiState newState);
    }
}