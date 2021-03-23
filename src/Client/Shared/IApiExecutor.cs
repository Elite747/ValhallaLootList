// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Threading.Tasks;

namespace ValhallaLootList.Client.Shared
{
    public interface IApiExecutor
    {
        Task RestartAsync();
        Task StartAsync();
    }
}