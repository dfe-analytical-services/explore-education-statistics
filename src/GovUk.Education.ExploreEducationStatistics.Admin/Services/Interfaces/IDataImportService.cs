#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IDataImportService
{
    Task<DataImport?> GetImport(Guid fileId);

    Task<Either<ActionResult, Unit>> CancelImport(Guid releaseVersionId,
        Guid fileId);

    Task DeleteImport(Guid fileId);

    Task<bool> HasIncompleteImports(Guid releaseVersionId);

    Task<DataImportStatusViewModel> GetImportStatus(Guid fileId);

    Task<DataImport> Import(Guid subjectId, File dataFile, File metaFile, File? sourceZipFile = null);
}
