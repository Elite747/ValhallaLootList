// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ValhallaLootList.Server.Data
{
    internal static class Seed
    {
        public static void EnsureSeeded(ApplicationDbContext context)
        {
            var instances = context.Instances.ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
            var bosses = context.Encounters.ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
            var itemIds = context.Items.Select(i => i.Id).ToHashSet();

            var rows = JsonSerializer.Deserialize<SeedItemRow[]>(File.ReadAllText("Data/tbcitems.json"));

            Debug.Assert(rows is not null);

            foreach (var bossesByInstance in rows.GroupBy(row => new { row.Source, row.Boss }).GroupBy(boss => boss.Key.Source))
            {
                var instanceName = bossesByInstance.Key;
                Debug.Assert(instanceName?.Length > 0);

                if (!instances.TryGetValue(instanceName, out var instance))
                {
                    instance = new Instance { Name = instanceName };
                    instances[instanceName] = instance;
                    context.Instances.Add(instance);
                }

                foreach (var itemsByBoss in bossesByInstance)
                {
                    var bossName = itemsByBoss.Key.Boss;
                    Debug.Assert(bossName?.Length > 0);

                    if (!bosses.TryGetValue(bossName, out var boss))
                    {
                        boss = new Encounter { Name = bossName, Instance = instance };
                        bosses[bossName] = boss;
                        context.Encounters.Add(boss);
                    }

                    foreach (var row in itemsByBoss)
                    {
                        if (!itemIds.Contains((uint)row.Id))
                        {
                            context.Items.Add(new Item
                            {
                                Agility = row.Agility,
                                Armor = row.Armor,
                                ArmorPenetration = row.ArmorPenetration,
                                BlockRating = row.BlockRating,
                                BlockValue = row.BlockValue,
                                Encounter = boss,
                                Defense = row.Defense,
                                Dodge = row.Dodge,
                                DPS = row.DPS,
                                Expertise = row.Expertise,
                                HasOnUse = row.HasOnUse,
                                HasProc = row.HasProc,
                                HasSpecial = row.HasSpecial,
                                Haste = row.Haste,
                                HealingPower = row.HealingPower,
                                HealthPer5 = row.HealthPer5,
                                Id = (uint)row.Id,
                                Intellect = row.Intellect,
                                ItemLevel = row.ItemLevel,
                                ManaPer5 = row.ManaPer5,
                                MeleeAttackPower = row.MeleeAttackPower,
                                MeleeCrit = row.MeleeCrit,
                                Name = row.Name ?? throw new Exception("Name cannot be empty."),
                                Parry = row.Parry,
                                PhysicalHit = row.PhysicalHit,
                                RangedAttackPower = row.RangedAttackPower,
                                RangedCrit = row.RangedCrit,
                                Resilience = row.Resilience,
                                RewardFromId = row.RewardFrom > 0 && row.RewardFrom != row.Id ? (uint)row.RewardFrom : null,
                                Slot = row.Slot,
                                Sockets = row.Sockets,
                                Speed = row.Speed,
                                SpellCrit = row.SpellCrit,
                                SpellHaste = row.SpellHaste,
                                SpellHit = row.SpellHit,
                                SpellPenetration = row.SpellPenetration,
                                SpellPower = row.SpellPower,
                                Spirit = row.Spirit,
                                Stamina = row.Stamina,
                                Strength = row.Strength,
                                TopEndDamage = row.TopEndDamage,
                                Type = row.Type,
                                UsableClasses = row.UsableClasses
                            });
                        }
                    }
                }
            }

            context.SaveChanges();
        }
        private class SeedItemRow
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Source { get; set; }
            public string? Boss { get; set; }
            public int RewardFrom { get; set; }
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
            public Specializations UsableSpecs { get; set; }
        }
    }
}
