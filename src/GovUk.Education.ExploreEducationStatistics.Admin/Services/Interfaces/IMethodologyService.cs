using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using TopicId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IMethodologyService
    {
        Task<List<MethodologyViewModel>> ListAsync();
        
        Task<List<MethodologyViewModel>> GetTopicMethodologiesAsync(TopicId topicId);
    }
}