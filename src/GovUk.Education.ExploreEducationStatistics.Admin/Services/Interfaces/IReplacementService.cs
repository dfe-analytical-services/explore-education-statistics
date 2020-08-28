using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReplacementService
    {
        Task<Either<ActionResult, ReplacementPlan>> GetReplacementPlan(
            Guid originalSubjectId,
            Guid replacementSubjectId);
    }
}