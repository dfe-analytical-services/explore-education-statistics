using IdentityServer4.EntityFramework.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.IdentityData
{
    public class ApplicationUserDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public ApplicationUserDbContext(
            DbContextOptions<ApplicationUserDbContext> options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
        }
    }
}
