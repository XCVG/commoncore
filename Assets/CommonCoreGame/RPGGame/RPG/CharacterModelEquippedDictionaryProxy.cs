using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CommonCore.RpgGame.Rpg
{
    public partial class CharacterModel
    {
        public interface IEquippedDictionaryProxy : IDictionary<int, InventoryItemInstance>
        {
            //TODO may implement IDictionary<EquipSlot, InventoryItemInstance>
            InventoryItemInstance this[EquipSlot key] { get; set; }
            bool ContainsKey(EquipSlot key);
            void Add(EquipSlot key, InventoryItemInstance value);
            bool Remove(EquipSlot key);
            bool TryGetValue(EquipSlot key, out InventoryItemInstance value);

        }

        //how equipped items are now handled
        private class EquippedDictionaryProxy : IEquippedDictionaryProxy
        {
            private CharacterModel CharacterModel;

            public EquippedDictionaryProxy(CharacterModel characterModel)
            {
                CharacterModel = characterModel;
            }

            public InventoryItemInstance this[EquipSlot key]
            {
                get => CharacterModel.Inventory.GetItem(CharacterModel.EquippedIDs[(int)key]);
                set => CharacterModel.EquippedIDs[(int)key] = value.InstanceUID;
            }

            public InventoryItemInstance this[int key]
            {
                get => CharacterModel.Inventory.GetItem(CharacterModel.EquippedIDs[key]);
                set => CharacterModel.EquippedIDs[key] = value.InstanceUID;
            }

            public ICollection<int> Keys => CharacterModel.EquippedIDs.Keys;

            public ICollection<InventoryItemInstance> Values => CharacterModel.EquippedIDs.Values.Select(v => CharacterModel.Inventory.GetItem(v)).ToArray();

            public int Count => CharacterModel.EquippedIDs.Count;

            public bool IsReadOnly => false;

            public void Add(int key, InventoryItemInstance value)
            {
                CharacterModel.EquippedIDs.Add(key, value.InstanceUID);
            }

            public void Add(KeyValuePair<int, InventoryItemInstance> item)
            {
                CharacterModel.EquippedIDs.Add(item.Key, item.Value.InstanceUID);
            }

            public void Add(EquipSlot key, InventoryItemInstance value)
            {
                Add((int)key, value);
            }

            public void Clear()
            {
                CharacterModel.EquippedIDs.Clear();
            }

            public bool Contains(KeyValuePair<int, InventoryItemInstance> item)
            {
                throw new NotImplementedException();
            }

            public bool ContainsKey(int key)
            {
                return CharacterModel.EquippedIDs.ContainsKey(key);
            }

            public bool ContainsKey(EquipSlot key)
            {
                return ContainsKey((int)key);
            }

            public void CopyTo(KeyValuePair<int, InventoryItemInstance>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<KeyValuePair<int, InventoryItemInstance>> GetEnumerator()
            {
                return CharacterModel.EquippedIDs.Select(kvp => new KeyValuePair<int, InventoryItemInstance>(kvp.Key, CharacterModel.Inventory.GetItem(kvp.Value))).GetEnumerator();
            }

            public bool Remove(int key)
            {
                return CharacterModel.EquippedIDs.Remove(key);
            }

            public bool Remove(KeyValuePair<int, InventoryItemInstance> item)
            {
                throw new NotImplementedException();
            }

            public bool Remove(EquipSlot key)
            {
                return Remove((int)key);
            }

            public bool TryGetValue(int key, out InventoryItemInstance value)
            {
                if (CharacterModel.EquippedIDs.TryGetValue(key, out long id))
                {
                    value = CharacterModel.Inventory.GetItem(id);
                    return true;
                }
                value = default;
                return false;
            }

            public bool TryGetValue(EquipSlot key, out InventoryItemInstance value)
            {
                return TryGetValue((int)key, out value);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}