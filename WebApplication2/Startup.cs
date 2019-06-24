using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApplication2.Areas.Identity.Data;
using WebApplication2.Areas.UserManagement.AuthorizationHandler;
using WebApplication2.Areas.UserManagement.Constants;
using WebApplication2.Models;

namespace WebApplication2
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });



            services.AddDbContext<WebApplication2Context>(options =>
                  options.UseSqlServer(Configuration.GetConnectionString("WebApplication2ContextConnection")));


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyNames.AccessToTest1ScreenPolicy, policy => policy.Requirements.Add(new AuthorizationNameRequirement(PolicyNames.AccessToTest1ScreenPolicy)));
                options.AddPolicy(PolicyNames.AccessToTest2ScreenPolicy, policy => policy.Requirements.Add(new AuthorizationNameRequirement(PolicyNames.AccessToTest2ScreenPolicy)));
                options.AddPolicy(PolicyNames.AdministratorPolicy, policy => policy.Requirements.Add(new AuthorizationNameRequirement(PolicyNames.AdministratorPolicy)));
            });

            services.AddSingleton<IAuthorizationHandler, AuthorizationNameHandler>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areaRoute",
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            InitializeAdministrator(serviceProvider).Wait();
        }

        private async Task InitializeAdministrator(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<WebApplication2User>>();
            string roleName = "Administrator";
            IdentityResult roleResult;

            //create an administrator role
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (roleResult.Succeeded)
                {
                    //add the administrator related claim to the administrator role
                    Claim claim = new Claim(PolicyNames.AdministratorPolicy, "");
                    IdentityRole identityRole = await roleManager.FindByNameAsync(roleName);
                    IdentityResult claimResult = await roleManager.AddClaimAsync(identityRole, claim);
                    
                }
            }

            var administratorUser = new WebApplication2User
            {
                UserName = Configuration.GetSection("UserSettings")["UserName"],
                Surname = Configuration.GetSection("UserSettings")["UserSurname"],
                FirstName = Configuration.GetSection("UserSettings")["UserFirstname"]
            };

            string UserPassword = Configuration.GetSection("UserSettings")["UserPassword"];
            var adminUser = await userManager.FindByNameAsync(Configuration.GetSection("UserSettings")["UserName"]);

            if (adminUser == null)
            {
                IdentityResult identityUser = await userManager.CreateAsync(administratorUser, UserPassword);
                if (identityUser.Succeeded)
                {
                    //add the Administrator role to the user 
                    await userManager.AddToRoleAsync(administratorUser, roleName);

                }
            }
        }

    }
}
