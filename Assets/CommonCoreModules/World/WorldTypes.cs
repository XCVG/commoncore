using CommonCore.Messaging;
using PseudoExtensibleEnum;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{

    /// <summary>
    /// Player view types, pretty self explanatory
    /// </summary>
    public enum PlayerViewType
    {
        PreferFirst, PreferThird, ForceFirst, ForceThird, ExplicitOther
    }

    /// <summary>
    /// Message signaling a request to shake the player view. Must be received by the player controller or somesuch.
    /// </summary>
    public class ViewShakeMessage : QdmsMessage
    {
        public readonly float Intensity;
        public readonly Vector3 Direction;
        public readonly float Time;
        public readonly int Priority;
        public readonly bool OverrideCurrent;

        public ViewShakeMessage(float intensity, Vector3 direction, float time, int priority, bool overrideCurrent)
        {
            Intensity = intensity;
            Direction = direction;
            Time = time;
            Priority = priority;
            OverrideCurrent = overrideCurrent;
        }
    }

    /// <summary>
    /// Message signaling a request to stop shaking the player view. Must be received by the player controller or somesuch.
    /// </summary>
    public class ViewShakeCancelMessage : QdmsMessage
    {

    }

    /// <summary>
    /// Message signaling a request to clear all actor targets. Must be received by all actors or things that can target
    /// </summary>
    public class ClearAllTargetsMessage : QdmsFlagMessage
    {
        public ClearAllTargetsMessage() : base("WorldClearAllTargets")
        {

        }
    }

    /// <summary>
    /// Data passed from GetClosestHit indicating what has been hit
    /// </summary>
    public readonly struct HitInfo
    {
        public readonly BaseController Controller;
        public readonly IHitboxComponent Hitbox;
        public readonly Collider HitCollider;
        public readonly Vector3 HitPoint;
        public readonly int HitLocation;
        public readonly int HitMaterial;

        public HitInfo(BaseController controller, IHitboxComponent hitbox, Collider hitCollider, Vector3 hitPoint, int hitLocation, int hitMaterial)
        {
            Controller = controller;
            Hitbox = hitbox;
            HitCollider = hitCollider;
            HitPoint = hitPoint;
            HitLocation = hitLocation;
            HitMaterial = hitMaterial;
        }

        public void Deconstruct(out BaseController controller, out Vector3 hitPoint, out int hitLocation, out int hitMaterial) //no Hitbox because it's meant to work with old code
        {
            controller = Controller;
            hitPoint = HitPoint;
            hitLocation = HitLocation;
            hitMaterial = HitMaterial;
        }
    }

    /// <summary>
    /// Data passed to an ITakeDamage when it is hit
    /// </summary>
    /// <remarks>The name is kind of an artifact now, from a time before ITakeDamage when only Actors could take damage</remarks>
    [Serializable]
    public struct ActorHitInfo
    {
        public float Damage;
        public float DamagePierce;
        public int DamageType; //up to you to define indices in an enum or static class
        public int DamageEffector; //up to you to define indices in an enum or static class (defaults/examples provided)
        public bool HarmFriendly;
        public int HitLocation; //up to you to define indices in an enum or static class (defaults/examples provided)
        public int HitMaterial; //up to you to define indices in an enum or static class (defaults/examples provided)
        public BaseController Originator;
        public string OriginatorFaction;
        public string HitPuff;
        public Vector3? HitCoords; //I'm no longer 100% sure this should be nullable
        public BuiltinHitFlags HitFlags;

        public int ExtraHitFlags;
        public IDictionary<string, object> ExtraData;

        public ActorHitInfo(ActorHitInfo original)
        {
            Damage = original.Damage;
            DamagePierce = original.DamagePierce;
            DamageType = original.DamageType;
            DamageEffector = original.DamageEffector;
            HarmFriendly = original.HarmFriendly;
            HitLocation = original.HitLocation;
            HitMaterial = original.HitMaterial;
            Originator = original.Originator;
            OriginatorFaction = original.OriginatorFaction;
            HitPuff = original.HitPuff;
            HitCoords = original.HitCoords;
            HitFlags = original.HitFlags;
            ExtraHitFlags = original.ExtraHitFlags;

            if (original.ExtraData != null)
                ExtraData = new Dictionary<string, object>(original.ExtraData);
            else
                ExtraData = null;
        }

        public ActorHitInfo(float damage, float damagePierce, int damageType, int damageEffector, bool harmFriendly, int hitlocation, int hitmaterial, BaseController originator, string originatorFaction, string hitPuff, Vector3? hitCoords, BuiltinHitFlags hitFlags)
        {
            Damage = damage;
            DamagePierce = damagePierce;
            DamageType = damageType;
            DamageEffector = damageEffector;
            HarmFriendly = harmFriendly;
            HitLocation = hitlocation;
            HitMaterial = hitmaterial;
            Originator = originator;
            OriginatorFaction = originatorFaction;
            HitPuff = hitPuff;
            HitCoords = hitCoords;
            HitFlags = hitFlags;
            ExtraHitFlags = 0;

            ExtraData = null;
        }        
    }

    /// <summary>
    /// Builtin hit flags
    /// </summary>
    [Flags]
    public enum BuiltinHitFlags : int
    {
        None = 0,
        PierceConsiderShields = 1 << 0,
        PierceConsiderArmor = 1 << 1,
        IgnoreShields = 1 << 2,
        IgnoreArmor = 1 << 3,
        NeverAlert = 1 << 4,
        NeverBlockable = 1 << 5,
        NoPain = 1 << 6,
        AlwaysPain = 1 << 7,
        IgnoreHitLocation = 1 << 8,
        AlwaysExtremeDeath = 1 << 9,
        NeverExtremeDeath = 1 << 10,
        DamageOnlyShields = 1 << 10
    }

    /// <summary>
    /// Data used by bullets and bullet creators describing on-hit push physics
    /// </summary>
    [Serializable]
    public struct HitPhysicsInfo
    {
        public float Impulse;
        public BuiltinHitPhysicsFlags HitPhysicsFlags;
        public int ExtraFlags;

        public HitPhysicsInfo(HitPhysicsInfo original)
        {
            Impulse = original.Impulse;
            HitPhysicsFlags = original.HitPhysicsFlags;
            ExtraFlags = original.ExtraFlags;
        }

        public HitPhysicsInfo(float impulse, BuiltinHitPhysicsFlags hitPhysicsFlags)
        {
            Impulse = impulse;
            HitPhysicsFlags = hitPhysicsFlags;
            ExtraFlags = 0;
        }
    }

    [Flags]
    public enum BuiltinHitPhysicsFlags : int
    {
        None = 0,
        //PushFriendly = 1 << 0, //not doable because of how factions are set up, maybe someday
        PushNonEntities = 1 << 1,
        UseFlatPhysics = 1 << 2,
    }

    /// <summary>
    /// Default/builtin body parts/hit locations for ActorHitInfo/ITakeDamage
    /// </summary>
    [PseudoExtensible]
    public enum DefaultHitLocations
    {
        Unspecified = 0, Body = 1, Head = 2, Limb = 3
    }

    /// <summary>
    /// Default/builtin hit materials for ActorHitInfo/ITakeDamage
    /// </summary>
    [PseudoExtensible]
    public enum DefaultHitMaterials
    {
        Unspecified = 0, Generic = 1, Metal = 2, Wood = 3, Stone = 4, Dirt = 5, Flesh = 6
    }

    /// <summary>
    /// Default/builtin damage effectors
    /// </summary>
    [PseudoExtensible]
    public enum DefaultDamageEffectors
    {
        Unspecified = 0, Projectile = 1, Explosion = 2, Melee = 3, Ambient = 4, Internal = 5
    }

    /// <summary>
    /// Default/builtin damage types
    /// </summary>
    [PseudoExtensible]
    public enum DefaultDamageTypes
    {
        Normal = 0, Impact = 1, Explosive = 2, Energy = 3, Poison = 4, Thermal = 5, Radiation = 6
    }
    

}