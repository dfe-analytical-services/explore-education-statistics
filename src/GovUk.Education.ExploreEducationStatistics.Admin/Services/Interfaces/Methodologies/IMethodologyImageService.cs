#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies
{
    public interface IMethodologyImageService
    {
        Task<Either<ActionResult, Unit>> DeleteAll(Guid methodologyVersionId, bool forceDelete = false);

        Task<Either<ActionResult, Unit>> Delete(
            Guid methodologyVersionId,
            IEnumerable<Guid> fileIds,
            bool forceDelete = false);

        Task<Either<ActionResult, FileStreamResult>> Stream(Guid methodologyVersionId, Guid fileId);

        Task<Either<ActionResult, ImageFileViewModel>> Upload(Guid methodologyVersionId, IFormFile formFile);
    }
}
