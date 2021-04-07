// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class Item
    {
        public Item(uint id)
        {
            if (id == 0U)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }
            Id = id;
        }

        [Key]
        public uint Id { get; }

        [Required]
        public string Name { get; set; } = null!;

        public uint? RewardFromId { get; set; }

        public Item? RewardFrom { get; set; }

        public InventorySlot Slot { get; set; }

        public ItemType Type { get; set; }

        public int ItemLevel { get; set; }

        public int TopEndDamage { get; set; }

        public double DPS { get; set; }

        public double Speed { get; set; }

        public int Armor { get; set; }

        public int Strength { get; set; }

        public int Agility { get; set; }

        public int Stamina { get; set; }

        public int Intellect { get; set; }

        public int Spirit { get; set; }

        public int PhysicalHit { get; set; }

        public int SpellHit { get; set; }

        public int MeleeCrit { get; set; }

        public int RangedCrit { get; set; }

        public int SpellCrit { get; set; }

        public int Haste { get; set; }

        public int SpellHaste { get; set; }

        public int Defense { get; set; }

        public int Dodge { get; set; }

        public int BlockRating { get; set; }

        public int BlockValue { get; set; }

        public int Parry { get; set; }

        public int SpellPower { get; set; }

        public int HealingPower { get; set; }

        public int ManaPer5 { get; set; }

        public int HealthPer5 { get; set; }

        public int MeleeAttackPower { get; set; }

        public byte Phase { get; set; }

        public int RangedAttackPower { get; set; }

        public int Resilience { get; set; }

        public int Expertise { get; set; }

        public int ArmorPenetration { get; set; }

        public int SpellPenetration { get; set; }

        public int Sockets { get; set; }

        public bool HasProc { get; set; }

        public bool HasOnUse { get; set; }

        public bool HasSpecial { get; set; }

        public Classes? UsableClasses { get; set; }

        public virtual Encounter? Encounter { get; set; }

        public string? EncounterId { get; set; }

        public virtual ICollection<Drop> Drops { get; set; } = new HashSet<Drop>();

        public virtual ICollection<ItemRestriction> Restrictions { get; set; } = new HashSet<ItemRestriction>();
    }
}
