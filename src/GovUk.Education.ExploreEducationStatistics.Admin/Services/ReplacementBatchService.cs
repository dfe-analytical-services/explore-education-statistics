#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using ValidationUtils = GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReplacementBatchService(
    ContentDbContext contentDbContext,
    IUserService userService,
    IReplacementService replacementService
) : IReplacementBatchService
{
    public async Task<Either<ActionResult, Unit>> Replace(
        Guid releaseVersionId,
        IEnumerable<Guid> originalFileIds,
        CancellationToken cancellationToken = default
    )
    {
        return await contentDbContext
            .ReleaseVersions.FirstOrNotFoundAsync(
                rv => rv.Id == releaseVersionId,
                cancellationToken: cancellationToken
            )
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async _ =>
            {
                var errors = new List<ErrorViewModel>();

                foreach (var originalFileId in originalFileIds)
                {
                    var replacementResult = await replacementService.Replace(
                        releaseVersionId: releaseVersionId,
                        originalFileId: originalFileId,
                        cancellationToken
                    );

                    if (replacementResult.IsLeft)
                    {
                        var error = GetReplaceError(replacementResult.Left, originalFileId);
                        errors.Add(error);
                    }
                }

                if (errors.Count > 0)
                {
                    return new Either<ActionResult, Unit>(ValidationUtils.ValidationResult(errors));
                }

                return Unit.Instance;
            });
    }

    private ErrorViewModel GetReplaceError(ActionResult actionResult, Guid originalFileId)
    {
        if (IsNotFound(actionResult))
        {
            return ValidationMessages.GenerateErrorReplacementNotFound(originalFileId);
        }

        if (HasValidationError(actionResult, ReplacementMustBeValid))
        {
            return ValidationMessages.GenerateErrorReplacementMustBeValid(originalFileId);
        }

        if (HasValidationError(actionResult, ReplacementImportMustBeComplete))
        {
            return ValidationMessages.GenerateErrorReplacementImportMustBeComplete(originalFileId);
        }

        return ValidationMessages.GenerateErrorReplacementError(originalFileId);
    }
}
