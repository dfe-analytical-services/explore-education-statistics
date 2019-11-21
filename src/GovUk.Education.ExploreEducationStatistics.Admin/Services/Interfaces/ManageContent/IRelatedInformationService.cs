using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent
{
    public interface IRelatedInformationService
    {
        Task<Either<ValidationResult, List<BasicLink>>> GetRelatedInformationAsync(Guid releaseId);
        
        Task<Either<ValidationResult, List<BasicLink>>> AddRelatedInformationAsync(Guid releaseId, CreateUpdateLinkRequest request);
        
        Task<Either<ValidationResult, List<BasicLink>>> UpdateRelatedInformationAsync(Guid releaseId, Guid relatedInformationId, CreateUpdateLinkRequest request);

        Task<Either<ValidationResult, List<BasicLink>>> DeleteRelatedInformationAsync(Guid releaseId, Guid relatedInformationId);
    }
}