using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.IdentityHostingStartup))]
namespace GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
//            builder.ConfigureServices((context, services) => {
//                services.AddDbContext<UsersAndRolesDbContext>(options =>
//                    options.UseSqlServer(
//                        context.Configuration.GetConnectionString("ContentDb")));
//
//                services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
//                    .AddEntityFrameworkStores<UsersAndRolesDbContext>();
//            });
        }
    }
}