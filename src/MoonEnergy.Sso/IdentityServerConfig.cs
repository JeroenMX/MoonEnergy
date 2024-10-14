using Duende.IdentityServer.Models;

namespace MoonEnergy.Sso;

public static class IdentityServerConfig
{
    public static IEnumerable<IdentityResource> IdentityResources => new List<IdentityResource>
    {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResources.Email()
    };
}