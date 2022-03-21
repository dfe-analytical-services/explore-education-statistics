using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies
{
    public interface IMethodologyAmendmentService
    {
        Task<Either<ActionResult, MethodologyVersionSummaryViewModel>> CreateMethodologyAmendment(
            Guid originalMethodologyVersionId);
    }
}
