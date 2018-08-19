using CommonCore.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonCore.Rpg
{
    public enum MoneyType
    {
        Gold
    }

    public enum AmmoType
    {
        NoAmmo, Para9, Acp45, Nato556, Nato762, Sa68, Shot12, Slug, Arrow, Bolt //game dependent, redo for A3
    }

    public enum AidType //are there even any other stats?
    {
        None, Health
    }

    public enum RestoreType
    {
        Add, Boost, //boost allows going over max, add does not
        Override //override replaces
    }

    //an actual inventory item that the player has
    [JsonObject(IsReference = true)]
    public class InventoryItemInstance
    {
        //public const int UnstackableQuantity = -1;

        public int Quantity { get; set; }
        public float Condition { get; set; } //it's here but basically unimplemented
        public bool Equipped { get; set; }
        [JsonProperty, JsonConverter(typeof(InstanceItemConverter))]
        public InventoryItemModel ItemModel { get; private set; }

        [JsonConstructor]
        internal InventoryItemInstance(InventoryItemModel model, float condition, int quantity, bool equipped)
        {
            ItemModel = model;
            Condition = condition;
            Equipped = equipped;
            Quantity = quantity;
        }

        public InventoryItemInstance(InventoryItemModel model)
        {
            ItemModel = model;
            Condition = model.MaxCondition;
            Equipped = false;
            Quantity = 1;
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
        public string NiceName;
        public string Image;
        public string Description;

        public InventoryItemDef(string niceName, string image, string description)
        {
            NiceName = niceName;
            Image = image;
            Description = description;
        }

        public override string ToString()
        {
            return string.Format("[{0}:{1}\t{2}]", NiceName, Image, Description);
        }
    }

    //base class for invariant inventory items
    public abstract class InventoryItemModel
    {
        public readonly string Name; //this is incredibly inelegant and we will find a way to autoset this at some point
        public readonly float Weight;
        public readonly float Value;
        public readonly float MaxCondition;
        public readonly bool Unique;
        public readonly bool Essential;
        public readonly string WorldModel;

        public bool Stackable { get; protected set; }

        public InventoryItemModel(string name, float weight, float value, float maxCondition, bool unique, bool essential, string worldModel)
        {
            Name = name;
            Weight = weight;
            Value = value;
            MaxCondition = maxCondition;
            Unique = unique;
            Essential = essential;
            Stackable = false;
            WorldModel = worldModel;
        }

        public virtual string GetStatsString()
        {
            return Essential ? "Essential" : string.Empty;
        }

    }

    public class MiscItemModel : InventoryItemModel
    {
        public MiscItemModel(string name, float weight, float value, float maxCondition, bool unique, bool essential, string worldModel)
            : base(name, weight, value, maxCondition, unique, essential, worldModel)
        {
        }
    }

    public abstract class WeaponItemModel : InventoryItemModel
    {
        public readonly float Damage; //superclass
        public readonly float DamagePierce; //superclass
        public readonly DamageType DType; //superclass
        public readonly string ViewModel; //superclass

        public WeaponItemModel(string name, float weight, float value, float maxCondition, bool unique, bool essential, float damage, float damagePierce, DamageType dType, string viewModel, string worldModel)
            : base(name, weight, value, maxCondition, unique, essential, worldModel)
        {
            Damage = damage;
            DamagePierce = damagePierce;
            DType = dType;
            ViewModel = viewModel;
        }
    }

    public class MeleeWeaponItemModel : WeaponItemModel
    {
        public readonly float Reach;
        public readonly float Rate;
        public readonly float EnergyCost;

        public MeleeWeaponItemModel(string name, float weight, float value, float maxCondition, bool unique, bool essential, float damage, float damagePierce, float reach, float rate, float energyCost, DamageType dType, string viewModel, string worldModel) 
            : base(name, weight, value, maxCondition, unique, essential, damage, damagePierce, dType, viewModel, worldModel)
        {
            Reach = reach;
            Rate = rate;
            EnergyCost = energyCost;
        }
    }

    public class RangedWeaponItemModel : WeaponItemModel //yeah no melee... yet
    {        
        public readonly float Velocity; //ranged subclass
        public readonly float Spread; //ranged subclass
        public readonly float FireRate; //ranged subclass
        public readonly int MagazineSize; //ranged subclass
        public readonly float ReloadTime; //ranged subclass
        public readonly AmmoType AType; //ranged subclass        

        public RangedWeaponItemModel(string name, float weight, float value, float maxCondition, bool unique, bool essential,
            float damage, float damagePierce, float velocity, float spread, float fireRate,
            int magazineSize, float reloadTime, AmmoType aType, DamageType dType, string viewModel, string worldModel)
            : base(name, weight, value, maxCondition, unique, essential, damage, damagePierce, dType, viewModel, worldModel)
        {
            Velocity = velocity;
            Spread = spread;
            FireRate = fireRate;
            MagazineSize = magazineSize;
            ReloadTime = reloadTime;
            AType = aType;
            
        }

        public override string GetStatsString()
        {
            StringBuilder str = new StringBuilder(255);
            //TODO finish impl

            return str.ToString() + base.GetStatsString();
        }
    }

    public class ArmorItemModel : InventoryItemModel
    {
        public readonly Dictionary<DamageType, float> DamageResistance;
        public readonly Dictionary<DamageType, float> DamageThreshold;

        public ArmorItemModel(string name, float weight, float value, float maxCondition, bool unique, bool essential, string worldModel,
            Dictionary<DamageType, float> damageResistance, Dictionary<DamageType, float> damageThreshold)
            : base(name, weight, value, maxCondition, unique, essential, worldModel)
        {
            DamageResistance = new Dictionary<DamageType, float>(damageResistance);
            DamageThreshold = new Dictionary<DamageType, float>(damageThreshold);
        }
    }

    public class AidItemModel : InventoryItemModel
    {
        public readonly AidType AType;
        public readonly RestoreType RType;
        public float Amount;

        public AidItemModel(string name, float weight, float value, float maxCondition, bool unique, bool essential, string worldModel,
            AidType aType, RestoreType rType, float amount)
            : base(name, weight, value, maxCondition, unique, essential, worldModel)
        {
            AType = aType;
            RType = rType;
            Amount = amount;
        }

        public void Apply()
        {
            Apply(this, GameState.Instance.PlayerRpgState);
        }

        public static void Apply(AidItemModel item, CharacterModel player)
        {
            switch (item.AType)
            {
                case AidType.Health:
                    {
                        switch (item.RType)
                        {
                            case RestoreType.Add:
                                player.Health = Math.Min(player.Health + item.Amount, player.DerivedStats.MaxHealth);
                                break;
                            case RestoreType.Boost:
                                player.Health += item.Amount;
                                break;
                            case RestoreType.Override:
                                player.Health = item.Amount;
                                break;
                            default:
                                break;
                        }
                    }
                    break;                
            }
        }
    }

    public class MoneyItemModel : InventoryItemModel
    {
        public readonly MoneyType Type;

        public MoneyItemModel(string name, float weight, float value, float maxCondition, bool unique, bool essential, string worldModel, MoneyType type) : base(name, weight, value, maxCondition, unique, essential, worldModel)
        {
            Type = type;
            Stackable = true;
        }
    }

    public class AmmoItemModel : InventoryItemModel
    {
        public readonly AmmoType Type;

        public AmmoItemModel(string name, float weight, float value, float maxCondition, bool unique, bool essential, string worldModel, AmmoType type) : base(name, weight, value, maxCondition, unique, essential, worldModel)
        {
            Type = type;
            Stackable = true;
        }
    }
}
