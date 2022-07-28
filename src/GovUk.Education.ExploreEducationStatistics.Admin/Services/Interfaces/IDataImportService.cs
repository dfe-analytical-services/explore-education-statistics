#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IDataImportService
    {
        Task<DataImport?> GetImport(Guid fileId);

        Task<Either<ActionResult, Unit>> CancelImport(Guid releaseId, Guid fileId);

        Task DeleteImport(Guid fileId);

        Task<bool> HasIncompleteImports(Guid releaseId);

        Task<DataImportStatusViewModel> GetImportStatus(Guid fileId);

        Task<DataImport> Import(Guid subjectId, File dataFile, File metaFile);

        Task<DataImport> ImportZip(Guid subjectId, File dataFile, File metaFile, File zipFile);
    }
}
