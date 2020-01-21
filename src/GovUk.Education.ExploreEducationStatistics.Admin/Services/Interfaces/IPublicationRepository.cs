using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublicationRepository
    {
        Task<List<MyPublicationViewModel>> GetAllPublicationsForTopicAsync(Guid topicId);

        Task<List<MyPublicationViewModel>> GetPublicationsForTopicRelatedToUserAsync(Guid topicId, Guid userId);
    }
}