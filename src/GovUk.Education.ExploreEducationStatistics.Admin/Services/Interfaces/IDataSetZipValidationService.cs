#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IDataSetZipValidationService
    {
        Task<Either<ActionResult, List<ZippedDataSet>>> ValidateBulkDataZipFiles(
            Guid releaseVersionId,
            List<DataSetFileDto> dataSetFiles);
    }
}
