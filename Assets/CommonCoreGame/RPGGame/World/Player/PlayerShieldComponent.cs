using CommonCore.Audio;
using CommonCore.Messaging;
using CommonCore.RpgGame.Rpg;
using CommonCore.State;
using CommonCore.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// Handles shield effects and recharge. Damage handling is handled in PlayerController and RPGValues
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerShieldComponent : MonoBehaviour
    {
        [Header("Recharge"), SerializeField]
        private float RechargeDelay = 1f;
        [SerializeField]
        private float RechargeRate = 1f;        
        [SerializeField, Tooltip("Need this much damage to cancel recharge")]
        private float RechargeCancelDamage = 1f;
        [SerializeField, FormerlySerializedAs("UseRPGRechargeRate"), Tooltip("RechargeDelay, RechargeRate, RechargeCancelDamage will be treated as multipliers over RPG stats if this is enabled")]
        private bool UseRPGRecharge = true;

        [Header("Misc"), SerializeField, Tooltip("This is compared against ShieldsFraction")]
        private float WarningThreshold = 0.2f;

        [Header("Effects"), SerializeField]
        private string ShieldsLostEffect = null;
        [SerializeField]
        private string ShieldsLostSound = null;
        [SerializeField]
        private AudioSource ShieldsLostAudioSource = null;

        [SerializeField]
        private string HitEffect = null;
        [SerializeField]
        private string HitSound = null;
        [SerializeField]
        private AudioSource HitAudioSource = null;

        [SerializeField]
        private string RechargeEffect = null;
        [SerializeField]
        private string RechargeSound = null;
        [SerializeField]
        private AudioSource RechargeAudioSource = null;
        [SerializeField]
        private bool LoopRechargeSound = false;

        [SerializeField]
        private string WarningSound = null;
        [SerializeField]
        private AudioSource WarningAudioSource = null;
        [SerializeField]
        private bool LoopWarningSound = false;

        //[SerializeField]
        private ShieldRechargeState RechargeState = ShieldRechargeState.Idle;
        private float ElapsedSinceLastDamage = 0;
        private AudioPlayer.SoundInfo RechargeSoundInfo = null;
        private AudioPlayer.SoundInfo WarningSoundInfo = null;

        //called on Start by PlayerController
        public void HandleLoadStart()
        {
            //load recharge state from player model
            var playerModel = GameState.Instance.PlayerRpgState;
            if(playerModel.ExtraData.TryGetValue("ShieldRechargeState", out object rawState))
            {
                RechargeState = (ShieldRechargeState)Convert.ToInt32(rawState); //because it'll actually be long because json.net

                if (RechargeState == ShieldRechargeState.Recharging)
                    PlayRechargeEffect();
            }

            if(RechargeState == ShieldRechargeState.Idle)
            {
                //start waiting to recharge if needed
                var player = GameState.Instance.PlayerRpgState;
                if (player.ShieldsFraction < 1)
                {
                    if (player.DerivedStats.ShieldParams.MaxShields > 0)
                    {
                        SetRechargeState(ShieldRechargeState.Waiting); //now waiting to recharge
                        ElapsedSinceLastDamage = 0;
                    }
                }
            }
        }

        //called on Update by PlayerController
        public void HandleRecharge()
        {
            if (GameState.Instance.PlayerRpgState.HealthFraction <= 0)
                return;

            if (GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoShieldRecharge))
                return;
            
            switch (RechargeState)
            {
                case ShieldRechargeState.Waiting:
                    {
                        ElapsedSinceLastDamage += Time.deltaTime;
                        float rechargeDelay = RechargeDelay;
                        if (UseRPGRecharge)
                            rechargeDelay *= GameState.Instance.PlayerRpgState.DerivedStats.ShieldParams.RechargeDelay;
                        if (ElapsedSinceLastDamage >= rechargeDelay)
                        {
                            SetRechargeState(ShieldRechargeState.Recharging);
                            ElapsedSinceLastDamage = 0;
                            StopWarningSoundLoop();
                            PlayRechargeEffect();
                            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("PlayerShieldsRecharging"));
                        }
                    }
                    break;
                case ShieldRechargeState.Recharging:
                    {
                        //recharge shields
                        var playerModel = GameState.Instance.PlayerRpgState;

                        float rechargeRate = RechargeRate;
                        if (UseRPGRecharge)
                            rechargeRate *= GameState.Instance.PlayerRpgState.DerivedStats.ShieldParams.RechargeRate;

                        playerModel.Shields = Mathf.Min(playerModel.DerivedStats.ShieldParams.MaxShields, playerModel.Shields + rechargeRate * Time.deltaTime);

                        if (Mathf.Approximately(playerModel.ShieldsFraction, 1.0f))
                        {
                            SetRechargeState(ShieldRechargeState.Idle);
                            StopRechargeSoundLoop();
                            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("PlayerShieldsRechargingDone"));
                        }
                    }
                    break;
            }
        }

        //handles recharge-on-equipment-change
        public void SignalEquipmentChanged()
        {
            //Debug.Log("PlayerShieldComponent received SignalEquipmentChanged");

            //stop recharging if we're recharging
            if(RechargeState == ShieldRechargeState.Recharging)
            {
                SetRechargeState(ShieldRechargeState.Idle);
                StopRechargeSoundLoop();
                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("PlayerShieldsRechargingDone"));
            }

            var player = GameState.Instance.PlayerRpgState;
            if(player.ShieldsFraction < 1)
            {
                if(player.DerivedStats.ShieldParams.MaxShields > 0)
                {
                    SetRechargeState(ShieldRechargeState.Waiting); //now waiting to recharge
                    ElapsedSinceLastDamage = 0;
                }
            }

            if(player.DerivedStats.ShieldParams.MaxShields == 0)
            {
                SetRechargeState(ShieldRechargeState.Idle);
            }

            StopWarningSoundLoop();
        }
        
        public void SignalLostShields()
        {
            if (ShieldsLostAudioSource != null)
                ShieldsLostAudioSource.Play();

            if (!string.IsNullOrEmpty(ShieldsLostSound))
                AudioPlayer.Instance.PlaySound(ShieldsLostSound, SoundType.Any, false);

            if (!string.IsNullOrEmpty(ShieldsLostEffect))
                WorldUtils.SpawnEffect(ShieldsLostEffect, transform.position, transform.rotation, null, false);
        }

        public void SignalTookDamage(float damageToShields, float damage)
        {
            CharacterModel player = GameState.Instance.PlayerRpgState;
            float cancelDamage = RechargeCancelDamage;
            if (UseRPGRecharge)
                cancelDamage *= player.DerivedStats.ShieldParams.RechargeCancelDamage;

            if (damageToShields > 0)
            {
                PlayHitEffect();
                
                if (player.ShieldsFraction <= WarningThreshold)
                {
                    PlayWarningEffect();

                }

                switch (RechargeState)
                {

                    case ShieldRechargeState.Idle:
                        SetRechargeState(ShieldRechargeState.Waiting); //now waiting to recharge
                        ElapsedSinceLastDamage = 0;
                        break;
                    case ShieldRechargeState.Waiting:
                        if(damageToShields > cancelDamage)
                            ElapsedSinceLastDamage = 0; //reset timer
                        break;
                    case ShieldRechargeState.Recharging:
                        if (damageToShields > cancelDamage)
                        {
                            SetRechargeState(ShieldRechargeState.Waiting); //reset to waiting
                            ElapsedSinceLastDamage = 0; //reset timer
                        }
                        break;
                }
            }

            //handle reset when shields are down
            if (damage > cancelDamage && player.ShieldsFraction <= 0)
            {
                switch (RechargeState)
                {
                    case ShieldRechargeState.Waiting:
                            ElapsedSinceLastDamage = 0;
                        break;
                    case ShieldRechargeState.Recharging:
                        SetRechargeState(ShieldRechargeState.Waiting); //reset to waiting
                        ElapsedSinceLastDamage = 0; //reset timer
                        break;
                }
            }
        }

        public void SetRechargeState(ShieldRechargeState newState)
        {
            RechargeState = newState;

            //save for later
            var playerModel = GameState.Instance.PlayerRpgState;
            playerModel.ExtraData["ShieldRechargeState"] = newState;            
        }

        public void PlayHitEffect()
        {
            if (HitAudioSource != null)
                HitAudioSource.Play();

            if (!string.IsNullOrEmpty(HitSound))
                AudioPlayer.Instance.PlaySound(HitSound, SoundType.Any, false);

            if (!string.IsNullOrEmpty(HitEffect))
                WorldUtils.SpawnEffect(HitEffect, transform.position, transform.rotation, null, false);
        }

        public void PlayRechargeEffect()
        {
            if (RechargeAudioSource != null)
            {
                if (LoopRechargeSound)
                    RechargeAudioSource.loop = true;
                RechargeAudioSource.Play();
            }

            if (!string.IsNullOrEmpty(RechargeSound))
            {
                if (LoopRechargeSound)
                    RechargeSoundInfo = AudioPlayer.Instance.PlaySoundEx(RechargeSound, SoundType.Any, false, false, true, false, 1.0f, Vector3.zero);
                else
                    AudioPlayer.Instance.PlaySound(RechargeSound, SoundType.Any, false);
            }

            if (!string.IsNullOrEmpty(RechargeEffect))
                WorldUtils.SpawnEffect(RechargeEffect, transform.position, transform.rotation, null, false);
        }

        public void PlayWarningEffect()
        {
            if (WarningAudioSource != null && !WarningAudioSource.isPlaying)
            {
                if (LoopWarningSound)
                    WarningAudioSource.loop = true;
                WarningAudioSource.Play();
            }

            if(WarningSoundInfo != null)
            {
                if (WarningSoundInfo.Source != null && !WarningSoundInfo.Source.isPlaying)
                    WarningSoundInfo = null;
            }

            if (!string.IsNullOrEmpty(WarningSound) && WarningSoundInfo == null)
            {
                if (LoopWarningSound)
                    WarningSoundInfo = AudioPlayer.Instance.PlaySoundEx(WarningSound, SoundType.Any, false, false, true, false, 1.0f, Vector3.zero);
                else
                    WarningSoundInfo = AudioPlayer.Instance.PlaySoundEx(WarningSound, SoundType.Any, false, false, false, false, 1.0f, Vector3.zero);
            }
        }

        public void StopRechargeSoundLoop()
        {
            if(RechargeAudioSource != null)
                RechargeAudioSource.loop = false;

            if (RechargeSoundInfo != null)
            {
                RechargeSoundInfo.Source.loop = false;
                RechargeSoundInfo = null;
            }
            
        }

        public void StopWarningSoundLoop()
        {
            if (WarningAudioSource != null)
                WarningAudioSource.loop = false;

            if (WarningSoundInfo != null)
            {
                WarningSoundInfo.Source.loop = false;
                WarningSoundInfo = null;
            }
        }

        public enum ShieldRechargeState
        {
            Idle, Waiting, Recharging
        }
    }
}