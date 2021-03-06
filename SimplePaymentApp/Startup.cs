﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimplePaymentApp.Services;
using AutoMapper;

namespace SimplePaymentApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder();

            builder.SetBasePath(env.ContentRootPath);
            builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            builder.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            builder.AddEnvironmentVariables();
            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }
            if (env.IsProduction())
            {
                builder.AddJsonFile("azurekeyvault.json", optional:false, reloadOnChange: true);
                var config = builder.Build();
                builder.AddAzureKeyVault($"https://{config["azureKeyVault:vault"]}.vault.azure.net/",
                    config["azureKeyVault:clientId"],
                    config["azureKeyVault:clientSecret"]);
            }

            Configuration = builder.Build();
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var paymill = Configuration.GetSection("PayMill");
            services.Configure<PaymillSettings>(paymill);
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddScoped<IHttpClient, HttpClientService>();

            this.ConfigureAuth(services);
            services.AddMvc();
            services.AddAutoMapper(x => x.AddProfile(new MappingEntity()));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            
            app.Use(async (context, next) =>
            {
                context.Request.Scheme = "https";
                await next.Invoke();
            });

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        protected virtual void ConfigureAuth(IServiceCollection services)
        {
            services.AddAuthentication(o =>
            {
                o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddAzureAd(o => Configuration.Bind("AzureAd", o))
            .AddCookie();
        }
    }
}
