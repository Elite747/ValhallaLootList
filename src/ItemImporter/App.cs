// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ValhallaLootList.ItemImporter.WarcraftDatabase;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemImporter
{
    internal class App : IHostedService
    {
        private readonly ILogger<App> _logger;
        private readonly WowDataContext _wowContext;
        private readonly Config _config;
        private readonly ApplicationDbContext _appContext;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private List<ItemTemplate> _itemTemplatesCache;

        public App(ILogger<App> logger, IOptions<Config> config, WowDataContext wowContext, ApplicationDbContext appContext, IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
            _wowContext = wowContext;
            _config = config.Value;
            _appContext = appContext;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("App started");

            await _appContext.Database.MigrateAsync(cancellationToken);

            foreach (var key in _config.Instances.Keys)
            {
                await foreach (var item in ParseZoneLootAsync(key, cancellationToken).WithCancellation(cancellationToken))
                {
                    _logger.LogInformation($"Added {item.Name} ({item.Id}) to the application context.");
                }
            }

            var changes = await _appContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Application context saved with {changes} changes.");

            _logger.LogInformation("App finished");
            _hostApplicationLifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("App stopped");

            return Task.CompletedTask;
        }

        private static ValueTask<T> FindAsync<T>(DbSet<T> items, Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default) where T : class
        {
            var item = items.Local.FirstOrDefault(expression.Compile());

            if (item is null)
            {
                return new ValueTask<T>(items.FirstOrDefaultAsync(expression, cancellationToken));
            }

            return new ValueTask<T>(item);
        }

        private async IAsyncEnumerable<Item> ParseZoneLootAsync(string zone, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var instanceConfig = _config.Instances[zone];
            foreach (var id in instanceConfig.Items)
            {
                _logger.LogInformation($"Discovered item:{id} in zone {zone}");

                var instance = await FindAsync(_appContext.Instances, i => i.Name == zone, cancellationToken);

                if (instance is null)
                {
                    instance = new Instance { Name = zone, Phase = instanceConfig.Phase };
                    _appContext.Instances.Add(instance);
                }
                else
                {
                    instance.Phase = instanceConfig.Phase;
                }

                var item = await ParseItemAsync(id, instance, null, cancellationToken);

                if (item is not null)
                {
                    yield return item;

                    if (_config.Tokens.TryGetValue(item.Id, out var tokenRewards))
                    {
                        foreach (var rewardId in tokenRewards)
                        {
                            var subitem = await ParseItemAsync(rewardId, instance, item.Encounter, cancellationToken);
                            if (item is not null)
                            {
                                _logger.LogInformation($"'{item.Name}' ({item.Id}) is a token for acquiring '{subitem.Name}' ({subitem.Id}). RewardFrom was set for this item.");
                                subitem.RewardFrom = item;
                                yield return subitem;
                            }
                        }
                    }
                }
            }
        }

        private async Task<Item> ParseItemAsync(uint id, Instance instance, Encounter encounter, CancellationToken cancellationToken)
        {
            var itemTemplate = (_itemTemplatesCache ??= (await _wowContext.ItemTemplates.AsNoTracking().ToListAsync(cancellationToken))).Find(x => x.Entry == id);

            if (itemTemplate is null)
            {
                _logger.LogWarning($"Item with ID {id} was not found! Item will not be parsed.");
                return null;
            }

            _logger.LogInformation($"Parsing Item #{id}...");

            if (itemTemplate.Quality != 4)
            {
                _logger.LogWarning($"'{itemTemplate.Name}' ({id}) is not epic quality. Item will not be parsed.");
                return null;
            }

            var item = await _appContext.Items.FindAsync(new object[] { id }, cancellationToken);

            if (item is null)
            {
                item = new Item { Id = id };
                _appContext.Items.Add(item);
            }
            else
            {
                // block value can be listed more than once, so make it cumulative. Are other stats like this?
                item.BlockValue = 0;
            }

            item.Name = itemTemplate.Name;
            item.Encounter = encounter ?? await GetOrCreateEncounterAsync(id, instance, cancellationToken);
            item.EncounterId = item.Encounter.Id;

            switch (itemTemplate.InventoryType)
            {
                case 0:
                    break;
                case 1:
                    item.Slot = InventorySlot.Head;
                    goto case 99;
                case 2:
                    item.Slot = InventorySlot.Neck;
                    break;
                case 3:
                    item.Slot = InventorySlot.Shoulder;
                    goto case 99;
                case 4:
                    item.Slot = InventorySlot.Shirt;
                    break;
                case 5:
                    item.Slot = InventorySlot.Chest;
                    goto case 99;
                case 6:
                    item.Slot = InventorySlot.Waist;
                    goto case 99;
                case 7:
                    item.Slot = InventorySlot.Legs;
                    goto case 99;
                case 8:
                    item.Slot = InventorySlot.Feet;
                    goto case 99;
                case 9:
                    item.Slot = InventorySlot.Wrist;
                    goto case 99;
                case 10:
                    item.Slot = InventorySlot.Hands;
                    goto case 99;
                case 11:
                    item.Slot = InventorySlot.Finger;
                    break;
                case 12:
                    item.Slot = InventorySlot.Trinket;
                    break;
                case 13:
                    item.Slot = InventorySlot.OneHand;
                    goto case 98;
                case 14:
                    item.Slot = InventorySlot.OffHand;
                    item.Type = ItemType.Shield;
                    break;
                case 15:
                    item.Slot = InventorySlot.Ranged;
                    item.Type = ItemType.Bow;
                    break;
                case 16:
                    item.Slot = InventorySlot.Back;
                    break;
                case 17:
                    item.Slot = InventorySlot.TwoHand;
                    goto case 98;
                case 19:
                    item.Slot = InventorySlot.Tabard;
                    break;
                case 20:
                    item.Slot = InventorySlot.Chest;
                    goto case 99;
                case 21:
                    item.Slot = InventorySlot.MainHand;
                    goto case 98;
                case 22:
                    item.Slot = InventorySlot.OffHand;
                    goto case 98;
                case 23:
                    item.Slot = InventorySlot.OffHand;
                    break;
                case 25:
                    item.Slot = InventorySlot.Ranged;
                    item.Type = ItemType.Thrown;
                    break;
                case 26:
                    item.Slot = InventorySlot.Ranged;
                    item.Type = itemTemplate.Subclass switch
                    {
                        3 => ItemType.Gun,
                        18 => ItemType.Crossbow,
                        19 => ItemType.Wand,
                        _ => default
                    };
                    break;
                case 28:
                    item.Slot = InventorySlot.Ranged;
                    item.Type = itemTemplate.Subclass switch
                    {
                        7 => ItemType.Libram,
                        8 => ItemType.Idol,
                        9 => ItemType.Totem,
                        _ => default
                    };
                    break;
                case 98: // parse weapon type
                    item.Type = itemTemplate.Subclass switch
                    {
                        0 or 1 => ItemType.Axe,
                        4 or 5 => ItemType.Mace,
                        6 => ItemType.Polearm,
                        7 or 8 => ItemType.Sword,
                        10 => ItemType.Stave,
                        13 => ItemType.Fist,
                        15 => ItemType.Dagger,
                        _ => default
                    };
                    break;
                case 99: // parse armor type
                    item.Type = itemTemplate.Subclass switch
                    {
                        1 => ItemType.Cloth,
                        2 => ItemType.Leather,
                        3 => ItemType.Mail,
                        4 => ItemType.Plate,
                        _ => default
                    };
                    break;
                default:
                    _logger.LogWarning($"'{itemTemplate.Name}' ({itemTemplate.Entry}) has an unexpected InventoryType value of {itemTemplate.InventoryType}!");
                    break;
            }

            item.ItemLevel = itemTemplate.ItemLevel;

            if (itemTemplate.DmgMax1 > 0)
            {
                item.TopEndDamage = (int)itemTemplate.DmgMax1;
                item.Speed = ((double)itemTemplate.Delay / 1000.0);
                item.DPS = (itemTemplate.DmgMax1 + itemTemplate.DmgMin1) / 2 / item.Speed;
            }

            item.Armor = itemTemplate.Armor;

            ParsePrimaryStat(item, itemTemplate.StatType1, itemTemplate.StatValue1);
            ParsePrimaryStat(item, itemTemplate.StatType2, itemTemplate.StatValue2);
            ParsePrimaryStat(item, itemTemplate.StatType3, itemTemplate.StatValue3);
            ParsePrimaryStat(item, itemTemplate.StatType4, itemTemplate.StatValue4);
            ParsePrimaryStat(item, itemTemplate.StatType5, itemTemplate.StatValue5);
            ParsePrimaryStat(item, itemTemplate.StatType6, itemTemplate.StatValue6);
            ParsePrimaryStat(item, itemTemplate.StatType7, itemTemplate.StatValue7);
            ParsePrimaryStat(item, itemTemplate.StatType8, itemTemplate.StatValue8);
            ParsePrimaryStat(item, itemTemplate.StatType9, itemTemplate.StatValue9);
            ParsePrimaryStat(item, itemTemplate.StatType10, itemTemplate.StatValue10);

            await ParseSpellAsync(item, itemTemplate.Spellid1, itemTemplate.Spelltrigger1, cancellationToken);
            await ParseSpellAsync(item, itemTemplate.Spellid2, itemTemplate.Spelltrigger2, cancellationToken);
            await ParseSpellAsync(item, itemTemplate.Spellid3, itemTemplate.Spelltrigger3, cancellationToken);
            await ParseSpellAsync(item, itemTemplate.Spellid4, itemTemplate.Spelltrigger4, cancellationToken);
            await ParseSpellAsync(item, itemTemplate.Spellid5, itemTemplate.Spelltrigger5, cancellationToken);

            if (itemTemplate.SocketColor1 != 0) item.Sockets++;
            if (itemTemplate.SocketColor2 != 0) item.Sockets++;
            if (itemTemplate.SocketColor3 != 0) item.Sockets++;

            var classes = (Classes)itemTemplate.AllowableClass;

            if (classes > 0)
            {
                item.UsableClasses = classes;

                const Classes allClasses =
                    Classes.Druid |
                    Classes.Hunter |
                    Classes.Mage |
                    Classes.Paladin |
                    Classes.Priest |
                    Classes.Rogue |
                    Classes.Shaman |
                    Classes.Warlock |
                    Classes.Warrior;

                if ((classes & ~allClasses) != 0)
                {
                    _logger.LogInformation($"'{itemTemplate.Name}' ({itemTemplate.Entry}) has an unexpected AllowableClass value of {itemTemplate.AllowableClass}!");
                }
            }

            _logger.LogInformation($"Finished parsing Item #{id}. '{item.Name}' will be added.");
            return item;
        }

        private async Task<Encounter> GetOrCreateEncounterAsync(uint itemId, Instance instance, CancellationToken cancellationToken)
        {
            var name = await GetEncounterNameAsync(itemId, cancellationToken);

            if (name?.Length > 0)
            {
                var encounter = await FindAsync(_appContext.Encounters, e => e.Name == name, cancellationToken);

                if (encounter is null)
                {
                    encounter = new Encounter { Name = name };
                    _appContext.Encounters.Add(encounter);
                }

                encounter.Instance = instance;

                return encounter;
            }

            return null;
        }

        private async Task<string> GetEncounterNameAsync(uint itemId, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Looking up encounter for Item #{itemId}...");
            if (_config.BossOverrides.TryGetValue(itemId, out string bossName))
            {
                _logger.LogInformation($"Item #{itemId} was hard-coded with the boss '{bossName}'.");
                return bossName;
            }

            uint lootId = itemId;

            var rlt = await _wowContext.ReferenceLootTemplates
                .AsNoTracking()
                .Where(x => x.Item == itemId)
                .Select(x => new { x.Entry })
                .FirstOrDefaultAsync(cancellationToken);

            if (rlt is not null)
            {
                _logger.LogInformation($"Found reference loot template #{rlt.Entry} for Item #{itemId}.");
                lootId = rlt.Entry;
            }

            var bosses = new HashSet<string>();

            await foreach (var boss in (from creature in _wowContext.CreatureTemplates
                                        join creatureLoot in _wowContext.CreatureLootTemplates
                                        on creature.Entry equals creatureLoot.Entry
                                        where creatureLoot.Item == lootId
                                        select creature.Name)
                                        .Concat(
                                        from obj in _wowContext.GameobjectTemplates
                                        join objLoot in _wowContext.GameobjectLootTemplates
                                        on (uint)obj.Data1 equals objLoot.Entry
                                        where objLoot.Item == lootId
                                        select obj.Name)
                                        .AsAsyncEnumerable()
                                        .WithCancellation(cancellationToken))
            {
                _logger.LogInformation($"Item #{itemId} was found to drop from '{boss}'.");
                if (_config.BossNameOverrides.TryGetValue(boss, out bossName))
                {
                    _logger.LogInformation($"Overriding boss name '{boss}' to '{bossName}'.");
                    bosses.Add(bossName);
                }
                else
                {
                    bosses.Add(boss);
                }
            }

            if (bosses.Count > 1)
            {
                _logger.LogWarning($"Item #{itemId} was found on more than one boss!");
                return string.Join(", ", bosses);
            }

            if (bosses.Count == 0)
            {
                _logger.LogError($"Could not find the boss for item #{itemId}!");
                return string.Empty;
            }

            return bosses.First();
        }

        private void ParsePrimaryStat(Item item, byte id, int value)
        {
            switch (id)
            {
                case 0: return;
                case 3: item.Agility = value; return;
                case 4: item.Strength = value; return;
                case 5: item.Intellect = value; return;
                case 6: item.Spirit = value; return;
                case 7: item.Stamina = value; return;
                case 12: item.Defense = value; return;
                case 13: item.Dodge = value; return;
                case 14: item.Parry = value; return;
                case 15: item.BlockRating = value; return;
                case 18: item.SpellHit = value; return;
                case 19: item.MeleeCrit = value; return;
                case 20: item.RangedCrit = value; return;
                case 21: item.SpellCrit = value; return;
                case 30: item.SpellHaste = value; return;
                case 31: item.PhysicalHit = value; return;
                case 32: item.MeleeCrit = item.RangedCrit = value; return;
                case 35: item.Resilience = value; return;
                case 36: item.Haste = value; return;
                case 37: item.Expertise = value; return;
                default:
                    _logger.LogWarning($"'{item.Name}' ({item.Id}) has an unknown primary stat of {id}: {value}.");
                    return;
            }
        }

        private async Task ParseSpellAsync(Item item, uint spellId, int trigger, CancellationToken cancellationToken)
        {
            if (spellId == 0)
            {
                return;
            }

            _logger.LogInformation($"Looking up spell for Item #{item.Id}...");

            var spell = await _wowContext.SpellTemplates.FindAsync(new object[] { spellId }, cancellationToken);

            if (spell is null)
            {
                _logger.LogError($"'{item.Name}' ({item.Id}) has an unknown spell #{spellId}!");
                return;
            }

            if (trigger == 0) // on-use
            {
                // TODO
                _logger.LogWarning($"'{item.Name}' ({item.Id}) has an on-use effect that will prevent auto-determination!");
                item.HasOnUse = true;
            }
            if (trigger == 1) // passive
            {
                ParseSpellEffect(item, spell.EffectBasePoints1, spell.EffectApplyAuraName1, spell.EffectTriggerSpell1, spell.EffectMiscValue1);
                ParseSpellEffect(item, spell.EffectBasePoints2, spell.EffectApplyAuraName2, spell.EffectTriggerSpell2, spell.EffectMiscValue2);
                ParseSpellEffect(item, spell.EffectBasePoints3, spell.EffectApplyAuraName3, spell.EffectTriggerSpell3, spell.EffectMiscValue3);
            }
            else if (trigger == 2) // on-hit
            {
                // TODO
                _logger.LogWarning($"'{item.Name}' ({item.Id}) has a proc effect that will prevent auto-determination!");
                item.HasProc = true;
            }

            _logger.LogInformation($"Finished parsing spell '{spell.SpellName}' for item '{item.Name}'.");
        }

        private void ParseSpellEffect(Item item, int basePoints, uint auraName, uint triggerSpell, int miscValue)
        {
            if (triggerSpell > 0)
            {
                //todo: procs
                _logger.LogWarning($"'{item.Name}' ({item.Id}) has a spell proc effect that will prevent auto-determination!");
                item.HasProc = true;
                return;
            }

            switch (auraName)
            {
                case 0: return;
                case 13:
                    if (miscValue == 126)
                    {
                        // void star talisman breaks this rule and reports spellpower as healing power. No idea why. Bug in the game possibly?
                        // Easier to just manually override this one item as it's the only one in TBC loot that has this exception.
                        if (item.Id == 30449)
                        {
                            item.SpellPower = 1 + basePoints;
                        }
                        else
                        {
                            item.HealingPower = 1 + basePoints;
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"'{item.Name}' ({item.Id}) has a special effect that will prevent auto-determination!");
                        item.HasSpecial = true;
                    }
                    return;
                case 85: item.ManaPer5 = 1 + basePoints; return;
                case 99: item.MeleeAttackPower = 1 + basePoints; return;
                case 123:
                    if (miscValue is 1)
                    {
                        item.ArmorPenetration = Math.Abs(basePoints) - 2;
                    }
                    else if (miscValue is 124 or 4 or 16)
                    {
                        item.SpellPenetration = Math.Abs(basePoints) - 2;
                    }
                    else
                    {
                        _logger.LogWarning($"'{item.Name}' ({item.Id}) has a special effect that will prevent auto-determination!");
                        item.HasSpecial = true;
                    }
                    return;
                case 124: item.RangedAttackPower = 1 + basePoints; return;
                case 135:
                    if (miscValue == 126)
                    {
                        item.SpellPower = 1 + basePoints;
                    }
                    else
                    {
                        _logger.LogWarning($"'{item.Name}' ({item.Id}) has a special effect that will prevent auto-determination!");
                        item.HasSpecial = true;
                    }
                    return;
                case 158: item.BlockValue += 1 + basePoints; return;
                case 4: // 38320 = improved seal of light
                case 107: // 38321 = improve healing touch
                case 108: // 37447 = improved mana gem
                case 112: // 37447 = improved mana gem
                case 234: // 35126 = silence resistance
                default:
                    item.HasSpecial = true;
                    _logger.LogWarning($"'{item.Name}' ({item.Id}) has a special effect that will prevent auto-determination!");
                    return;
            }
        }
    }
}