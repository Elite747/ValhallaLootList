// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public static class ItemDtoExtensions
{
    public static IEnumerable<RestrictionDto> GetRestrictions(this ItemDto item, Specializations mainSpec, Specializations offSpec = Specializations.None, bool allowsOffspec = false)
    {
        var mainspecRestrictions = item.Restrictions.Where(r => (r.Specs & mainSpec) != 0);

        if (allowsOffspec && offSpec != mainSpec && offSpec != Specializations.None)
        {
            var offspecRestrictions = item.Restrictions.Where(r => (r.Specs & offSpec) != 0);

            bool offSpecHasAny = offspecRestrictions.Any();

            if (mainspecRestrictions.Any() ^ offSpecHasAny)
            {
                return Array.Empty<RestrictionDto>();
            }
            else if (offSpecHasAny && offspecRestrictions.All(r => r.Level == ItemRestrictionLevel.ManualReview))
            {
                // return the "least restricted" of the two specs.
                return offspecRestrictions;
            }
        }

        return mainspecRestrictions;
    }
}
