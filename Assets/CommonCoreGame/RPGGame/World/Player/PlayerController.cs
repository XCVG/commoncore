using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using CommonCore.Input;
using CommonCore.UI;
using CommonCore.LockPause;
using CommonCore.DebugLog;
using CommonCore.State;
using CommonCore.Messaging;
using CommonCore.Audio;
using CommonCore.Config;
using CommonCore.World;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.UI;

namespace CommonCore.RpgGame.World
{
    public class PlayerController : BaseController, ITakeDamage, IAmTargetable
    {
        public bool AutoinitHud = true;

        [Header("Interactivity")]
        public bool PlayerInControl;

        public float MaxProbeDist;
        public float MaxUseDist;

        [Header("Components")]
        public PlayerMovementComponent MovementComponent;
        public PlayerWeaponComponent WeaponComponent;
        public RpgHUDController HUDScript; //TODO should this be here?
        public Transform CameraRoot;
        public GameObject ModelRoot;        
        public Transform TargetPoint;
        public PlayerCameraZoomComponent CameraZoomComponent;
        public PlayerDeathComponent DeathComponent;
        public PlayerShieldComponent ShieldComponent;
        private QdmsMessageInterface MessageInterface;

        [Header("Sounds")]
        public AudioSource PainSound;
        public float PainSoundThreshold = 0;
        public AudioSource DeathSound;

        [Header("Shooting")]
        public bool AttackEnabled = true;

        [Header("Options")]
        public bool HandleDeath = true;
        public bool SynchronizeDeathDelay = true;
        public float DeathWaitTime = 5f;

        private Renderer[] ModelRendererCache;

        private float DyingElapsed = 0;
        public bool IsDying { get; private set; } = false;
        public bool IsDead { get; private set; } = false;

        private bool HadTargetLastFrame = false;

        //bringing back a stupid hack
        public Func<ActorHitInfo, ActorHitInfo?> DamageHandler = null;

        float ITakeDamage.Health => GameState.Instance.PlayerRpgState.Health;

        public override HashSet<string> Tags
        {
            get
            {
                if (_Tags == null)
                {
                    _Tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    _Tags.Add("Player");

                    if (EntityTags != null && EntityTags.Length > 0)
                        Tags.UnionWith(EntityTags);
                }

                return _Tags;
            }
        }

        bool IAmTargetable.ValidTarget => isActiveAndEnabled && !(IsDead || IsDying || GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoTarget));

        string IAmTargetable.Faction => PredefinedFaction.Player.ToString();

        float IAmTargetable.Detectability => RpgValues.DetectionChance(GameState.Instance.PlayerRpgState, MovementComponent.IsCrouching, MovementComponent.IsRunning);

        public override void Awake()
        {
            base.Awake();
        }

        public override void Start()
        {
            base.Start();

            Debug.Log("Player controller start");

            if(!CameraRoot)
            {
                CameraRoot = transform.Find("CameraRoot");
            }

            if(!ModelRoot)
            {
                ModelRoot = transform.GetChild(0).gameObject;
            }

            if(!MovementComponent)
            {
                MovementComponent = GetComponent<PlayerMovementComponent>();
            }

            if(!WeaponComponent)
            {
                WeaponComponent = GetComponentInChildren<PlayerWeaponComponent>();
            }

            if(!CameraZoomComponent)
            {
                CameraZoomComponent = GetComponentInChildren<PlayerCameraZoomComponent>(true);
            }

            if(!DeathComponent)
            {
                DeathComponent = GetComponentInChildren<PlayerDeathComponent>();
            }

            if(!ShieldComponent)
            {
                ShieldComponent = GetComponent<PlayerShieldComponent>();
            }

            if(!HUDScript)
            {
                HUDScript = (RpgHUDController)BaseHUDController.Current; //I would not recommend this cast
            }
            
            if(!HUDScript && AutoinitHud)
            {
                Instantiate<GameObject>(CoreUtils.LoadResource<GameObject>("UI/DefaultWorldHUD"), CoreUtils.GetUIRoot());
                if (EventSystem.current == null)
                    Instantiate(CoreUtils.LoadResource<GameObject>("UI/DefaultEventSystem"));

                HUDScript = (RpgHUDController)BaseHUDController.Current;
            }

            MessageInterface = new QdmsMessageInterface(gameObject);

            LockPauseModule.CaptureMouse = true;

            SetDefaultPlayerView();
            SetInitialViewModels();

            ShieldComponent.Ref()?.HandleLoadStart();
        }

