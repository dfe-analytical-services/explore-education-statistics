using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IMethodologyService
    {
        Task<List<MethodologyViewModel>> ListAsync();
        
        Task<List<MethodologyStatusViewModel>> ListStatusAsync();
        
        Task<List<MethodologyViewModel>> GetTopicMethodologiesAsync(Guid topicId);
        
        Task<Either<ValidationResult, MethodologyViewModel>> CreateMethodologyAsync(
            CreateMethodologyViewModel methodology);
    }
}