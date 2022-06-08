#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Guid = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserPublicationInviteRepository : IUserPublicationInviteRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public UserPublicationInviteRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task Create(Guid publicationId,
            string email,
            PublicationRole publicationRole,
            bool emailSent,
            Guid createdById,
            bool accepted = false)
        {
            await _contentDbContext.AddAsync(
                new UserPublicationInvite
                {
                    Email = email.ToLower(),
                    PublicationId = publicationId,
                    Role = publicationRole,
                    Accepted = accepted,
                    EmailSent = emailSent,
                    Created = DateTime.UtcNow,
                    CreatedById = createdById,
                }
            );

            await _contentDbContext.SaveChangesAsync();
        }
    }
}
