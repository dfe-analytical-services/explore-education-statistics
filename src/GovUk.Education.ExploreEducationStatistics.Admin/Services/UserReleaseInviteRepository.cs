#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Guid = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserReleaseInviteRepository : IUserReleaseInviteRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public UserReleaseInviteRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task MarkInviteEmailAsSent(UserReleaseInvite invite)
        {
            invite.EmailSent = true;
            _contentDbContext.Update(invite);
            await _contentDbContext.SaveChangesAsync();
        }

        public async Task<bool> UserHasInvite(Guid releaseId, string email, ReleaseRole role)
        {
            return await _contentDbContext
                .UserReleaseInvites
                .AsQueryable()
                .AnyAsync(i =>
                    i.ReleaseId == releaseId
                    && i.Email.ToLower().Equals(email.ToLower())
                    && i.Role == role);
        }
    }
}