        //should be fixedupdate, probably
        public override void Update()
        {
            HandleMessages();

            if (Time.timeScale == 0 || LockPauseModule.IsPaused())
                return;

            if (HandleDeath)
                HandleDying();

            if (PlayerInControl && !LockPauseModule.IsInputLocked())
            {
                HandleView();
                HandleInteraction();
                //HandleWeapons();
            }

            ShieldComponent.Ref()?.HandleRecharge();
        }

        private void SetDefaultPlayerView()
        {
            //TODO make this not stupid
            GameObject tpCamera = CameraRoot.Find("Main Camera").gameObject;
            GameObject fpCamera = CameraRoot.Find("ViewBobNode").FindDeepChild("FP Camera").gameObject;

            switch (GameParams.DefaultPlayerView)
            {
                case PlayerViewType.PreferFirst:
                    tpCamera.SetActive(false);
                    fpCamera.SetActive(true);
                    SetModelVisibility(ModelVisibility.Invisible);
                    break;
                case PlayerViewType.PreferThird:
                    tpCamera.SetActive(true);
                    fpCamera.SetActive(false);
                    SetModelVisibility(ModelVisibility.Visible);
                    break;
                case PlayerViewType.ForceFirst:
                    tpCamera.SetActive(false);
                    fpCamera.SetActive(true);
                    SetModelVisibility(ModelVisibility.Invisible);
                    break;
                case PlayerViewType.ForceThird:
                    tpCamera.SetActive(true);
                    fpCamera.SetActive(false);
                    SetModelVisibility(ModelVisibility.Visible);
                    break;
                case PlayerViewType.ExplicitOther:
                    tpCamera.SetActive(false);
                    fpCamera.SetActive(false);
                    //SetModelVisibility(ModelVisibility.TotallyInvisible);
                    break;
            }

            PushViewChangeMessage(GameParams.DefaultPlayerView);
        }

        private void SetInitialViewModels()
        {
            WeaponComponent.HandleWeaponChange(EquipSlot.LeftWeapon, true);
            WeaponComponent.HandleWeaponChange(EquipSlot.RightWeapon, true);
        }

        private void HandleMessages()
        {
            while (MessageInterface.HasMessageInQueue)
            {
                HandleMessage(MessageInterface.PopFromQueue());
            }
        }

        private void HandleMessage(QdmsMessage message)
        {
            if (message is QdmsFlagMessage)
            {
                string flag = ((QdmsFlagMessage)message).Flag;
                switch (flag)
                {
                    case "RpgChangeWeapon":
                        {
                            var kvm = message as QdmsKeyValueMessage;
                                
                            if(kvm != null && kvm.HasValue<EquipSlot>("Slot"))
                            {
                                WeaponComponent.HandleWeaponChange(kvm.GetValue<EquipSlot>("Slot"), false);
                            }
                            else
                            {
                                WeaponComponent.HandleWeaponChange(EquipSlot.None, false);
                            }
                                
                        }                        
                        break;
                    case "RpgStatsUpdated":
                        {
                            ShieldComponent.Ref()?.SignalEquipmentChanged();
                        }
                        break;
                }
            }
        }

        private void HandleView()
        {
            if (!(GameParams.DefaultPlayerView == PlayerViewType.PreferFirst || GameParams.DefaultPlayerView == PlayerViewType.PreferThird))
                return;

            if(MappedInput.GetButtonDown(DefaultControls.ChangeView)) 
            {
                //slow and stupid but it'll work for now

                GameObject tpCamera = CameraRoot.Find("Main Camera").gameObject;
                GameObject fpCamera = CameraRoot.Find("ViewBobNode").FindDeepChild("FP Camera").gameObject;

                if (tpCamera.activeSelf)
                {
                    fpCamera.SetActive(true);
                    tpCamera.SetActive(false);
                    SetModelVisibility(ModelVisibility.Invisible);
                    PushViewChangeMessage(PlayerViewType.ForceFirst);
                }
                else
                {
                    fpCamera.SetActive(false);
                    tpCamera.SetActive(true);
                    SetModelVisibility(ModelVisibility.Visible);
                    PushViewChangeMessage(PlayerViewType.ForceThird);
                }
            }
        }

