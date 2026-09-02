#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

internal static class IndicatorMappingExtensions
{
    public static Either<ActionResult, IndicatorMapping> UpdateIndicatorMapping(
        this DataSetMapping dataSetMapping,
        IReadOnlyList<Indicator> replacementIndicators,
        Guid originalId,
        Guid? newReplacementId = null
    )
    {
        if (!dataSetMapping.IndicatorMappings.TryGetValue(originalId, out var indicatorMapping))
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(IndicatorMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.OriginalId)}",
                    Code = "IndicatorMatchingOriginalIdNotFound",
                    Message = $"Could not find indicator mapping matching original id \"{originalId}\"",
                }
            );
        }

        if (indicatorMapping.ReplacementId == newReplacementId && indicatorMapping.Status == MapStatus.ManuallySet)
        {
            return indicatorMapping; // it is already mapped, so can skip
        }

        if (
            newReplacementId != null
            && !IsIndicatorCandidateAvailable(
                dataSetMapping,
                indicatorMapping,
                replacementIndicators,
                newReplacementId.Value
            )
        )
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(IndicatorMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                    Code = "UnmappedIndicatorMatchingReplacementIdNotFound",
                    Message =
                        $"No available unmapped indicator matching replacement id \"{newReplacementId}\". DataSetMapping.Id: {dataSetMapping.Id}",
                }
            );
        }

        var replacement =
            newReplacementId == null
                ? null
                : replacementIndicators.Single(indicator => indicator.Id == newReplacementId);

        // mapping.Original* properties should never change
        indicatorMapping.ReplacementId = replacement?.Id;
        indicatorMapping.ReplacementColumnName = replacement?.Name;
        indicatorMapping.ReplacementLabel = replacement?.Label;
        indicatorMapping.ReplacementGroupId = replacement?.IndicatorGroupId;
        indicatorMapping.ReplacementGroupLabel = replacement?.IndicatorGroup.Label;
        indicatorMapping.Status = MapStatus.ManuallySet;

        return indicatorMapping;
    }

    private static bool IsIndicatorCandidateAvailable(
        DataSetMapping dataSetMapping,
        IndicatorMapping indicatorMapping,
        IReadOnlyList<Indicator> replacementIndicators,
        Guid candidateId
    )
    {
        var candidateExists = replacementIndicators.Any(candidate => candidate.Id == candidateId);

        var alreadyClaimed = dataSetMapping.IndicatorMappings.Values.Any(other =>
            other.OriginalId != indicatorMapping.OriginalId && other.ReplacementId == candidateId
        );

        return candidateExists && !alreadyClaimed;
    }
}
