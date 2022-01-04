// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using IdGen;
using Microsoft.Extensions.Options;
using ValhallaLootList.Server.IdGeneration;

namespace Microsoft.Extensions.DependencyInjection.Extensions;

public static class IdGenServiceCollectionExtensions
{
    public static IServiceCollection AddIdGen(this IServiceCollection services, Action<IdGeneratorConfiguration>? config = null)
    {
        if (config is not null)
        {
            services.Configure(config);
        }

        services.TryAddSingleton<IIdGenerator<long>>(services =>
        {
            var config = services.GetRequiredService<IOptions<IdGeneratorConfiguration>>().Value;
            return new IdGenerator(config.GeneratorId, new(config.Structure, config.TimeSource, config.SequenceOverflowStrategy));
        });

        return services;
    }

    public static IServiceCollection ConfigureIdGen(this IServiceCollection services, Action<IdGeneratorConfiguration>? config)
    {
        services.Configure(config);
        return services;
    }
}