        private void HandleInteraction()
        {
            //get thing, probe and display tooltip, check use

            bool haveTarget = false;

            int layerMask = LayerMask.GetMask("Default","ActorHitbox","Actor");

            Debug.DrawRay(CameraRoot.position, CameraRoot.transform.forward * MaxProbeDist);

            //raycast all, go through the hits ignoring hits to self
            RaycastHit[] hits = Physics.RaycastAll(CameraRoot.transform.position, CameraRoot.transform.forward, MaxProbeDist * 2, layerMask, QueryTriggerInteraction.Collide);            
            if(hits != null && hits.Length > 0)
            {
                //GameObject nearestObject = null;
                InteractableComponent nearestInteractable = null;
                float nearestDist = float.MaxValue;
                foreach(RaycastHit hit in hits)
                {
                    //skip if it's further than nearestDist (occluded) or flatdist is further than MaxProbeDist (too far away)
                    if (hit.distance > nearestDist)
                        continue;

                    float fDist = VectorUtils.GetFlatVectorToTarget(transform.position, hit.point).magnitude;
                    if (fDist > MaxProbeDist)
                        continue;

                    //nearestObject = hit.collider.gameObject;

                    //if there's a PlayerController attached, we've hit ourselves
                    if (hit.collider.GetComponent<PlayerController>() != null)
                        continue;

                    //TODO pull a similar trick to see if we're pointing at an Actor?

                    //get the interactable component and hitbox component; if it doesn't have either then it's an obstacle
                    InteractableComponent ic = hit.collider.GetComponent<InteractableComponent>();
                    IHitboxComponent ahc = hit.collider.GetComponent<IHitboxComponent>();
                    if (ic == null && ahc == null)
                    {
                        //we null out our hit first since it's occluded by this one                        
                        nearestInteractable = null;
                        nearestDist = hit.distance;
                        continue;
                    }

                    //it's just us lol
                    if (ahc != null && ahc.ParentController is PlayerController)
                        continue;                    
                    
                    //we have an interactablecomponent and we're not occluded
                    if(ic != null)
                    {
                        nearestInteractable = ic;
                        nearestDist = hit.distance;
                        continue;
                    }

                    //if it doesn't meet any of those criteria then it's an occluder
                    nearestInteractable = null;
                    nearestDist = hit.distance;

                }

                //if(nearestObject != null)
                //    Debug.Log("Nearest: " + nearestObject.name);

                if (nearestInteractable != null && nearestInteractable.enabled)
                {
                    //Debug.Log("Detected: " + nearestInteractable.Tooltip);

                    //HUDScript.SetTargetMessage(nearestInteractable.Tooltip);
                    nearestInteractable.OnLook(this.gameObject);
                    if (!string.IsNullOrEmpty(nearestInteractable.Tooltip))
                    {
                        MessageInterface.PushToBus(new QdmsKeyValueMessage("PlayerHasTarget", "Target", nearestInteractable.Tooltip));
                        HadTargetLastFrame = true;
                        haveTarget = true;
                    }

                    //actual use
                    if (MappedInput.GetButtonDown(DefaultControls.Use) && !GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoInteract))
                    {
                        nearestInteractable.OnActivate(this.gameObject);
                    }
                }
            }

            if(!haveTarget && HadTargetLastFrame)
            {
                MessageInterface.PushToBus(new QdmsFlagMessage("PlayerClearTarget")); //should probably not do this constantly
            }

