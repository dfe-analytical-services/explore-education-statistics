#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IDataArchiveValidationService
    {
        Task<Either<ActionResult, ArchiveDataSetFile>> ValidateDataArchiveFile(
            Guid releaseVersionId,
            string dataSetTitle,
            IFormFile zipFile,
            File? replacingFile = null);

        Task<Either<ActionResult, List<ArchiveDataSetFile>>> ValidateBulkDataArchiveFiles(
            Guid releaseVersionId,
            IFormFile zipFile);

        Task<List<ErrorViewModel>> IsValidZipFile(IFormFile zipFile);
    }
}
