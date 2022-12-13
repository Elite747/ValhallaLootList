// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers;

public class PhasesController : ApiControllerV1
{
    private readonly ApplicationDbContext _context;

    public PhasesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet, Authorize(AppPolicies.Administrator)]
    public IAsyncEnumerable<PhaseDto> Get()
    {
        return _context.PhaseDetails.AsNoTracking()
            .OrderBy(p => p.Id)
            .Select(p => new PhaseDto
            {
                Phase = p.Id,
                StartsAt = p.StartsAt,
                Brackets = _context.Brackets
                    .AsNoTracking()
                    .Where(b => b.Phase == p.Id)
                    .Select(b => new BracketDto
                    {
                        AllowOffspec = b.AllowOffspec,
                        AllowTypeDuplicates = b.AllowTypeDuplicates,
                        HeroicItems = b.HeroicItems,
                        MaxRank = b.MaxRank,
                        MinRank = b.MinRank,
                        NormalItems = b.NormalItems
                    })
                    .ToList()
            })
            .AsAsyncEnumerable();
    }

    [HttpGet("{id:int}"), Authorize(AppPolicies.Administrator)]
    public async Task<ActionResult<PhaseDto>> Get(int id)
    {
        var dto = await _context.PhaseDetails
            .AsNoTracking()
            .Where(pd => pd.Id == id)
            .Select(p => new PhaseDto { Phase = p.Id, StartsAt = p.StartsAt })
            .FirstOrDefaultAsync();

        if (dto is null)
        {
            return NotFound();
        }

        dto.Brackets = await _context.Brackets
            .AsNoTracking()
            .Where(b => b.Phase == id)
            .OrderBy(b => b.Index)
            .Select(b => new BracketDto
            {
                AllowOffspec = b.AllowOffspec,
                AllowTypeDuplicates = b.AllowTypeDuplicates,
                HeroicItems = b.HeroicItems,
                MaxRank = b.MaxRank,
                MinRank = b.MinRank,
                NormalItems = b.NormalItems
            })
            .ToListAsync();

        return dto;
    }

    [HttpPost, Authorize(AppPolicies.Administrator)]
    public async Task<ActionResult<PhaseDto>> Post([FromBody] PhaseDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem();
        }

        if (await _context.PhaseDetails.CountAsync(pd => pd.Id == dto.Phase) > 0)
        {
            ModelState.AddModelError(nameof(dto.Phase), "This phase already exists.");
            return ValidationProblem();
        }

        _context.PhaseDetails.Add(new PhaseDetails(dto.Phase, dto.StartsAt));

        for (byte i = 0; i < dto.Brackets.Count; i++)
        {
            BracketDto? bracket = dto.Brackets[i];
            _context.Brackets.Add(new Bracket
            {
                AllowOffspec = bracket.AllowOffspec,
                AllowTypeDuplicates = bracket.AllowTypeDuplicates,
                HeroicItems = (byte)bracket.HeroicItems,
                Index = i,
                MaxRank = (byte)bracket.MaxRank,
                MinRank = (byte)bracket.MinRank,
                NormalItems = (byte)bracket.NormalItems,
                Phase = dto.Phase
            });
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = dto.Phase }, dto);
    }

    [HttpPut("{id:int}"), Authorize(AppPolicies.Administrator)]
    public async Task<ActionResult<PhaseDto>> Put(int id, [FromBody] PhaseDto dto, [FromServices] IdGen.IIdGenerator<long> idGenerator)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem();
        }

        var phase = await _context.PhaseDetails.FindAsync((byte)id);

        if (phase is null)
        {
            return NotFound();
        }

        if (await _context.CharacterLootLists.AnyAsync(ll => ll.Phase == id && ll.Status == LootListStatus.Locked))
        {
            return Problem("Phase has locked loot lists and cannot be edited.");
        }

        phase.StartsAt = dto.StartsAt;

        var brackets = await _context.Brackets.Where(b => b.Phase == id).OrderBy(b => b.Index).ToListAsync();
        var bracketsToAdd = new List<Bracket>();

        var existingLists = await _context.CharacterLootLists.Where(e => e.Phase == id).Include(e => e.Entries).ToListAsync();

        while (brackets.Count < dto.Brackets.Count)
        {
            var bracket = new Bracket { Phase = (byte)id };
            brackets.Add(bracket);
            bracketsToAdd.Add(bracket);
        }

        while (brackets.Count > dto.Brackets.Count)
        {
            var bracket = brackets[^1];
            _context.Brackets.Remove(bracket);
            brackets.Remove(bracket);
        }

        for (byte i = 0; i < brackets.Count; i++)
        {
            var bracketDto = dto.Brackets[i];
            var bracket = brackets[i];
            bracket.Index = i;
            bracket.AllowOffspec = bracketDto.AllowOffspec;
            bracket.AllowTypeDuplicates = bracketDto.AllowTypeDuplicates;
            bracket.MaxRank = (byte)bracketDto.MaxRank;
            bracket.MinRank = (byte)bracketDto.MinRank;
            bracket.HeroicItems = (byte)bracketDto.HeroicItems;
            bracket.NormalItems = (byte)bracketDto.NormalItems;
            if (bracketsToAdd.Contains(bracket))
            {
                _context.Brackets.Add(bracket);
            }
        }

        foreach (var list in existingLists)
        {
            foreach (var entry in list.Entries.Where(e => !dto.Brackets.Any(b => e.Rank >= b.MinRank && e.Rank <= b.MaxRank)).ToList())
            {
                list.Entries.Remove(entry);
                _context.LootListEntries.Remove(entry);
            }
            foreach (var bracket in dto.Brackets)
            {
                var entries = list.Entries.Where(e => e.Rank >= bracket.MinRank && e.Rank <= bracket.MaxRank).ToLookup(e => e.Rank);
                for (byte rank = (byte)bracket.MinRank; rank <= bracket.MaxRank; rank++)
                {
                    var rankEntries = entries[rank];
                    var normalDiff = rankEntries.Count(e => !e.Heroic) - bracket.NormalItems;
                    var heroicDiff = rankEntries.Count(e => e.Heroic) - bracket.HeroicItems;

                    if (normalDiff < 0)
                    {
                        normalDiff = -normalDiff;
                        for (int i = 0; i < normalDiff; i++)
                        {
                            _context.LootListEntries.Add(new(idGenerator.CreateId()) { LootList = list, Rank = rank });
                        }
                    }
                    else if (normalDiff > 0)
                    {
                        foreach (var entry in rankEntries.Where(e => !e.Heroic).TakeLast(normalDiff).ToList())
                        {
                            list.Entries.Remove(entry);
                            _context.LootListEntries.Remove(entry);
                        }
                    }

                    if (heroicDiff < 0)
                    {
                        heroicDiff = -heroicDiff;
                        for (int i = 0; i < heroicDiff; i++)
                        {
                            _context.LootListEntries.Add(new(idGenerator.CreateId()) { Heroic = true, LootList = list, Rank = rank });
                        }
                    }
                    else if (heroicDiff > 0)
                    {
                        foreach (var entry in rankEntries.Where(e => e.Heroic).TakeLast(heroicDiff).ToList())
                        {
                            list.Entries.Remove(entry);
                            _context.LootListEntries.Remove(entry);
                        }
                    }
                }
            }
        }

        await _context.SaveChangesAsync();

        return dto;
    }

    [HttpDelete("{id:int}"), Authorize(AppPolicies.Administrator)]
    public async Task<ActionResult<PhaseDto>> Delete(int id)
    {
        var phase = await _context.PhaseDetails
            .AsNoTracking()
            .Where(pd => pd.Id == id)
            .FirstOrDefaultAsync();

        if (phase is null)
        {
            return NotFound();
        }

        if (await _context.CharacterLootLists.AnyAsync(ll => ll.Phase == id && ll.Status == LootListStatus.Locked))
        {
            return Problem("Phase has locked loot lists and cannot be deleted.");
        }

        var dto = new PhaseDto
        {
            StartsAt = phase.StartsAt,
            Phase = phase.Id,
            Brackets = await _context.Brackets
                .AsNoTracking()
                .Where(b => b.Phase == id)
                .OrderBy(b => b.Index)
                .Select(b => new BracketDto
                {
                    AllowOffspec = b.AllowOffspec,
                    AllowTypeDuplicates = b.AllowTypeDuplicates,
                    HeroicItems = b.HeroicItems,
                    MaxRank = b.MaxRank,
                    MinRank = b.MinRank,
                    NormalItems = b.NormalItems
                })
                .ToListAsync()
        };

        _context.PhaseDetails.Remove(phase);
        _context.Brackets.RemoveRange(await _context.Brackets.Where(b => b.Phase == id).ToListAsync());
        _context.LootListEntries.RemoveRange(await _context.LootListEntries.Where(e => e.LootList.Phase == id).ToListAsync());
        _context.CharacterLootLists.RemoveRange(await _context.CharacterLootLists.Where(l => l.Phase == id).ToListAsync());

        await _context.SaveChangesAsync();

        return dto;
    }
}
