using CommonCore.Scripting;
using CommonCore.State;
using CommonCore.World;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PseudoExtensibleEnum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommonCore.RpgGame.Rpg
{

    public enum AidType //are there even any other stats?
    {
        None, Health, Energy, Magic, Shields //other stats?
    }

    public enum RestoreType
    {
        Add, Boost, //boost allows going over max, add does not
        AddFractional, BoostFractional,
        Override //override replaces
    }

    public enum ItemFlag
    {
        Undefined,

        Unique,

        //weapon flags 
        WeaponTwoHanded, WeaponAutoReload, WeaponNoAmmoUse, WeaponHasADS, WeaponFullAuto, WeaponNoAlert, WeaponHasCharge, WeaponHasRecock, WeaponChargeHold, WeaponShake, WeaponUseCrosshair, WeaponCrosshairInADS, WeaponNoMovebob, WeaponProportionalMovement, WeaponIgnoreLevelledRate, WeaponUnscaledAnimations, WeaponUseFarShootPoint, WeaponProjectileIsEntity, WeaponNeverRandomize, WeaponNeverHarmFriendly, WeaponAlwaysHarmFriendly, WeaponBurstSingleAnimation, WeaponEffectWaitsForLockTime, WeaponAlwaysUseEffectExplosion, WeaponRecockIgnoreLevelledRate, WeaponRecockSkipOnEmpty, WeaponPrewarmBullet, WeaponDamageOnlyShields,

        //WeaponBurstSingleEffect, WeaponBurstRequireFullAmmo, WeaponBurstSucceedWithPartialAmmo, //not supported yet

        //weapon flags (translated to HitFlags)
        WeaponPierceConsiderShields, WeaponPierceConsiderArmor, WeaponIgnoreShields, WeaponIgnoreArmor, WeaponNeverAlert, WeaponNeverBlockable, WeaponNoPain, WeaponAlwaysPain, WeaponIgnoreHitLocation, WeaponAlwaysExtremeDeath, WeaponNeverExtremeDeath,

        //melee-specific weapon flags
        MeleeWeaponUsePreciseCasting, MeleeWeaponDelayCasting, MeleeWeaponAllowMultipleHits, MeleeWeaponHitNonDamageable, MeleeWeaponUseContactHitHack, MeleeWeaponDistinctMultipleHits, MeleeWeaponPushNonEntities,

        //dummy-specific weapon flags
        DummyWeaponUseViewModelRaiseLower,

        //aid item flags
        AidDoNotConsume, AidNoMessage, ComboAidKeepAllMessages
    }

    /// <summary>
    /// A set of scripts attached to an inventory item instance
    /// </summary>
    public class ItemScriptNode
    {
        [JsonProperty]
        public string OnAdd { get; private set; }
        //caller: inventory model
        //args: item model, item instance
        //return: void

        [JsonProperty]
        public string OnRemove { get; private set; }
        //caller: inventory model
        //args: item model, item instance
        //return: void

        [JsonProperty]
        public string OnEquip { get; private set; } //weapon and armor items only
        //args: item model, item instance
        //return: void

        [JsonProperty]
        public string OnUnequip { get; private set; } //weapon and armor items only
        //args: item model, item instance
        //return: void

        [JsonProperty]
        public string OnApply { get; private set; } //aid items only
        //caller: character model
        //args: item model, item instance
        //return: AidItemScriptResult

        [JsonProperty]
        public string OnQuantityChange { get; private set; } //stackable items only
        //caller: inventory model
        //args: item model, item instance, old quantity, new quantity
        //return: void

        [JsonProperty]
        public string OnFire { get; private set; } //weapon items only
        //caller: weapon controller
        //args: item instance, item model
        //return: void

        [JsonProperty]
        public string OnReload { get; private set; } //ranged weapon items only
        //caller: weapon controller
        //args: item instance, item model
        //return: void
    }

    //an actual inventory item that the player has
    [JsonObject(IsReference = true)]
    public class InventoryItemInstance
    {
        //public const int UnstackableQuantity = -1;

        [JsonProperty]
        public long InstanceUID { get; private set; }
        public int Quantity { get; set; }
        public float Condition { get; set; } //it's here but basically unimplemented
        public bool Equipped { get; set; }
        [JsonProperty, JsonConverter(typeof(InstanceItemConverter))]
        public InventoryItemModel ItemModel { get; private set; }
        [JsonProperty]
        public Dictionary<string, object> ExtraData { get; private set; } = new Dictionary<string, object>();

        [JsonConstructor]
        private InventoryItemInstance()
        {

        }

        public InventoryItemInstance(InventoryItemModel model, long id, float condition, int quantity, bool equipped)
        {
            InstanceUID = id;
            ItemModel = model;
            Condition = condition;
            Equipped = equipped;
            Quantity = quantity;
        }

        public InventoryItemInstance(InventoryItemModel model, float condition, int quantity, bool equipped) : this(model, 0, condition, quantity, equipped)
        {
            ResetUID();
        }

        internal void ResetUID()
        {
            var gameState = GameState.Instance;
            if (gameState != null)
            {
                //use GameState id counter
                InstanceUID = gameState.NextUID;
            }
            else
            {
                //use Time based id counter
                byte[] idBytes = BitConverter.GetBytes((ulong)DateTime.UtcNow.Ticks);
                idBytes[0] = (byte)(CoreUtils.Random.Next(byte.MinValue, byte.MaxValue));
                InstanceUID = (long)BitConverter.ToUInt64(idBytes, 0);
                Debug.LogWarning($"InventoryItemInstance had InstanceUID set from time based counter (UID {InstanceUID}, model {ItemModel?.Name})");
            }
        }

        public InventoryItemInstance(InventoryItemModel model) : this(model, model.MaxCondition, 1, false)
        {
        }

        public override string ToString()
        {
            return $"{InstanceUID} ({ItemModel}) [{Quantity}]";
        }
    }

    public class InstanceItemConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(InventoryItemModel).IsAssignableFrom(objectType);
            //return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            JObject jsonObject = JObject.Load(reader);
            string modelName = jsonObject["$ItemModel"].Value<string>();
            var model = InventoryModel.GetModel(modelName);
            return model;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var item = value as InventoryItemModel;
            writer.WriteStartObject();
            writer.WritePropertyName("$ItemModel");
            writer.WriteValue(item.Name);
            writer.WriteEndObject();
        }
    }

    // class for invariant inventory defs
    public class InventoryItemDef
    {
        [JsonProperty]
        public string NiceName { get; private set; }
        [JsonProperty]
        public string Image { get; private set; }
        [JsonProperty]
        public string Description { get; private set; }

        [JsonProperty]
        public IReadOnlyDictionary<string, object> ExtraData { get; private set; } = new Dictionary<string, object>();

        public InventoryItemDef(string niceName, string image, string description, IEnumerable<KeyValuePair<string, object>> extraData)
        {
            NiceName = niceName;
            Image = image;
            Description = description;
            ExtraData = extraData?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, object>();
        }

        public override string ToString()
        {
            return string.Format("[{0}:{1}\t{2}]", NiceName, Image, Description);
        }
    }

    //base class for invariant inventory items
    public abstract class InventoryItemModel
    {
        [JsonProperty]
        public string Name { get; protected set; }
        [JsonProperty]
        public float Weight { get; protected set; }
        [JsonProperty]
        public float Value { get; protected set; }
        [JsonProperty]
        public float MaxCondition { get; protected set; }
        [JsonProperty]
        public int MaxQuantity { get; protected set; }
        [JsonProperty]
        public bool Hidden { get; protected set; }
        [JsonProperty]
        public bool Essential { get; protected set; }
        [JsonProperty]
        public string WorldModel { get; protected set; }
        [JsonProperty]
        public string[] Flags { get; protected set; } = new string[0];
        [JsonProperty]
        public ItemScriptNode Scripts { get; protected set; } = new ItemScriptNode();
        [JsonProperty]
        public IReadOnlyDictionary<string, object> ExtraData { get; private set; } = new Dictionary<string, object>();

        [JsonIgnore]
        public bool Stackable { get; protected set; } = false;

        [JsonConstructor]
        protected InventoryItemModel()
        {

        }

        public InventoryItemModel(string name, float weight, float value, float maxCondition, int maxQuantity, bool hidden, bool essential, string[] flags, Dictionary<string, object> extraData, ItemScriptNode scripts, string worldModel)
        {
            Name = name;
            Weight = weight;
            Value = value;
            MaxCondition = maxCondition;
            MaxQuantity = maxQuantity;
            Hidden = hidden;
            Essential = essential;
            Stackable = false;
            WorldModel = worldModel;
            Scripts = scripts ?? new ItemScriptNode();            
            Flags = (flags == null || flags.Length == 0) ? new string[0] : flags;
            ExtraData = extraData == null ? new Dictionary<string, object>() : extraData;
        }

        public virtual string GetStatsString()
        {
            return $"{(Essential ? "<b>Essential</b>" : string.Empty)}\nValue: {Value:F1}\nWeight: {Weight:F1}";
        }

        public bool CheckFlag(ItemFlag flag)
        {
            return CheckFlag(flag.ToString());
        }

        public bool CheckFlag(string flag)
        {
            return Array.Exists(Flags, x => flag.Equals(x, StringComparison.OrdinalIgnoreCase));
        }

        public override string ToString()
        {
            return $"{Name} : {GetType().Name}";
        }

    }

    public class MiscItemModel : InventoryItemModel
    {
        [JsonConstructor]
        protected MiscItemModel() : base()
        {

        }

        public MiscItemModel(string name, float weight, float value, float maxCondition, int maxQuantity, bool hidden, bool essential, string[] flags, Dictionary<string, object> extraData, ItemScriptNode scripts, string worldModel)
            : base(name, weight, value, maxCondition, maxQuantity, hidden, essential, flags, extraData, scripts, worldModel)
        {
        }

        public override string GetStatsString()
        {
            return "<b>Misc Item</b>\n" + base.GetStatsString();
        }
    }

    public abstract class WeaponItemModel : InventoryItemModel
    {
        [JsonProperty]
        public float Damage { get; protected set; }
        [JsonProperty]
        public float DamagePierce { get; protected set; }
        [JsonProperty]
        public float DamageSpread { get; protected set; }
        [JsonProperty]
        public float DamagePierceSpread { get; protected set; }
        [JsonProperty, JsonConverter(typeof(PxEnumConverter), typeof(DefaultDamageTypes))]
        public int DType { get; protected set; }
        [JsonProperty, JsonConverter(typeof(PxEnumConverter), typeof(DefaultDamageEffectors))]
        protected int? DEffector { get; set; }
        [JsonProperty, JsonConverter(typeof(PxEnumConverter), typeof(SkillType))]
        public int SkillType { get; protected set; }
        [JsonProperty]
        public string ViewModel { get; protected set; }
        [JsonProperty]
        public string HitPuff { get; protected set; }
        [JsonProperty]
        public float LowerTime { get; protected set; }
        [JsonProperty]
        public float RaiseTime { get; protected set; }

        [JsonConstructor]
        protected WeaponItemModel() : base()
        {

        }

        public WeaponItemModel(string name, float weight, float value, float maxCondition, int maxQuantity, bool hidden, bool essential, string[] flags, Dictionary<string, object> extraData, ItemScriptNode scripts,
            float damage, float damagePierce, float damageSpread, float damagePierceSpread,
            int dType, int? dEffector, int skillType, string viewModel, string worldModel, string hitPuff, float lowerTime, float raiseTime)
            : base(name, weight, value, maxCondition, maxQuantity, hidden, essential, flags, extraData, scripts, worldModel)
        {
            Damage = damage;
            DamagePierce = damagePierce;
            DamageSpread = damageSpread;
            DamagePierceSpread = damagePierceSpread;
            DType = dType;
            DEffector = dEffector;
            ViewModel = viewModel;
            HitPuff = hitPuff;
            SkillType = skillType;
            LowerTime = lowerTime;
            RaiseTime = raiseTime;
        }

        public override string GetStatsString()
        {
            return base.GetStatsString() + $"\nDamage: {Damage:F1} ({DamagePierce:F1})\nType: {DType.ToString()}";
        }

        [JsonIgnore]
        public virtual int Effector => DEffector ?? (int)DefaultDamageEffectors.Unspecified;

        [JsonIgnore]
        public bool? HarmFriendly {
            get
            {
                bool alwaysFlag = CheckFlag(ItemFlag.WeaponAlwaysHarmFriendly);
                bool neverFlag = CheckFlag(ItemFlag.WeaponNeverHarmFriendly);
                if (alwaysFlag && neverFlag)
                {
                    UnityEngine.Debug.LogWarning($"Weapon item model \"{Name}\" is set to both always and never harm friendlies! (behaviour is undefined)");
                    return null;
                }

                if (alwaysFlag)
                    return true;

                if (neverFlag)
                    return false;

                return null;
            }
        }

        public BuiltinHitFlags GetHitFlags()
        {
            BuiltinHitFlags flags = BuiltinHitFlags.None;

            if (CheckFlag(ItemFlag.WeaponPierceConsiderShields))
                flags |= BuiltinHitFlags.PierceConsiderShields;

            if (CheckFlag(ItemFlag.WeaponPierceConsiderArmor))
                flags |= BuiltinHitFlags.PierceConsiderArmor;

            if (CheckFlag(ItemFlag.WeaponIgnoreShields))
                flags |= BuiltinHitFlags.IgnoreShields;

            if (CheckFlag(ItemFlag.WeaponIgnoreArmor))
                flags |= BuiltinHitFlags.IgnoreArmor;

            if (CheckFlag(ItemFlag.WeaponNeverAlert))
                flags |= BuiltinHitFlags.NeverAlert;

            if (CheckFlag(ItemFlag.WeaponNeverBlockable))
                flags |= BuiltinHitFlags.NeverBlockable;

            if (CheckFlag(ItemFlag.WeaponNoPain))
                flags |= BuiltinHitFlags.NoPain;

            if (CheckFlag(ItemFlag.WeaponAlwaysPain))
                flags |= BuiltinHitFlags.AlwaysPain;

            if (CheckFlag(ItemFlag.WeaponIgnoreHitLocation))
                flags |= BuiltinHitFlags.IgnoreHitLocation;

            if (CheckFlag(ItemFlag.WeaponAlwaysExtremeDeath))
                flags |= BuiltinHitFlags.AlwaysExtremeDeath;

            if (CheckFlag(ItemFlag.WeaponNeverExtremeDeath))
                flags |= BuiltinHitFlags.NeverExtremeDeath;

            if (CheckFlag(ItemFlag.WeaponDamageOnlyShields))
                flags |= BuiltinHitFlags.DamageOnlyShields;

            return flags;
        }
    }

    public class MeleeWeaponItemModel : WeaponItemModel
    {
        [JsonProperty]
        public float Reach { get; protected set; }
        [JsonProperty]
        public float Rate { get; protected set; }
        [JsonProperty]
        public float EnergyCost { get; protected set; }
        [JsonProperty]
        public float DamageDelay { get; protected set; }

        [JsonProperty]
        public float CastRadius { get; protected set; }

        [JsonProperty]
        public float Impulse { get; protected set; }

        [JsonProperty]
        public string EnvironmentHitPuff { get; protected set; }

        [JsonConstructor]
        protected MeleeWeaponItemModel() : base()
        {

        }

        public MeleeWeaponItemModel(string name, float weight, float value, float maxCondition, int maxQuantity, bool hidden, bool essential, string[] flags, Dictionary<string, object> extraData, ItemScriptNode scripts,
            float damage, float damagePierce, float damageSpread, float damagePierceSpread,
            float reach, float rate, float energyCost, float damageDelay,
            float castRadius, float impulse, string environmentHitPuff,
            int dType, int? dEffector, int skillType,
            string viewModel, string worldModel, string hitPuff, float lowerTime, float raiseTime) 
            : base(name, weight, value, maxCondition, maxQuantity, hidden, essential, flags, extraData, scripts, damage, damagePierce, damageSpread, damagePierceSpread, dType, dEffector, skillType, viewModel, worldModel, hitPuff, lowerTime, raiseTime)
        {
            Reach = reach;
            Rate = rate;
            EnergyCost = energyCost;
            DamageDelay = damageDelay;

            CastRadius = castRadius;
            Impulse = impulse;
            EnvironmentHitPuff = environmentHitPuff;
        }

        public override int Effector => DEffector ?? (int)DefaultDamageEffectors.Melee;

        public override string GetStatsString()
        {
            return $"<b>Melee Weapon ({SkillType})</b>\n" + base.GetStatsString() + $"\nSpeed: {(1/Rate):F1}";
        }
                
    }

    public class RangedWeaponItemModel : WeaponItemModel
    {
        [JsonProperty]
        public float ProjectileVelocity { get; protected set; }

        [JsonProperty]
        public RangeEnvelope Recoil { get; protected set; }
        [JsonProperty]
        public RangeEnvelope Spread { get; protected set; }
        [JsonProperty]
        public RangeEnvelope ADSRecoil { get; protected set; }
        [JsonProperty]
        public RangeEnvelope ADSSpread { get; protected set; }
        [JsonProperty]
        public PulseEnvelope RecoilImpulse { get; protected set; }
        [JsonProperty]
        public PulseEnvelope ADSRecoilImpulse { get; protected set; }

        [JsonProperty]
        public float RecoilEffectScale { get; protected set; } = 1f;
        [JsonProperty]
        public float ADSRecoilEffectScale { get; protected set; } = 1f;

        [JsonProperty]
        public float MovementSpreadFactor { get; protected set; }
        [JsonProperty]
        public float MovementRecoveryFactor { get; protected set; }
        [JsonProperty]
        public float CrouchSpreadFactor { get; protected set; }
        [JsonProperty]
        public float CrouchRecoveryFactor { get; protected set; }

        [JsonProperty]
        public float FireInterval { get; protected set; }
        [JsonProperty(propertyName: "BurstFireInterval")]
        protected float? BurstFireIntervalInternal { get; set; }
        [JsonIgnore]
        public float BurstFireInterval => BurstFireIntervalInternal ?? FireInterval;

        [JsonProperty]
        public int ProjectilesPerShot { get; protected set; } = 1;
        [JsonProperty]
        public int AmmoPerShot { get; protected set; } = 1;
        [JsonProperty]
        public int ShotsPerBurst { get; protected set; } = 1;
        [JsonProperty]
        public float LockTime { get; protected set; }
        [JsonProperty]
        public float RecockTime { get; protected set; }

        [JsonProperty]
        public int MagazineSize { get; protected set; }
        [JsonProperty]
        public float ReloadTime { get; protected set; }

        [JsonProperty]
        public float ADSZoomFactor { get; protected set; }

        [JsonProperty]
        public string AType { get; protected set; }
        [JsonProperty]
        public string Projectile { get; protected set; }

        [JsonProperty]
        public RangedWeaponItemProjectileData ProjectileData { get; protected set; }
        [JsonProperty]
        public RangedWeaponItemExplosionData ExplosionData { get; protected set; }
        [JsonProperty]
        public RangedWeaponItemPhysicsData PhysicsData { get; protected set; }

        [JsonConstructor]
        protected RangedWeaponItemModel() : base()
        {

        }

        public RangedWeaponItemModel(string name, float weight, float value, float maxCondition, int maxQuantity, bool hidden, bool essential, string[] flags, Dictionary<string, object> extraData, ItemScriptNode scripts,
            float damage, float damagePierce, float damageSpread, float damagePierceSpread, float projectileVelocity,
            RangeEnvelope recoil, RangeEnvelope spread, RangeEnvelope adsRecoil, RangeEnvelope adsSpread,
            PulseEnvelope recoilImpulse, PulseEnvelope adsRecoilImpulse,
            float? recoilEffectScale, float? adsRecoilEffectScale,
            float movementSpreadFactor, float movementRecoveryFactor, float crouchSpreadFactor, float crouchRecoveryFactor,
            float fireInterval, float? burstFireInterval, int? projectilesPerShot, int? ammoPerShot, int? shotsPerBurst, float lockTime, float recockTime, int magazineSize, float reloadTime,
            string aType, int dType, int? dEffector, int skillType, string viewModel, string worldModel,
            string hitPuff, string projectile, float adsZoomFactor, float lowerTime, float raiseTime,
            RangedWeaponItemProjectileData projectileData, RangedWeaponItemExplosionData explosionData, RangedWeaponItemPhysicsData physicsData)
            : base(name, weight, value, maxCondition, maxQuantity, hidden, essential, flags, extraData, scripts, damage, damagePierce, damageSpread, damagePierceSpread, dType, dEffector, skillType, viewModel, worldModel, hitPuff, lowerTime, raiseTime)
        {
            ProjectileVelocity = projectileVelocity;

            Recoil = recoil;
            Spread = spread;
            ADSRecoil = adsRecoil;
            ADSSpread = adsSpread;
            RecoilImpulse = recoilImpulse;
            ADSRecoilImpulse = adsRecoilImpulse;

            RecoilEffectScale = recoilEffectScale ?? 1f;
            ADSRecoilEffectScale = adsRecoilEffectScale ?? 1f;

            MovementSpreadFactor = movementSpreadFactor;
            MovementRecoveryFactor = movementRecoveryFactor;
            CrouchSpreadFactor = crouchSpreadFactor;
            CrouchRecoveryFactor = crouchRecoveryFactor;

            FireInterval = fireInterval;
            BurstFireIntervalInternal = burstFireInterval;
            ProjectilesPerShot = projectilesPerShot ?? 1;
            AmmoPerShot = ammoPerShot ?? 1;
            ShotsPerBurst = shotsPerBurst?? 1;
            RecockTime = recockTime;
            LockTime = lockTime;

            MagazineSize = magazineSize;
            ReloadTime = reloadTime;

            ADSZoomFactor = adsZoomFactor;

            AType = aType;
            Projectile = projectile;

            ProjectileData = projectileData;
            ExplosionData = explosionData;
            PhysicsData = physicsData;
        }

        [JsonIgnore]
        public bool UseMagazine => MagazineSize > 0;

        [JsonIgnore]
        public bool UseAmmo => !(string.IsNullOrEmpty(AType) || string.Equals(AType, "NoAmmo", StringComparison.OrdinalIgnoreCase)); //"NoAmmo" is for legacy compat

        public override int Effector => DEffector ?? (int)DefaultDamageEffectors.Projectile;

        public override string GetStatsString()
        {
            return $"<b>Ranged Weapon ({SkillType})</b>\n" + base.GetStatsString() + $"\nSpeed: {(1 / FireInterval):F1}\nMagazine: {MagazineSize}\nAmmo Type{InventoryModel.GetNiceName(AType.ToString())}";
        }
    }

    public class RangedWeaponItemExplosionData
    {
        [JsonProperty]
        public float Damage { get; private set; }
        [JsonProperty]
        public float Radius { get; private set; }
        [JsonProperty]
        public bool UseFalloff { get; private set; } = true;
        [JsonProperty]
        public string HitPuff { get; private set; } = string.Empty;

        [JsonProperty]
        public bool DetonateOnWorldHit { get; private set; } = true;

        [JsonProperty]
        public bool DetonateOnDespawn { get; private set; } = false;

        [JsonProperty]
        public bool EnableProximityDetonation { get; private set; }
        [JsonProperty]
        public float ProximityRadius { get; private set; }
        [JsonProperty]
        public bool UseFactions { get; private set; } = false;
        [JsonProperty]
        public bool UseTangentHack { get; private set; } = false;

        [JsonProperty]
        public float Impulse { get; private set; }
        [JsonProperty]
        public bool PushNonEntities { get; private set; }
        [JsonProperty]
        public bool ImpulseFlatPhysics { get; private set; }
        [JsonProperty]
        public bool ImpulseUseFalloff { get; private set; }
    }

    public class RangedWeaponItemProjectileData
    {
        //we may enable more options here later
        [JsonProperty]
        public float Gravity { get; private set; }
    }

    public class RangedWeaponItemPhysicsData
    {
        [JsonProperty]
        public float Impulse { get; private set; }
        [JsonProperty]
        public bool PushNonEntities { get; private set; }
        [JsonProperty]
        public bool UseFlatPhysics { get; private set; }
    }

    public class DummyWeaponItemModel : WeaponItemModel
    {
        public DummyWeaponItemModel(string name, float weight, float value, float maxCondition, int maxQuantity, bool hidden, bool essential, string[] flags, Dictionary<string, object> extraData, ItemScriptNode scripts, float damage, float damagePierce, float damageSpread, float damagePierceSpread, int dType, int? dEffector, int skillType, string viewModel, string worldModel, string hitPuff, float lowerTime, float raiseTime) : base(name, weight, value, maxCondition, maxQuantity, hidden, essential, flags, extraData, scripts, damage, damagePierce, damageSpread, damagePierceSpread, dType, dEffector, skillType, viewModel, worldModel, hitPuff, lowerTime, raiseTime)
        {
        }

        public override int Effector => (int)DefaultDamageEffectors.Unspecified;

        public override string GetStatsString()
        {
            return base.GetStatsString() + $"\n(dummy weapon)";
        }
    }

    public class ArmorItemModel : InventoryItemModel
    {
        [JsonProperty, JsonConverter(typeof(PxEnumObjectConverter), typeof(DefaultDamageTypes), true, true)]
        public IReadOnlyDictionary<int, float> DamageResistance { get; protected set; } = new Dictionary<int, float>();
        [JsonProperty, JsonConverter(typeof(PxEnumObjectConverter), typeof(DefaultDamageTypes), true, true)]
        public IReadOnlyDictionary<int, float> DamageThreshold { get; protected set; } = new Dictionary<int, float>();
        [JsonProperty]
        public ShieldParams Shields { get; protected set; }
        [JsonProperty, JsonConverter(typeof(PxEnumConverter), typeof(EquipSlot))]
        public int Slot { get; protected set; }

        [JsonConstructor]
        protected ArmorItemModel() : base()
        {

        }

        public ArmorItemModel(string name, float weight, float value, float maxCondition, int maxQuantity, bool hidden, bool essential, string[] flags, Dictionary<string, object> extraData, ItemScriptNode scripts, string worldModel,
            Dictionary<int, float> damageResistance, Dictionary<int, float> damageThreshold, ShieldParams shields, int slot)
            : base(name, weight, value, maxCondition, maxQuantity, hidden, essential, flags, extraData, scripts, worldModel)
        {
            DamageResistance = new Dictionary<int, float>(damageResistance);
            DamageThreshold = new Dictionary<int, float>(damageThreshold);
            Shields = shields;
            Slot = slot;
        }

        public override string GetStatsString()
        {
            StringBuilder res = new StringBuilder();
            if(Shields != null && Shields.MaxShields > 0)
            {
                res.Append($"\nShields: {Shields.MaxShields:F1} ({Shields.RechargeRate:F1}/s)");
            }
            foreach(var key in DamageResistance.Keys.Union(DamageThreshold.Keys))
            {
                float dr = DamageResistance.GetOrDefault(key, 0);
                float dt = DamageThreshold.GetOrDefault(key, 0);
                if (dr == 0 && dt == 0)
                    continue;
                res.AppendFormat("\n{0}: {1:F1} ({2:F1})", key, dr, dt);
            }

            return $"<b>Armor: {Slot.ToString()}</b>\n" + base.GetStatsString() + res.ToString();
        }
    }

    public class AidItemModel : InventoryItemModel
    {
        [JsonProperty]
        public AidType AType { get; protected set; }
        [JsonProperty]
        public RestoreType RType { get; protected set; }
        [JsonProperty]
        public float Amount { get; protected set; }

        [JsonConstructor]
        protected AidItemModel() : base()
        {

        }

        public AidItemModel(string name, float weight, float value, float maxCondition, int maxQuantity, bool hidden, bool essential, string[] flags, Dictionary<string, object> extraData, ItemScriptNode scripts, string worldModel,
            AidType aType, RestoreType rType, float amount, string script)
            : base(name, weight, value, maxCondition, maxQuantity, hidden, essential, flags, extraData, scripts, worldModel)
        {
            AType = aType;
            RType = rType;
            Amount = amount;
        }

        public virtual AidItemApplyResult Apply(CharacterModel target, InventoryItemInstance itemInstance)
        {
            float amountRestored;
            if (string.IsNullOrEmpty(Scripts.OnApply))
            {
                amountRestored = ApplyBase(this, target);
                return new AidItemApplyResult() { Success = true, ConsumeItem = !CheckFlag(ItemFlag.AidDoNotConsume), ShowMessage = !CheckFlag(ItemFlag.AidNoMessage), Message = GetSuccessMessage(amountRestored) };
            }

            var sResult = ApplyScript(this, target, itemInstance);
            amountRestored = sResult.AmountRestored;
            if (sResult.ContinueApply)
                amountRestored = ApplyBase(this, target);
            return new AidItemApplyResult() { Success = sResult.ReturnSuccess, ConsumeItem = sResult.ConsumeItem, ShowMessage = sResult.ShowMessage, Message = sResult.MessageOverride ?? GetSuccessMessage(amountRestored) };
        }

        protected static AidItemScriptResult ApplyScript(AidItemModel item, CharacterModel player, InventoryItemInstance itemInstance)
        {
            return ScriptingModule.CallForResult<AidItemScriptResult>(item.Scripts.OnApply, new ScriptExecutionContext() { Caller = player }, item, itemInstance);
        }

        protected static float ApplyBase(AidItemModel item, CharacterModel player)
        {
            return ApplyNode(item.AType, item.RType, item.Amount, player);
        }

        protected static float ApplyNode(AidType aType, RestoreType rType, float amount, CharacterModel player)
        {
            float amountRestored = 0;

            //this is super gross but I will improve it later
            switch (aType)
            {
                case AidType.Health:
                    {
                        switch (rType)
                        {
                            case RestoreType.Add:
                                {
                                    float oldHealth = player.Health;
                                    player.Health = Math.Min(player.Health + amount, player.DerivedStats.MaxHealth);
                                    amountRestored = player.Health - oldHealth;
                                }
                                break;
                            case RestoreType.Boost:
                                player.Health += amount;
                                amountRestored = amount;
                                break;
                            case RestoreType.AddFractional:
                                {
                                    float oldHealth = player.Health;
                                    player.Health = Math.Min(player.Health + (amount * player.DerivedStats.MaxHealth), player.DerivedStats.MaxHealth);
                                    amountRestored = player.Health - oldHealth;
                                }
                                break;
                            case RestoreType.BoostFractional:
                                player.Health += amount * player.DerivedStats.MaxHealth;
                                amountRestored = amount * player.DerivedStats.MaxHealth;
                                break;
                            case RestoreType.Override:
                                player.Health = amount;
                                amountRestored = amount;
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case AidType.Energy:
                    {
                        switch (rType)
                        {
                            case RestoreType.Add:
                                {
                                    float oldEnergy = player.Energy;
                                    player.Energy = Math.Min(player.Energy + amount, player.DerivedStats.MaxEnergy);
                                    amountRestored = player.Energy - oldEnergy;
                                }
                                break;
                            case RestoreType.Boost:
                                player.Energy += amount;
                                amountRestored = amount;
                                break;
                            case RestoreType.AddFractional:
                                {
                                    float oldEnergy = player.Energy;
                                    player.Energy = Math.Min(player.Energy + amount * player.DerivedStats.MaxEnergy, player.DerivedStats.MaxEnergy);
                                    amountRestored = player.Energy - oldEnergy;
                                }
                                break;
                            case RestoreType.BoostFractional:
                                player.Energy += amount * player.DerivedStats.MaxEnergy;
                                amountRestored = amount * player.DerivedStats.MaxEnergy;
                                break;
                            case RestoreType.Override:
                                player.Energy = amount;
                                amountRestored = amount;
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case AidType.Magic:
                    {
                        switch (rType)
                        {
                            case RestoreType.Add:
                                {
                                    float oldMagic = player.Magic;
                                    player.Magic = Math.Min(player.Magic + amount, player.DerivedStats.MaxMagic);
                                    amountRestored = player.Magic - oldMagic;
                                }
                                break;
                            case RestoreType.Boost:
                                player.Magic += amount;
                                amountRestored = amount;
                                break;
                            case RestoreType.AddFractional:
                                {
                                    float oldMagic = player.Magic;
                                    player.Magic = Math.Min(player.Magic + amount * player.DerivedStats.MaxMagic, player.DerivedStats.MaxMagic);
                                    amountRestored = player.Magic - oldMagic;
                                }
                                break;
                            case RestoreType.BoostFractional:
                                player.Magic += amount * player.DerivedStats.MaxMagic;
                                amountRestored = amount * player.DerivedStats.MaxMagic;
                                break;
                            case RestoreType.Override:
                                player.Magic = amount;
                                amountRestored = amount;
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case AidType.Shields:
                    {
                        switch (rType)
                        {
                            case RestoreType.Add:
                                {
                                    float oldShields = player.Shields;
                                    player.Shields = Math.Min(player.Shields + amount, player.DerivedStats.ShieldParams.MaxShields);
                                    amountRestored = player.Shields - oldShields;
                                }
                                break;
                            case RestoreType.Boost:
                                player.Shields += amount;
                                amountRestored = amount;
                                break;
                            case RestoreType.AddFractional:
                                {
                                    float oldShields = player.Shields;
                                    player.Shields = Math.Min(player.Shields + amount * player.DerivedStats.ShieldParams.MaxShields, player.DerivedStats.ShieldParams.MaxShields);
                                    amountRestored = player.Shields - oldShields;
                                }
                                break;
                            case RestoreType.BoostFractional:
                                player.Shields += amount * player.DerivedStats.ShieldParams.MaxShields;
                                amountRestored = amount * player.DerivedStats.ShieldParams.MaxShields;
                                break;
                            case RestoreType.Override:
                                player.Shields = amount;
                                amountRestored = amount;
                                break;
                            default:
                                break;
                        }
                    }
                    break;
            }

            return amountRestored;
        }

        protected virtual string GetSuccessMessage(float amountRestored)
        {
            return string.Format($"Restored {(int)amountRestored} {AType.ToString()}!");
        }

        public override string GetStatsString()
        {
            return $"<b>Aid Item</b>\n" + base.GetStatsString() + $"\n{AType}: {Amount:F1} ({RType})\n"; //will need to redo this when we extend AidItem
        }
    }

    public struct AidItemApplyResult
    {
        public bool Success;
        public bool ConsumeItem;
        public bool ShowMessage;
        public string Message;
    }

    public struct AidItemScriptResult
    {
        public bool ContinueApply;
        public bool ReturnSuccess;        
        public bool ConsumeItem;
        public bool ShowMessage;
        public string MessageOverride; //null=no override, empty=empty message
        public float AmountRestored;
    }

    public class ComboAidItemRestoreNode
    {
        [JsonProperty]
        public AidType AType { get; private set; }
        [JsonProperty]
        public RestoreType RType { get; private set; }
        [JsonProperty]
        public float Amount { get; private set; }
        [JsonProperty]
        public bool AutoApply { get; private set; }
    }

    public class ComboAidItemModel : AidItemModel
    {
        [JsonProperty]
        public IReadOnlyList<ComboAidItemRestoreNode> RestoreNodes { get; protected set; } = new List<ComboAidItemRestoreNode>();

        [JsonConstructor]
        protected ComboAidItemModel(): base()
        {

        }

        public ComboAidItemModel(string name, float weight, float value, float maxCondition, int maxQuantity, bool hidden, bool essential, string[] flags, Dictionary<string, object> extraData, ItemScriptNode scripts, string worldModel,
            AidType aType, RestoreType rType, float amount, string script, ComboAidItemRestoreNode[] restoreNodes) : base(name, weight, value, maxCondition, maxQuantity, hidden, essential, flags, extraData, scripts, worldModel, aType, rType, amount, script)
        {
            RestoreNodes = restoreNodes;
        }

        public override AidItemApplyResult Apply(CharacterModel target, InventoryItemInstance itemInstance)
        {
            StringBuilder messageBuilder = new StringBuilder();
            AidItemScriptResult scriptResult = new AidItemScriptResult() { ReturnSuccess = true, ContinueApply = true, ConsumeItem = !CheckFlag(ItemFlag.AidDoNotConsume), ShowMessage = !CheckFlag(ItemFlag.AidNoMessage) };
            //apply script
            if(!string.IsNullOrEmpty(Scripts.OnApply))
            {
                scriptResult = ApplyScript(this, target, itemInstance);
            }

            if (scriptResult.ContinueApply)
            {
                //apply base
                if (AType != AidType.None)
                {
                    float amountRestored = ApplyBase(this, target);
                    messageBuilder.AppendLine(base.GetSuccessMessage(amountRestored));
                }

                //apply nodes
                if(RestoreNodes != null && RestoreNodes.Count > 0)
                {
                    foreach(var restoreNode in RestoreNodes)
                    {
                        if (!restoreNode.AutoApply)
                            continue;
                        float amountRestored = ApplyNode(restoreNode.AType, restoreNode.RType, restoreNode.Amount, target);
                        messageBuilder.AppendLine($"Restored {(int)amountRestored} {restoreNode.AType.ToString()}!");
                    }
                }
            }

            //concat messages
            string message;
            if(scriptResult.MessageOverride != null)
            {
                if (CheckFlag(ItemFlag.ComboAidKeepAllMessages))
                    message = scriptResult.MessageOverride + '\n' + messageBuilder.ToString();
                else
                    message = scriptResult.MessageOverride;
            }    
            else
            {
                message = messageBuilder.ToString();
            }

            return new AidItemApplyResult() { ConsumeItem = scriptResult.ConsumeItem, Message = message, ShowMessage = scriptResult.ShowMessage, Success = scriptResult.ReturnSuccess };

        }
    }
    

    public class MoneyItemModel : InventoryItemModel
    {
        [JsonConstructor]
        protected MoneyItemModel() : base()
        {
            Stackable = true;
        }

        public MoneyItemModel(string name, float weight, float value, float maxCondition, int maxQuantity, bool hidden, bool essential, string[] flags, Dictionary<string, object> extraData, ItemScriptNode scripts, string worldModel) :
            base(name, weight, value, maxCondition, maxQuantity, hidden, essential, flags, extraData, scripts, worldModel)
        {
            Stackable = true;
        }

        public override string GetStatsString()
        {
            return "<b>Currency</b>\n" + base.GetStatsString(); //should probably be a lookup
        }
    }

    public class AmmoItemModel : InventoryItemModel
    {
        [JsonConstructor]
        protected AmmoItemModel(): base()
        {
            Stackable = true;
        }

        public AmmoItemModel(string name, float weight, float value, float maxCondition, int maxQuantity, bool hidden, bool essential, string[] flags, Dictionary<string, object> extraData, ItemScriptNode scripts, string worldModel) :
            base(name, weight, value, maxCondition, maxQuantity, hidden, essential, flags, extraData, scripts, worldModel)
        {
            Stackable = true;
        }

        public override string GetStatsString()
        {
            return "<b>Ammunition</b>\n" + base.GetStatsString();
        }
    }
}
