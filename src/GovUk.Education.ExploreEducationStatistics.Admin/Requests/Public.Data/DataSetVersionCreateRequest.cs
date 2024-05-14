#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record DataSetVersionCreateRequest
{
    public required Guid ReleaseFileId { get; init; }

    public class Validator : AbstractValidator<DataSetVersionCreateRequest>
    {
        public Validator(IDataSetVersionService dataSetVersionService)
        {
            RuleFor(request => request.ReleaseFileId)
                .NotEmpty()
                .MustAsync(async (releaseFileId, cancellationToken) => 
                    await dataSetVersionService.FileHasVersion(
                        releaseFileId: releaseFileId,
                        cancellationToken: cancellationToken)
                    is false)
                .WithErrorCode(ValidationMessages.FileHasApiDataSetVersion.Code)
                .WithMessage(ValidationMessages.FileHasApiDataSetVersion.Message);
        }
    }
}
