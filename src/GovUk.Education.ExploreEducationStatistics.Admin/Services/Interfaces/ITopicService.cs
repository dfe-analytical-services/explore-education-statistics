using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface ITopicService
    {
        Task<Topic> GetTopicAsync(Guid topicId);

        Task<Either<ValidationResult, TopicViewModel>> CreateTopicAsync(
            CreateTopicViewModel topic
        );
    }
}