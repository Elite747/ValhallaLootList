﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList;

public readonly struct ItemGroup : IEquatable<ItemGroup>, IComparable<ItemGroup>, IComparable
{
    private const byte
        _head = 1,
        _shoulder = 2,
        _chest = 3,
        _back = 4,
        _wrist = 5,
        _hands = 6,
        _waist = 7,
        _legs = 8,
        _feet = 9,
        _jewelry = 10,
        _trinket = 11,
        _weapon = 12,
        _misc = 13;

    private readonly byte _value;

    private ItemGroup(byte value)
    {
        _value = value;
    }

    public ItemGroup(ItemType type, InventorySlot slot)
    {
        _value = slot switch
        {
            InventorySlot.Head => _head,
            InventorySlot.Neck => _jewelry,
            InventorySlot.Shoulder => _shoulder,
            InventorySlot.Chest => _chest,
            InventorySlot.Waist => _waist,
            InventorySlot.Legs => _legs,
            InventorySlot.Feet => _feet,
            InventorySlot.Wrist => _wrist,
            InventorySlot.Hands => _hands,
            InventorySlot.Finger => _jewelry,
            InventorySlot.Trinket => _trinket,
            InventorySlot.Back => _back,
            InventorySlot.MainHand or InventorySlot.OneHand or InventorySlot.TwoHand or InventorySlot.OffHand => _weapon,
            InventorySlot.Ranged => type switch
            {
                ItemType.Libram or ItemType.Idol or ItemType.Totem or ItemType.Wand or ItemType.Sigil => _misc,
                ItemType.Bow or ItemType.Crossbow or ItemType.Gun or ItemType.Thrown => _weapon,
                _ => default
            },
            _ => default
        };
    }

    public static IEnumerable<ItemGroup> All
    {
        get
        {
            for (byte v = _head; v <= _misc; v++)
            {
                yield return new(v);
            }
        }
    }

    public string Name => _value switch
    {
        _head => "Head Armor",
        _shoulder => "Shoulder Armor",
        _chest => "Chest Armor",
        _waist => "Waist Armor",
        _legs => "Leg Armor",
        _feet => "Foot Armor",
        _wrist => "Wrist Armor",
        _hands => "Hand Armor",
        _jewelry => "Rings & Necklaces",
        _trinket => "Trinkets",
        _back => "Cloaks",
        _weapon => "Weapons & Offhands",
        _misc => "Wands & Relics",
        _ => "Unknown",
    };

    public int CompareTo(ItemGroup other)
    {
        return _value.CompareTo(other._value);
    }

    public int CompareTo(object? obj)
    {
        return _value.CompareTo((obj as ItemGroup?)?._value ?? 0);
    }

    public bool Equals(ItemGroup other)
    {
        return other._value == _value;
    }

    public override bool Equals(object? obj)
    {
        return obj is ItemGroup itemGroup2 && Equals(itemGroup2);
    }

    public override int GetHashCode()
    {
        return _value;
    }

    public override string ToString()
    {
        return Name;
    }

    public static bool operator ==(ItemGroup left, ItemGroup right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ItemGroup left, ItemGroup right)
    {
        return !(left == right);
    }
}
