using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface ITopicService
    {
        Task<Either<ActionResult, TopicViewModel>> CreateTopic(Guid themeId, CreateTopicRequest request);

        Task<Either<ActionResult, TopicViewModel>> GetTopic(Guid topicId);
    }
}