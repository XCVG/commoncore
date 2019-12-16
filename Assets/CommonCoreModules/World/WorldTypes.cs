using CommonCore.Messaging;
using System;
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
    /// Data passed to an ITakeDamage when it is hit
    /// </summary>
    /// <remarks>The name is kind of an artifact now, from a time before ITakeDamage when only Actors could take damage</remarks>
    [Serializable]
    public struct ActorHitInfo
    {
        public float Damage;
        public float DamagePierce;
        public int DamageType; //up to you to define indices in an enum or static class
        public int HitLocation; //up to you to define indices in an enum or static class
        public int HitMaterial; //up to you to define indices in an enum or static class
        public BaseController Originator;
        public string HitPuff;
        public Vector3? HitCoords; //I'm no longer 100% sure this should be nullable

        public ActorHitInfo(float damage, float damagePierce, int dtype, int hitlocation, int hitmaterial, BaseController originator)
            : this(damage, damagePierce, dtype, hitlocation, hitmaterial, originator, null, null)
        {
        }

        public ActorHitInfo(float damage, float damagePierce, int dtype, int hitlocation, int hitmaterial, BaseController originator, string hitPuff, Vector3? hitCoords)
        {
            Damage = damage;
            DamagePierce = damagePierce;
            DamageType = dtype;
            HitLocation = hitlocation;
            HitMaterial = hitmaterial;
            Originator = originator;
            HitPuff = hitPuff;
            HitCoords = hitCoords;
        }
    }

    /// <summary>
    /// Default body parts/hit locations for ActorHitInfo/ITakeDamage
    /// </summary>
    public enum DefaultHitLocations
    {
        Unspecified = 0, Body = 1, Head = 2, Limb = 3
    }

    /// <summary>
    /// Default hit materials for ActorHitInfo/ITakeDamage
    /// </summary>
    public enum DefaultHitMaterials
    {
        Unspecified = 0, Generic = 1, Metal = 2, Wood = 3, Stone = 4, Dirt = 5, Flesh = 6
    }

    //I'm not 100% sure about providing these at all
    /*
    public enum DefaultDamageTypes
    {

    }
    */

}