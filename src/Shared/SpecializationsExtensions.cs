// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;

namespace ValhallaLootList
{
    public static class SpecializationsExtensions
    {
        public static IEnumerable<Specializations> Split(this Specializations specs)
        {
            for (int i = 1; i <= 1 << 18; i <<= 1)
            {
                Specializations spec = (Specializations)i;

                if ((specs & spec) == spec)
                {
                    yield return spec;
                }
            }
        }

        public static bool IsSingleSpecialization(this Specializations spec)
        {
            int i = (int)spec;
            return i > 0 && i <= (1 << 18) && (i & (i - 1)) == 0;
        }

        public static bool IsClass(this Specializations spec, Classes playerClass)
        {
            if (!IsSingleSpecialization(spec))
            {
                throw new ArgumentOutOfRangeException(nameof(spec), "Parameter must be a single defined specialization.");
            }

            return (spec & playerClass.ToSpecializations()) != 0;
        }
    }
}
