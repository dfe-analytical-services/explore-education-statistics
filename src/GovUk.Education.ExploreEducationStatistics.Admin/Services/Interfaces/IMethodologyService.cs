using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using TopicId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IMethodologyService
    {
        List<MethodologyViewModel> List();
        
        List<MethodologyViewModel> GetTopicMethodologies(TopicId topicId);
    }
}