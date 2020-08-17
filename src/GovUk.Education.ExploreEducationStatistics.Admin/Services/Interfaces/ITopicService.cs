using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface ITopicService
    {
        Task<Either<ActionResult, TopicViewModel>> CreateTopic(SaveTopicViewModel createdTopic);

        Task<Either<ActionResult, TopicViewModel>> UpdateTopic(Guid id, SaveTopicViewModel updatedTopic);

        Task<Either<ActionResult, TopicViewModel>> GetTopic(Guid topicId);

        Task<Either<ActionResult, Unit>> DeleteTopic(Guid topicId);
    }
}