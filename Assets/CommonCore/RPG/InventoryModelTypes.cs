using CommonCore.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace CommonCore.Rpg
{
    public enum MoneyType
    {
        Dollars
    }

    public enum AmmoType
    {
        NoAmmo, Acp32, Spc38, Acp45, R3006 //game dependent, redo for A3
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
    [JsonConverter(typeof(InventoryItemSerializer))]
    public class InventoryItemInstance
    {
        public const int UnstackableQuantity = -1;

        public int Quantity { get; set; }
        public float Condition { get; set; } //it's here but basically unimplemented
        public bool Equipped { get; set; }
        public readonly InventoryItemModel ItemModel;

        internal InventoryItemInstance(InventoryItemModel model, float condition, int quantity)
        {
            ItemModel = model;
            Condition = condition;
            Equipped = false;
            Quantity = quantity;
        }

        public InventoryItemInstance(InventoryItemModel model)
        {
            ItemModel = model;
            Condition = model.MaxCondition;
            Equipped = false;
            Quantity = model.Stackable ? 1 : UnstackableQuantity;
        }
    }

    public class InventoryItemSerializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var item = value as InventoryItemInstance;
            writer.WriteStartObject();
            writer.WritePropertyName("Condition");
            writer.WriteValue(item.Condition);
            writer.WritePropertyName("Quantity");
            writer.WriteValue(item.Quantity);
            writer.WritePropertyName("$ItemModel");
            writer.WriteValue(item.ItemModel.Name);
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            JObject jsonObject = JObject.Load(reader);
            float condition = jsonObject["Condition"].Value<float>();
            string modelName = jsonObject["$ItemModel"].Value<string>();
            int quantity = jsonObject["Quantity"].Value<int>();
            InventoryItemModel model = InventoryModel.GetModel(modelName);

            return new InventoryItemInstance(model, condition, quantity);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(InventoryItemInstance).IsAssignableFrom(objectType);
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
        public readonly string Name;
        public readonly float Weight;
        public readonly float Value;
        public readonly float MaxCondition;
        public readonly bool Unique;
        public readonly bool Essential;
        public bool Stackable { get; protected set; }

        public InventoryItemModel(string name, float weight, float value, float maxCondition, bool unique, bool essential)
        {
            Name = name;
            Weight = weight;
            MaxCondition = maxCondition;
            Unique = unique;
            Essential = essential;
            Stackable = false;
        }

        public virtual string GetStatsString()
        {
            return Essential ? "Essential" : string.Empty;
        }

    }

    public class MiscItemModel : InventoryItemModel
    {
        public MiscItemModel(string name, float weight, float value, float maxCondition, bool unique, bool essential)
            : base(name, weight, value, maxCondition, unique, essential)
        {
        }
    }

    public class WeaponItemModel : InventoryItemModel //yeah no melee... yet
    {
        public readonly float Damage; //superclass
        public readonly float DamagePierce; //superclass
        public readonly float Velocity; //ranged subclass
        public readonly float Spread; //ranged subclass
        public readonly float FireRate; //ranged subclass
        public readonly int MagazineSize; //ranged subclass
        public readonly float ReloadTime; //ranged subclass
        public readonly AmmoType AType; //ranged subclass
        public readonly DamageType DType; //superclass
        public readonly string ViewModel; //superclass
        public readonly string WorldModel; //superclass

        public WeaponItemModel(string name, float weight, float value, float maxCondition, bool unique, bool essential,
            float damage, float damagePierce, float velocity, float spread, float fireRate,
            int magazineSize, float reloadTime, AmmoType aType, DamageType dType, string viewModel, string worldModel)
            : base(name, weight, value, maxCondition, unique, essential)
        {
            Damage = damage;
            DamagePierce = damagePierce;
            Velocity = velocity;
            Spread = spread;
            FireRate = fireRate;
            MagazineSize = magazineSize;
            ReloadTime = reloadTime;
            AType = aType;
            DType = dType;
            ViewModel = viewModel;
            WorldModel = worldModel;
        }

        public override string GetStatsString()
        {
            StringBuilder str = new StringBuilder(255);
            //TODO finish impl

            return str.ToString() + base.GetStatsString();
        }
    }

    public class ArmorItemModel : InventoryItemModel //TODO damage types but I'm lazy
    {
        public readonly float DamageResistance;
        public readonly float DamageThreshold;

        public ArmorItemModel(string name, float weight, float value, float maxCondition, bool unique, bool essential,
            float damageResistance, float damageThreshold)
            : base(name, weight, value, maxCondition, unique, essential)
        {
            DamageResistance = damageResistance;
            DamageThreshold = damageThreshold;
        }
    }

    public class AidItemModel : InventoryItemModel
    {
        public readonly AidType AType;
        public readonly RestoreType RType;
        public float Amount;

        public AidItemModel(string name, float weight, float value, float maxCondition, bool unique, bool essential,
            AidType aType, RestoreType rType, float amount)
            : base(name, weight, value, maxCondition, unique, essential)
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

        public MoneyItemModel(string name, float weight, float value, float maxCondition, bool unique, bool essential, MoneyType type) : base(name, weight, value, maxCondition, unique, essential)
        {
            Type = type;
            Stackable = true;
        }
    }

    public class AmmoItemModel : InventoryItemModel
    {
        public readonly AmmoType Type;

        public AmmoItemModel(string name, float weight, float value, float maxCondition, bool unique, bool essential, AmmoType type) : base(name, weight, value, maxCondition, unique, essential)
        {
            Type = type;
            Stackable = true;
        }
    }
}
