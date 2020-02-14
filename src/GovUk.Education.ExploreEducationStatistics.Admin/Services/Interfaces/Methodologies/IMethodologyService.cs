using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies
{
    // TODO EES-919 - return ActionResults rather than ValidationResults
    public interface IMethodologyService
    {
        Task<Either<ActionResult, List<MethodologyViewModel>>> ListAsync();
        
        Task<Either<ActionResult, List<MethodologyStatusViewModel>>> ListStatusAsync();
        
        Task<Either<ActionResult, MethodologyViewModel>> GetAsync(Guid id);
        
        Task<Either<ActionResult, List<MethodologyViewModel>>> GetTopicMethodologiesAsync(Guid topicId);
        
        Task<Either<ActionResult, MethodologyViewModel>> CreateMethodologyAsync(
            CreateMethodologyViewModel methodology);
    }
}