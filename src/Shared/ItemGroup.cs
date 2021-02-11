// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList
{
    public readonly struct ItemGroup : IEquatable<ItemGroup>, IComparable<ItemGroup>, IComparable
    {
        private const byte
            _head = 1, _neck = 2, _shoulder = 3, _chest = 4,
            _waist = 5, _legs = 6, _feet = 7, _wrist = 8,
            _hands = 9, _finger = 10, _trinket = 11, _back = 12,
            _weapon = 13, _offhand = 14, _misc = 15;

        private readonly byte _value;

        public ItemGroup(ItemType type, InventorySlot slot)
        {
            _value = slot switch
            {
                InventorySlot.Head => _head,
                InventorySlot.Neck => _neck,
                InventorySlot.Shoulder => _shoulder,
                InventorySlot.Chest => _chest,
                InventorySlot.Waist => _waist,
                InventorySlot.Legs => _legs,
                InventorySlot.Feet => _feet,
                InventorySlot.Wrist => _wrist,
                InventorySlot.Hands => _hands,
                InventorySlot.Finger => _finger,
                InventorySlot.Trinket => _trinket,
                InventorySlot.Back => _back,
                InventorySlot.MainHand or InventorySlot.OneHand or InventorySlot.TwoHand => _weapon,
                InventorySlot.OffHand => type switch
                {
                    ItemType.Other or ItemType.Shield => _offhand,
                    ItemType.Dagger or ItemType.Fist or ItemType.Axe or ItemType.Mace or ItemType.Sword => _weapon,
                    _ => default
                },
                InventorySlot.Ranged => type switch
                {
                    ItemType.Libram or ItemType.Idol or ItemType.Totem or ItemType.Wand => _misc,
                    ItemType.Bow or ItemType.Crossbow or ItemType.Gun or ItemType.Thrown => _weapon,
                    _ => default
                },
                _ => default
            };
        }

        public string Name => _value switch
        {
            _head => "Head",
            _neck => "Neck",
            _shoulder => "Shoulder",
            _chest => "Chest",
            _waist => "Waist",
            _legs => "Legs",
            _feet => "Feet",
            _wrist => "Wrist",
            _hands => "Hands",
            _finger => "Finger",
            _trinket => "Trinket",
            _back => "Back",
            _weapon => "Weapon",
            _offhand => "Offhand",
            _misc => "Special",
            _ => "Unknown",
        };

        public int CompareTo(ItemGroup other) => _value.CompareTo(other._value);

        public int CompareTo(object? obj) => _value.CompareTo((obj as ItemGroup?)?._value ?? 0);

        public bool Equals(ItemGroup other) => other._value == _value;

        public override bool Equals(object? obj) => obj is ItemGroup itemGroup2 && Equals(itemGroup2);

        public override int GetHashCode() => _value;

        public override string ToString() => Name;

        public static bool operator ==(ItemGroup left, ItemGroup right) => left.Equals(right);

        public static bool operator !=(ItemGroup left, ItemGroup right) => !(left == right);
    }
}
