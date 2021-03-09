using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces
{
    public interface IMethodologyImageService
    {
        Task<Either<ActionResult, FileStreamResult>> Stream(Guid methodologyId, Guid fileId);
    }
}
