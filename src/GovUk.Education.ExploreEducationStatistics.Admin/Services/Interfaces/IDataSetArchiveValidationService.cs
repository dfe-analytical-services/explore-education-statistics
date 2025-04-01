#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IDataSetArchiveValidationService
    {
        Task<Either<ActionResult, List<ArchivedDataSet>>> ValidateBulkDataArchiveFiles(
            Guid releaseVersionId,
            List<DataSetFileDto> dataSetFiles);
    }
}
