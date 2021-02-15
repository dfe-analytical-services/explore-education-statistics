using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseImageService
    {
        Task<Either<ActionResult, FileStreamResult>> Stream(Guid releaseId, Guid fileId);

        Task<Either<ActionResult, ImageFileViewModel>> Upload(Guid releaseId, IFormFile formFile);
    }
}
