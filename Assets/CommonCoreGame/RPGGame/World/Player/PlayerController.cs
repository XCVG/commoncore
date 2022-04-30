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
    public class PlayerController : BaseController, ITakeDamage, IAmTargetable, IAmPushable, IControlPlayerCamera
    {
        public bool AutoinitHud = true;

        [Header("Interactivity")]
        [Obsolete("Use PlayerFlags instead"), Tooltip("Obsolete")]
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
        public AudioListener AudioListener;

        //hacking around Unity's shitty support for interfaces
        [SerializeField]
        private MonoBehaviour LightReportingComponent;
        public IReportLight LightReporter
        {
            get
            {
                var rl = LightReportingComponent as IReportLight;
                if(rl == null && LightReportingComponent != null)
                {
                    Debug.LogError("[PlayerController] LightReportingComponent is not an IReportLight!");
                    LightReportingComponent = null;
                }

                return rl;
            }
            private set //iunno why you'd want it but it's here if you need it
            {
                LightReportingComponent = (MonoBehaviour)value;
            }
        }

        private QdmsMessageInterface MessageInterface;

        [Header("Sounds")]
        public AudioSource PainSound;
        public float PainSoundThreshold = 0;
        public AudioSource DeathSound;

        [Header("Shooting")]
        [Obsolete("Use PlayerFlags instead"), Tooltip("Obsolete")]
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

        Vector3 IAmTargetable.TargetPoint => TargetPoint.Ref()?.position ?? (MovementComponent.Ref()?.CharacterController == null ? null : (Vector3?)transform.TransformPoint(MovementComponent.CharacterController.center)) ?? transform.position;

        protected override bool DeferComponentInitToSubclass => true;

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

            if(LightReportingComponent == null)
            {
                LightReportingComponent = GetComponentInChildren<IReportLight>() as MonoBehaviour;
            }

            if(AudioListener == null)
            {
                AudioListener = CameraRoot.Ref()?.GetComponentInChildren<AudioListener>();

                if (AudioListener == null)
                    AudioListener = GetComponentInChildren<AudioListener>();
            }

            if(!HUDScript)
            {
                HUDScript = SharedUtils.TryGetHudController() as RpgHUDController;
            }
            
            if(!HUDScript && AutoinitHud)
            {
                Instantiate<GameObject>(CoreUtils.LoadResource<GameObject>("UI/DefaultWorldHUD"), CoreUtils.GetUIRoot());
                if (EventSystem.current == null)
                    Instantiate(CoreUtils.LoadResource<GameObject>("UI/DefaultEventSystem"));

                HUDScript = SharedUtils.TryGetHudController() as RpgHUDController;
                if (HUDScript == null)
                    Debug.LogError("[PlayerController] Failed to initialize HUD properly");
            }

            MessageInterface = new QdmsMessageInterface(gameObject);

            LockPauseModule.CaptureMouse = true;

            SetDefaultPlayerView();
            SetInitialViewModels();

            ShieldComponent.Ref()?.HandleLoadStart();

            TryExecuteOnComponents(component => component.Init(this));
            Initialized = true;
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
                    case "RpgEquipmentChanged":
                        {
                            var kvm = message as QdmsKeyValueMessage;

                            if(kvm != null && kvm.GetValue<CharacterModel>("CharacterModel").IsPlayer)
                            {
                                if (kvm.HasValue<EquipSlot>("Slot"))
                                {
                                    WeaponComponent.HandleWeaponChange(kvm.GetValue<EquipSlot>("Slot"), false);

                                    var arim = kvm.GetValue<InventoryItemInstance>("InventoryItemInstance")?.ItemModel as ArmorItemModel;
                                    if (arim != null && arim.Shields != null)
                                        ShieldComponent.Ref()?.SignalEquipmentChanged();
                                }
                            }                               
                                
                        }                        
                        break;
                    case "RpgStatsUpdated":
                        {
                            var kvm = message as QdmsKeyValueMessage;

                            if (kvm != null && kvm.GetValue<CharacterModel>("CharacterModel").IsPlayer)
                            {
                                ShieldComponent.Ref()?.SignalStatsUpdated();
                            }                            
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
                TryExecuteOnComponents(component => (component as IReceiveDamageableEntityEvents)?.Killed());
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
            QdmsKeyValueMessage msg = new QdmsKeyValueMessage("PlayerChangeView", "ViewType", newView);
            QdmsMessageBus.Instance.PushBroadcast(msg);
        }
        
        public void Kill()
        {
            GameState.Instance.PlayerRpgState.HealthFraction = 0;
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
                    FactionRelationStatus relation = GameState.Instance.FactionState.GetRelation(hitFaction, PredefinedFaction.Player.ToString()); //this looks backwards but it's because we're checking if the Bullet is-friendly-to the Actor
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
        
            var (damageToShields, damageToArmor, damageToCharacter) = RpgValues.DamageRatio(data, playerModel);

            float oldShields = playerModel.Shields;
            playerModel.Shields -= damageToShields;

            if (oldShields > 0 && playerModel.Shields <= 0)
            {
                MessageInterface.PushToBus(new QdmsFlagMessage("PlayerShieldsLost"));
                ShieldComponent.Ref()?.SignalLostShields();
            }

            var (dt, dr) = playerModel.GetDamageThresholdAndResistance(data.DamageType);
            data.Damage = damageToArmor;
            data.DamagePierce = damageToCharacter;
            float damageTaken = RpgValues.DamageTaken(data, dt, dr);

            //do we consider this in the hitboxes already? (answer is no)
            if (!data.HitFlags.HasFlag(BuiltinHitFlags.IgnoreHitLocation))
            {
                if (data.HitLocation == (int)ActorBodyPart.Head)
                    damageTaken *= 2.0f;
                else if (data.HitLocation == (int)ActorBodyPart.LeftArm || data.HitLocation == (int)ActorBodyPart.LeftLeg || data.HitLocation == (int)ActorBodyPart.RightArm || data.HitLocation == (int)ActorBodyPart.RightLeg)
                    damageTaken *= 0.75f;
            }

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
                MessageInterface.PushToBus(new QdmsKeyValueMessage("PlayerTookDamage", damageValues));
            }

            TryExecuteOnComponents(component => (component as IReceiveDamageableEntityEvents)?.DamageTaken(data));

        }

        public Camera GetCamera()
        {
            var cameras = gameObject.GetComponentsInChildren<Camera>();

            //speedhack: if there is one camera on the player and it is enabled, it's our best choice by the conditions below
            if (cameras.Length == 1 && cameras[0].enabled)
                return cameras[0];

            foreach (var camera in cameras)
            {
                if (camera.gameObject.layer == LayerMask.NameToLayer("LightReporter") || camera.gameObject.name.Equals("GunCamera", StringComparison.OrdinalIgnoreCase))
                    continue;

                //First choice is the camera on the player object tagged MainCamera and enabled
                if (camera.gameObject.tag == "MainCamera" && camera.enabled)
                    return camera;
            }

            foreach (var camera in cameras)
            {
                if (camera.gameObject.layer == LayerMask.NameToLayer("LightReporter") || camera.gameObject.name.Equals("GunCamera", StringComparison.OrdinalIgnoreCase))
                    continue;

                //Next choice is the camera on the player object that is enabled
                if (camera.enabled)
                    return camera;
            }

            return null;
        }

        public AudioListener GetAudioListener()
        {
            return AudioListener;
        }

        public void Push(Vector3 impulse)
        {
            if (MovementComponent != null)
                MovementComponent.Push(impulse);
        }

        private enum ModelVisibility
        {
            Visible, Invisible, TotallyInvisible
        }

    }
}
