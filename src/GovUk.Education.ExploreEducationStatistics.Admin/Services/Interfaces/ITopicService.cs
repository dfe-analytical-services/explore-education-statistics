using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface ITopicService
    {
        Task<Topic> GetTopicAsync(Guid topicId);
    }
}