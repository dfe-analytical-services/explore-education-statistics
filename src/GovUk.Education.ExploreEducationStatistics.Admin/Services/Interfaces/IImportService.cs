using System;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IImportService
    {
        void Import(string dataFileName, Guid releaseId, IFormFile dataFile);
    }
}