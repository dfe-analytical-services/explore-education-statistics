using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IImportService
    {
        void Import(string dataFileName, Guid releaseId, IFormFile dataFile);

        Task<Either<ActionResult, bool>> CreateImportTableRow(Guid releaseId, string dataFileName);
    }
}