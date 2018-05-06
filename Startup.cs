﻿using CoreDemo.Authorization;
using CoreDemo.Authorization.Twitter;
using CoreDemo.Caching;
using CoreDemo.Configuration;
using CoreDemo.Contexts;
using CoreDemo.Repos;
using CoreDemo.Services.Twitter;
using LinqToTwitter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSession();
            services.AddMvc();

            services.Configure<TwitterConfig>(Configuration.GetSection(nameof(TwitterConfig)));
            services.AddScoped<IAuthorizationManager<MvcAuthorizer>, AuthorizationManager<MvcAuthorizer>>();
            services.AddScoped<ISessionContext, SessionContext>();
            services.AddScoped<ITwitterRepo, TwitterRepo>();
            services.AddScoped<ITwitterService, TwitterService>();
            services.AddSingleton<IStaticCacheManager, MemoryCacheManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSession();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
