#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IFileValidatorService
{
    Task<Either<ActionResult, Unit>> ValidateFileForUpload(IFormFile file, FileType type);
}
