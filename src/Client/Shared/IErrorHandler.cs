// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Client.Data;

namespace ValhallaLootList.Client.Shared
{
    public interface IErrorHandler
    {
        void Handle(ProblemDetails problem);
    }
}