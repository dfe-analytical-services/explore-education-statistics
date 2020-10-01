using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IImportService
    {
        Task Import(Guid releaseId, string dataFileName, string metaFileName, IFormFile dataFile, bool isZip);
        Task<Either<ActionResult, Unit>> CreateImportTableRow(Guid releaseId, string dataFileName);
        Task FailImport(Guid releaseId, string dataFileName, IEnumerable<ValidationError> errors);
        Task RemoveImportTableRowIfExists(Guid releaseId, string dataFileName);
    }
}
