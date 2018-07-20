using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Rpg; //THIS is the interdependency we're trying to avoid

namespace CommonCore.World
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
        Idle, Dead, Wandering, Chasing, Hurting, Attacking, Covering, Fleeing
    }

    public enum ActorBodyPart
    {
        Unspecified, Torso, Head, LeftArm, LeftLeg, RightArm, RightLeg
    }

    [System.Serializable]
    public struct ActorHitInfo
    {
        public float Damage;
        public float DamagePierce;
        public DamageType DType;
        public ActorBodyPart HitLocation;
        public BaseController Originator;

        public ActorHitInfo(float damage, float damagePierce, DamageType dtype, ActorBodyPart hitlocation, BaseController originator)
        {
            Damage = damage;
            DamagePierce = damagePierce;
            DType = dtype;
            HitLocation = hitlocation;
            Originator = originator;
        }
    }
}