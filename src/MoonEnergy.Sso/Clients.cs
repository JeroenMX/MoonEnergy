﻿// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Duende.IdentityServer.Models;

namespace MoonEnergy.Sso;

public static class Clients
{
    public static IEnumerable<Client> List =>
        new []
        {
            new Client
            {
                ClientId = "webapp",
                RequireClientSecret = false,
                
                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:5000/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:5000/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:5000/signout-callback-oidc" },

                AllowOfflineAccess = false,
                AllowedScopes = { "openid", "profile", "email" }
            }
        };
}