            HadTargetLastFrame = haveTarget;

        }

        private void HandleDying()
        {
            if (IsDying && !IsDead)
            {
                float waitTime = (SynchronizeDeathDelay && DeathComponent) ? DeathComponent.TotalWaitTime : DeathWaitTime;

                if (DyingElapsed > waitTime)
                {
                    //Debug.Log("Death complete!");
                    IsDead = true;
                    MessageInterface.PushToBus(new QdmsFlagMessage("PlayerDead"));
                }

                DyingElapsed += Time.deltaTime;
            }

            if (!IsDying && GameState.Instance.PlayerRpgState.Health <= 0 && !MetaState.Instance.SessionFlags.Contains("BuddhaMode") && !GameState.Instance.PlayerFlags.Contains(PlayerFlags.Immortal))
            {
                Debug.Log("You died!");
                IsDying = true;
                PlayerInControl = false;
                MessageInterface.PushToBus(new QdmsFlagMessage("PlayerDying"));
                SetModelVisibility(ModelVisibility.TotallyInvisible);
                WeaponComponent.Ref()?.RequestHideWeapon();
                DeathSound.Ref()?.Play();
                DeathComponent.Ref()?.Die();
            }
        }

        private void SetModelVisibility(ModelVisibility visibility) //sets the visibility of the _third-person_ model, I think
        {
            //fill renderer cache if empty
            if(ModelRendererCache == null || ModelRendererCache.Length == 0)
            {
                ModelRendererCache = ModelRoot.GetComponentsInChildren<Renderer>(true);
            }

            foreach(var r in ModelRendererCache)
            {
                if (visibility == ModelVisibility.Visible)
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                else
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

                if (visibility == ModelVisibility.TotallyInvisible)
                    r.enabled = false;
                else
                    r.enabled = true;
            }

            WeaponComponent.SetVisibility(!(visibility == ModelVisibility.Visible)); //invert because that sets the visibility of the first-person models
        }

        private void PushViewChangeMessage(PlayerViewType newView)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["ViewType"] = newView;
            QdmsKeyValueMessage msg = new QdmsKeyValueMessage(dict, "PlayerChangeView");
            QdmsMessageBus.Instance.PushBroadcast(msg);
        }
        

        public void TakeDamage(ActorHitInfo data)
        {
            if (MetaState.Instance.SessionFlags.Contains("GodMode") || GameState.Instance.PlayerFlags.Contains(PlayerFlags.Invulnerable) || IsDying)
                return;

            if (!data.HarmFriendly)
            {
                string hitFaction = data.OriginatorFaction;
                if (!string.IsNullOrEmpty(hitFaction))
                {
                    FactionRelationStatus relation = FactionModel.GetRelation(hitFaction, PredefinedFaction.Player.ToString()); //this looks backwards but it's because we're checking if the Bullet is-friendly-to the Actor
                    if (relation == FactionRelationStatus.Friendly)
                        return; //no friendly fire
                }
            }

            if (DamageHandler != null)
            {
                var hitOut = DamageHandler(data);
                if (hitOut.HasValue)
                    data = hitOut.Value;
                else
                    return;
            }

            CharacterModel playerModel = GameState.Instance.PlayerRpgState;
        
            var (damageToShields, damageToArmor, damageToCharacter) = RpgValues.DamageRatio(data.Damage, data.DamagePierce, playerModel);

            float oldShields = playerModel.Shields;
            playerModel.Shields -= damageToShields;

            if (oldShields > 0 && playerModel.Shields <= 0)
            {
                MessageInterface.PushToBus(new QdmsFlagMessage("PlayerShieldsLost"));
                ShieldComponent.Ref()?.SignalLostShields();
            }

            var (dt, dr) = playerModel.GetDamageThresholdAndResistance(data.DamageType);
            float damageTaken = RpgValues.DamageTaken(damageToArmor, damageToCharacter, dt, dr);

            if (data.HitLocation == (int)ActorBodyPart.Head)
                damageTaken *= 2.0f;
            else if (data.HitLocation == (int)ActorBodyPart.LeftArm || data.HitLocation == (int)ActorBodyPart.LeftLeg || data.HitLocation == (int)ActorBodyPart.RightArm || data.HitLocation == (int)ActorBodyPart.RightLeg)
                damageTaken *= 0.75f;          

            playerModel.Health -= damageTaken;

            if (damageTaken > PainSoundThreshold)
            {                
                if (PainSound != null && !PainSound.isPlaying)
                    PainSound.Play();                
            }

            if (damageToShields > 0 || damageTaken > 0)
            {
                ShieldComponent.Ref()?.SignalTookDamage(damageToShields, damageTaken);

                var damageValues = new Dictionary<string, object>()
                {
                    { "DamageTaken", damageTaken },
                    { "DamageToShields", damageToShields },
                    { "DamageToArmor", damageToArmor },
                    { "DamageToCharacter", damageToCharacter }
                };
                MessageInterface.PushToBus(new QdmsKeyValueMessage(damageValues, "PlayerTookDamage"));
            }

        }

        private enum ModelVisibility
        {
            Visible, Invisible, TotallyInvisible
        }

    }
}
