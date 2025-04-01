#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileUploadsValidatorService
    {
        Task<List<ErrorViewModel>> ValidateDataSet(
            Guid releaseVersionId,
            string dataSetTitle,
            List<DataSetFileDto> dataSetFiles,
            File? replacingFile = null);

        Task<Either<ActionResult, Unit>> ValidateFileForUpload(
            IFormFile file,
            FileType type);
    }
}
