using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent
{
    public interface IRelatedInformationService
    {
        Task<Either<ActionResult, List<Link>>> GetRelatedInformationAsync(Guid releaseId);
        
        Task<Either<ActionResult, List<Link>>> AddRelatedInformationAsync(Guid releaseId, CreateUpdateLinkRequest request);
        
        Task<Either<ActionResult, List<Link>>> UpdateRelatedInformationAsync(Guid releaseId, Guid relatedInformationId, CreateUpdateLinkRequest request);

        Task<Either<ActionResult, List<Link>>> DeleteRelatedInformationAsync(Guid releaseId, Guid relatedInformationId);
    }
}
