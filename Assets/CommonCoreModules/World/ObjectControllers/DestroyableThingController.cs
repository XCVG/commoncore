﻿using CommonCore.Audio;
using CommonCore.LockPause;
using CommonCore.ObjectActions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{

    /// <summary>
    /// Controller for "destroyable things" like explosive barrels, etc
    /// </summary>
    public class DestroyableThingController : ThingController, ITakeDamage, IAmTargetable
    {

        [Header("Destroyable Options"), SerializeField]
        public float MaxHealth = 10;        
        public bool Invincible = false;
        public bool IsTarget = false;
        public string Faction;
        public Transform TargetPoint;
        [SerializeField]
        protected float Detectability = 1;
        //public bool Reversible = false;
        [SerializeField, Tooltip("Works the opposite way of DR; incoming damage for a type is multiplied by damage factor")]
        protected DamageFactorNode[] DamageFactors;
        [SerializeField]
        protected bool DisableCollidersOnDeath = false;
        [SerializeField]
        protected bool DeactivateOnDeath = false;
        [SerializeField]
        protected bool DestroyOnDeath = false;
        [SerializeField]
        protected ActionSpecial DeathSpecial = null;
        [SerializeField, Tooltip("If set, death special will be executed on restore")]
        protected bool RepeatDeathSpecial = false;
        [SerializeField]
        protected bool AllowPushingWhenDead = false;
        [SerializeField]
        protected bool TakeShieldsOnlyDamage = false;

        [Header("Pain State Options")]
        public bool UsePainState = false;
        [SerializeField]
        protected float PainChance = 0;
        [SerializeField]
        protected float PainThreshold = 0;
        [SerializeField, Tooltip("If set to true, will treat PainChance as the minimum damage guaranteeing pain, and scale between PainThreshold and PainChance")]
        protected bool UseRelativePainCalculation = false;


        [Header("Visual Options"), SerializeField, Tooltip("Will swap between alive and dead objects if one or both are present")]
        protected GameObject AliveObject = null;
        [SerializeField]
        protected GameObject DeadObject = null;
        [SerializeField]
        protected Transform EffectSpawnPoint = null;
        [SerializeField]
        protected string DeathEffect = null;
        [SerializeField, Tooltip("Pick either play-by-name or play-source. Unless you want both sounds to play.")]
        protected string DeathSoundName = null;
        [SerializeField]
        protected AudioSource DeathSoundSource = null;
        [SerializeField]
        protected string PainEffect = null;
        [SerializeField, Tooltip("Pick either play-by-name or play-source. Unless you want both sounds to play.")]
        protected string PainSoundName = null;
        [SerializeField]
        protected AudioSource PainSoundSource = null;

        [Header("Animation Options"), SerializeField]
        protected Animator Animator = null;
        [SerializeField]
        protected string AliveState = "idle";
        [SerializeField]
        protected string HurtingState = "pain";
        [SerializeField]
        protected string DyingState = "dying";
        [SerializeField]
        protected string DeadState = "dead";

        [Header("Facing Sprite Options"), SerializeField]
        protected DestroyableThingFacingSpriteComponent FacingSpriteComponent = null;
        [SerializeField]
        protected bool DisableFacingSpriteOnDeath = false;

        [Header("Debug")]
        public float Health;

        float ITakeDamage.Health => Health;

        bool IAmTargetable.ValidTarget => IsTarget && !IsDead && isActiveAndEnabled;

        string IAmTargetable.Faction => Faction;

        float IAmTargetable.Detectability => Detectability;

        Vector3 IAmTargetable.TargetPoint => GetTargetPoint();

        public bool IsDead { get; protected set; } = false;
        public bool ForceDeadState { get; protected set; } = false;
        public BaseController LastDamageDealer { get; protected set; } = null;

        protected override bool DeferComponentInitToSubclass => true;
        

        public override void Start()
        {
            base.Start();

            if(Health == 0 && !IsDead)
                Health = MaxHealth;

            TryExecuteOnComponents(component => component.Init(this));
            Initialized = true;
        }

        public override void Update()
        {
            base.Update();

            if (LockPauseModule.IsPaused())
                return;

            UpdateDeathState();
        }

        protected virtual void UpdateDeathState()
        {
            if(ForceDeadState)
            {
                //force dead state after a restore
                Debug.LogWarning($"{name}: force dead state after restore");

                //force objects if they exist
                if (AliveObject != null)
                {
                    AliveObject.SetActive(false);
                }
                if (DeadObject != null)
                {
                    DeadObject.SetActive(true);
                }

                //force animator
                if (Animator != null)
                {
                    Animator.Play(DeadState);
                }

                //force FacingSprite
                if (FacingSpriteComponent != null)
                {
                    FacingSpriteComponent.SetState(DestroyableThingState.Dead);
                }

                //force colliders
                if (DisableCollidersOnDeath)
                {
                    foreach (var collider in GetComponentsInChildren<Collider>())
                    {
                        collider.enabled = false;
                    }
                }

                if(RepeatDeathSpecial)
                    DeathSpecial.Ref()?.Execute(new ActionInvokerData() { Activator = LastDamageDealer, Caller = this }); //note that LastDamageDealer will almost certainly be null

                //destroy/deactivate
                if (DeactivateOnDeath)
                    gameObject.SetActive(false);
                if (DestroyOnDeath)
                    Destroy(gameObject);

                ForceDeadState = false;
                IsDead = true;
            }

            /*
            if(IsDead && Health > 0 && Reversible)
            {
                IsDead = false; //TODO full resurrections
                return;
            }
            */

            if (IsDead)
                return;

            if(Health <= 0)
            {
                Transform effectSpawnPoint = EffectSpawnPoint == null ? transform : EffectSpawnPoint;

                //die

                //animation
                if(AliveObject != null)
                {
                    AliveObject.SetActive(false);                    
                }
                if(DeadObject != null)
                {
                    DeadObject.SetActive(true);
                }

                //Animator animation options
                if(Animator != null)
                {
                    Animator.Play(DyingState);
                }
                //FacingSprite animation options
                if(FacingSpriteComponent != null)
                {
                    FacingSpriteComponent.SetState(DestroyableThingState.Dying);
                }

                //colliders
                if(DisableCollidersOnDeath)
                {
                    foreach(var collider in GetComponentsInChildren<Collider>())
                    {
                        collider.enabled = false;
                    }
                }

                //sound
                if (!string.IsNullOrEmpty(DeathSoundName))
                    AudioPlayer.Instance.PlaySoundPositional(DeathSoundName, SoundType.Sound, false, effectSpawnPoint.position);
                DeathSoundSource.Ref()?.Play();

                //effect
                if(!string.IsNullOrEmpty(DeathEffect))
                    WorldUtils.SpawnEffect(DeathEffect, effectSpawnPoint.position, effectSpawnPoint.eulerAngles, null, false);

                DeathSpecial.Ref()?.Execute(new ActionInvokerData() { Activator = LastDamageDealer, Caller = this });

                //destroy/deactivate
                if (DeactivateOnDeath)
                    gameObject.SetActive(false);
                if (DestroyOnDeath)
                    Destroy(gameObject);

                IsDead = true;

                TryExecuteOnComponents(component => (component as IReceiveDamageableEntityEvents)?.Killed());
            }
        }

        public virtual void Kill(bool bypassInvulnerability)
        {
            if (Invincible && !bypassInvulnerability)
                return;

            Health = 0;
        }

        public virtual void TakeDamage(ActorHitInfo data)
        {
            LastDamageDealer = data.Originator;

            float damage = data.Damage + data.DamagePierce;

            if(data.HitFlags.HasFlag(BuiltinHitFlags.DamageOnlyShields) && !TakeShieldsOnlyDamage)
            {
                damage = 0;
            }

            if(DamageFactors != null && DamageFactors.Length > 0)
            {
                foreach(var dfac in DamageFactors) //no you can't eat here
                {
                    if(dfac.DamageType == data.DamageType)
                    {
                        damage *= dfac.DamageFactor;
                        break;
                    }
                }
            }

            if (!Invincible)
                Health -= damage;

            if(UsePainState && Health > 0 && !ForceDeadState && !IsDead && (damage > PainThreshold || data.HitFlags.HasFlag(BuiltinHitFlags.AlwaysPain)) && !data.HitFlags.HasFlag(BuiltinHitFlags.NoPain))
            {
                //do pain calculation
                bool didTakePain;
                if (data.HitFlags.HasFlag(BuiltinHitFlags.AlwaysPain))
                {
                    didTakePain = true;
                }
                else
                {
                    if (UseRelativePainCalculation)
                    {
                        didTakePain = UnityEngine.Random.Range(PainThreshold, PainChance) < damage;
                    }
                    else
                    {
                        didTakePain = UnityEngine.Random.Range(0f, 1f) < PainChance;
                    }
                }

                if (didTakePain)
                    EnterPainState();

            }

            TryExecuteOnComponents(component => (component as IReceiveDamageableEntityEvents)?.DamageTaken(data));

        }

        protected virtual Vector3 GetTargetPoint() => TargetPoint.Ref()?.position ?? transform.position;

        protected virtual void EnterPainState()
        {
            Transform effectSpawnPoint = EffectSpawnPoint == null ? transform : EffectSpawnPoint;

            if (Animator != null)
            {
                Animator.Play(HurtingState);
            }
            if (FacingSpriteComponent != null)
            {
                FacingSpriteComponent.SetState(DestroyableThingState.Hurting);
            }

            if (!string.IsNullOrEmpty(PainSoundName))
                AudioPlayer.Instance.PlaySoundPositional(PainSoundName, SoundType.Sound, false, effectSpawnPoint.position);
            PainSoundSource.Ref()?.Play();

            if (!string.IsNullOrEmpty(PainEffect))
                WorldUtils.SpawnEffect(PainEffect, effectSpawnPoint.position, effectSpawnPoint.eulerAngles, null, false);
        }

        public override void Push(Vector3 impulse)
        {
            if (IsDead && !AllowPushingWhenDead)
                return;

            base.Push(impulse);
        }

        public override Dictionary<string, object> CommitEntityData()
        {
            var data = base.CommitEntityData();

            var dtData = new DestroyableThingData() { IsDead = this.IsDead, Health = this.Health };

            data.Add("DestroyableThing", dtData);

            return data;
        }

        public override void RestoreEntityData(Dictionary<string, object> data)
        {
            base.RestoreEntityData(data);

            if(data != null && data.ContainsKey("DestroyableThing") && data["DestroyableThing"] is DestroyableThingData dtData)
            {
                IsDead = dtData.IsDead;
                Health = dtData.Health;

                ForceDeadState = IsDead;
            }
        }        

        protected struct DestroyableThingData
        {
            public bool IsDead;
            public float Health;
        }

        [Serializable]
        protected struct DamageFactorNode
        {
            public int DamageType;
            public float DamageFactor;
        }
    }

    public enum DestroyableThingState
    {
        Idle, Hurting, Dying, Dead
    }
}