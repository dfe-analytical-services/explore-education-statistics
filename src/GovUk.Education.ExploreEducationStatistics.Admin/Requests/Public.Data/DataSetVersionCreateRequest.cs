#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record DataSetVersionCreateRequest
{
    public required Guid ReleaseFileId { get; init; }

    public class Validator : AbstractValidator<DataSetVersionCreateRequest>
    {
        public Validator(IDataSetVersionRepository dataSetVersionRepository)
        {
            RuleFor(request => request.ReleaseFileId).MustAsync(async (releaseFileId, cancellationToken) => 
                    await dataSetVersionRepository.GetByReleaseFileId(
                        releaseFileId: releaseFileId,
                        cancellationToken: cancellationToken)
                    is null)
                .WithErrorCode(ValidationMessages.HasExistingApiDataSetVersion.Code)
                .WithMessage(ValidationMessages.HasExistingApiDataSetVersion.Message);
        }
    }
}
