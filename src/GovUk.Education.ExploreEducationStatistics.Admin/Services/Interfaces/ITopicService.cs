using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface ITopicService
    {
        Task<Either<ActionResult, TopicViewModel>> CreateTopic(TopicSaveViewModel created);

        Task<Either<ActionResult, TopicViewModel>> UpdateTopic(Guid id, TopicSaveViewModel updated);

        Task<Either<ActionResult, TopicViewModel>> GetTopic(Guid topicId);

        Task<Either<ActionResult, Unit>> DeleteTopic(Guid topicId);
    }
}