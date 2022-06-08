#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserPublicationInviteRepository
    {
        Task Create(Guid publicationId,
            string email,
            PublicationRole publicationRole,
            bool emailSent,
            Guid createdById,
            bool accepted = false);
    }
}
