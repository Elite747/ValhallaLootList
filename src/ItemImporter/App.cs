// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ValhallaLootList.ItemImporter.WarcraftDatabase;
using ValhallaLootList.Server.Data.Seeding;

namespace ValhallaLootList.ItemImporter
{
    internal class App : IHostedService
    {
        private readonly ILogger<App> _logger;
        private readonly WowDataContext _wowContext;
        private readonly Config _config;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private List<ItemTemplate> _itemTemplates;
        private List<SpellTemplate> _spellTemplates;

        public App(ILogger<App> logger, IOptions<Config> config, WowDataContext wowContext, IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
            _wowContext = wowContext;
            _config = config.Value;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("App started");

            await LoadTablesIntoMemoryAsync(cancellationToken);

            var items = new List<SeedItem>();

            foreach (var itemId in await LoadItemsFromSeedInstancesAsync(cancellationToken))
            {
                _logger.LogDebug($"Discovered Item #{itemId}");
                items.Add(ParseItem(itemId, null));

                if (_config.Tokens.TryGetValue(itemId, out var tokenRewards))
                {
                    foreach (var tokenRewardId in tokenRewards)
                    {
                        _logger.LogDebug($"Discovered Item #{tokenRewardId} as a reward from token #{itemId}.");
                        items.Add(ParseItem(tokenRewardId, itemId));
                    }
                }
            }

            _logger.LogInformation($"Parsed {items.Count} items. Saving to seed file.");

            await SaveItemsSeedAsync(items, cancellationToken);

            _logger.LogDebug("App finished");
            _hostApplicationLifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("App stopped");

            return Task.CompletedTask;
        }

        private async Task LoadTablesIntoMemoryAsync(CancellationToken cancellationToken)
        {
            _itemTemplates ??= await _wowContext.ItemTemplates.AsNoTracking().ToListAsync(cancellationToken);
            _spellTemplates ??= await _wowContext.SpellTemplates.AsNoTracking().ToListAsync(cancellationToken);
        }

        private async Task<IEnumerable<uint>> LoadItemsFromSeedInstancesAsync(CancellationToken cancellationToken)
        {
            using var fs = File.OpenRead(_config.SeedInstancesPath);
            var instances = await JsonSerializer.DeserializeAsync<List<SeedInstance>>(fs, cancellationToken: cancellationToken);
            return instances.SelectMany(i => i.Encounters).SelectMany(e => e.Items).Distinct().OrderBy(id => id);
        }

        private async Task SaveItemsSeedAsync(List<SeedItem> items, CancellationToken cancellationToken)
        {
            using var fs = File.Create(_config.SeedItemsPath);
            await JsonSerializer.SerializeAsync(fs, items, new() { WriteIndented = true }, cancellationToken);
        }

        private SeedItem ParseItem(uint id, uint? tokenId)
        {
            var itemTemplate = _itemTemplates.Find(x => x.Entry == id);

            if (itemTemplate is null)
            {
                _logger.LogWarning($"Item with ID {id} was not found! Item will not be parsed.");
                return null;
            }

            if (itemTemplate.Quality != 4)
            {
                _logger.LogWarning($"'{itemTemplate.Name}' ({id}) is not epic quality. Item will not be parsed.");
                return null;
            }

            var item = new SeedItem { Id = id, RewardFromId = tokenId };

            item.Name = itemTemplate.Name;

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

            ParseSpell(item, itemTemplate.Spellid1, itemTemplate.Spelltrigger1);
            ParseSpell(item, itemTemplate.Spellid2, itemTemplate.Spelltrigger2);
            ParseSpell(item, itemTemplate.Spellid3, itemTemplate.Spelltrigger3);
            ParseSpell(item, itemTemplate.Spellid4, itemTemplate.Spelltrigger4);
            ParseSpell(item, itemTemplate.Spellid5, itemTemplate.Spelltrigger5);

            if (itemTemplate.SocketColor1 != 0) item.Sockets++;
            if (itemTemplate.SocketColor2 != 0) item.Sockets++;
            if (itemTemplate.SocketColor3 != 0) item.Sockets++;

            var classes = (Classes)itemTemplate.AllowableClass;

            // 1535 and 32767 are used on items that have no class restrictions. (They equal all class flags plus extra unknown flags)
            if (classes > 0 && classes != (Classes)1535 && classes != (Classes)32767)
            {
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

                item.UsableClasses = (classes & allClasses);
            }

            _logger.LogInformation($"Finished parsing Item #{id}. '{item.Name}' will be added.");
            return item;
        }

        private void ParsePrimaryStat(SeedItem item, byte id, int value)
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

        private void ParseSpell(SeedItem item, uint spellId, int trigger)
        {
            if (spellId == 0)
            {
                return;
            }

            _logger.LogInformation($"Looking up spell for Item #{item.Id}...");

            var spell = _spellTemplates.Find(spell => spell.Id == spellId);

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

        private void ParseSpellEffect(SeedItem item, int basePoints, uint auraName, uint triggerSpell, int miscValue)
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