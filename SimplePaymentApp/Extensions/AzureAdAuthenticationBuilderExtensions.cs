﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication
{
    public static class AzureAdAuthenticationBuilderExtensions
    {        
        public static AuthenticationBuilder AddAzureAd(this AuthenticationBuilder builder)
            => builder.AddAzureAd(_ => { });

        public static AuthenticationBuilder AddAzureAd(this AuthenticationBuilder builder, Action<AzureAdOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureAzureOptions>();
            builder.AddOpenIdConnect();
            return builder;
        }

        private class ConfigureAzureOptions: IConfigureNamedOptions<OpenIdConnectOptions>
        {
            private readonly AzureAdOptions _azureOptions;

            public ConfigureAzureOptions(IOptions<AzureAdOptions> azureOptions)
            {
                _azureOptions = azureOptions.Value;
            }

            public void Configure(string name, OpenIdConnectOptions o)
            {
                o.ClientId = _azureOptions.ClientId;
                o.Authority = $"{_azureOptions.Instance}common/v2.0";   // V2 specific
                o.UseTokenLifetime = true;
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters.ValidateIssuer = false;     // accept several tenants
                o.Events = new OpenIdConnectEvents {
                    OnRemoteFailure = ctx =>
                    {
                        ctx.Response.Redirect("/Account/SignIn"); // https://github.com/aspnet/Security/issues/1165#issuecomment-289522941
                        ctx.HandleResponse();
                        return Task.FromResult(0);
                    }
                };
            }

            public void Configure(OpenIdConnectOptions options)
            {
                Configure(Options.DefaultName, options);
            }
        }
    }
}
