// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Text;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data;

public class ApiClientMembers
{
    public ApiClientMembers(ApiClient client)
    {
        Client = client;
    }

    public ApiClient Client { get; }

    public IApiClientOperation<IList<GuildMemberDto>> GetAll()
    {
        return Client.CreateRequest<IList<GuildMemberDto>>(HttpMethod.Get, "api/v1/members");
    }

    public IApiClientOperation<IList<GuildMemberDto>> GetInRole(string role)
    {
        return Client.CreateRequest<IList<GuildMemberDto>>(HttpMethod.Get, "api/v1/members?role=" + role);
    }

    public IApiClientOperation<IList<GuildMemberDto>> GetInRoles(params string[] roles)
    {
        if (roles.Length == 0)
        {
            return GetAll();
        }

        if (roles.Length == 1)
        {
            return GetInRole(roles[0]);
        }

        var pathBuilder = new StringBuilder("api/v1/members?role=").Append(roles[0]);

        for (int i = 1; i < roles.Length; i++)
        {
            pathBuilder.Append("&role=").Append(roles[i]);
        }

        return Client.CreateRequest<IList<GuildMemberDto>>(HttpMethod.Get, pathBuilder.ToString());
    }
}
