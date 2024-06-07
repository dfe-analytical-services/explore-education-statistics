#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IDataArchiveValidationService
    {
        Task<Either<ActionResult, ArchiveDataSetFile>> ValidateDataArchiveFile(
            Guid releaseVersionId,
            string dataSetFileName,
            IFormFile zipFile,
            File? replacingFile = null);

        Task<Either<ActionResult, List<ArchiveDataSetFile>>> ValidateBulkDataArchiveFile(
            Guid releaseVersionId,
            IFormFile zipFile);
    }
}
