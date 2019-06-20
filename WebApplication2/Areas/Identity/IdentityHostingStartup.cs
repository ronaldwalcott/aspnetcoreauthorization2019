using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApplication2.Areas.Identity.Data;
using WebApplication2.Models;

[assembly: HostingStartup(typeof(WebApplication2.Areas.Identity.IdentityHostingStartup))]
namespace WebApplication2.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddDbContext<WebApplication2Context>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("WebApplication2ContextConnection")));

                services.AddDefaultIdentity<WebApplication2User>()
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<WebApplication2Context>();
            });
        }
    }
}