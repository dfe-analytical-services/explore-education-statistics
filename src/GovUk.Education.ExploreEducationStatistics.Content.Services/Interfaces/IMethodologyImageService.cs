#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces
{
    public interface IMethodologyImageService
    {
        Task<Either<ActionResult, FileStreamResult>> Stream(Guid methodologyVersionId, Guid fileId);
    }
}
