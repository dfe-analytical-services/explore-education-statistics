#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

internal static class LocationMappingExtensions
{
    public static Either<ActionResult, LocationMapping> UpdateLocationMapping(
        this DataSetMapping dataSetMapping,
        IReadOnlyList<Location> replacementLocations,
        Guid originalId,
        Guid? newReplacementId = null
    )
    {
        if (!dataSetMapping.LocationMappings.TryGetValue(originalId, out var locationMapping))
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path = $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.OriginalId)}",
                    Code = "LocationMatchingOriginalIdNameNotFound",
                    Message = $"Could not find location mapping matching original location id \"{originalId}\"",
                }
            );
        }

        if (locationMapping.ReplacementId == newReplacementId && locationMapping.Status == MapStatus.ManuallySet)
        {
            return locationMapping; // it is already mapped, so can skip
        }

        if (
            newReplacementId != null
            && !IsLocationCandidateAvailable(
                dataSetMapping,
                locationMapping,
                replacementLocations,
                newReplacementId.Value
            )
        )
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                    Code = "UnmappedLocationMatchingReplacementLocationIdNotFound",
                    Message =
                        $"No available unmapped location matching replacement id \"{newReplacementId}\". DataSetMapping.Id: {dataSetMapping.Id}",
                }
            );
        }

        var replacement =
            newReplacementId == null ? null : replacementLocations.Single(location => location.Id == newReplacementId);

        if (replacement != null && replacement.GeographicLevel != locationMapping.OriginalGeographicLevel)
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                    Code = "UnmappedLocationHasDifferentGeographicLevelAsOriginalLocation",
                    Message =
                        $"The replacement location has a different geographic level than the original location. Replacement id: \"{newReplacementId}\"",
                }
            );
        }

        // mapping.Original* properties should never change
        locationMapping.ReplacementId = replacement?.Id;
        locationMapping.ReplacementGeographicLevel = replacement?.GeographicLevel;
        locationMapping.ReplacementCode = replacement?.ToLocationAttribute().GetCodeOrFallback();
        locationMapping.ReplacementName = replacement?.ToLocationAttribute().Name ?? "";
        locationMapping.Status = MapStatus.ManuallySet;

        return locationMapping;
    }

    private static bool IsLocationCandidateAvailable(
        DataSetMapping dataSetMapping,
        LocationMapping locationMapping,
        IReadOnlyList<Location> replacementLocations,
        Guid candidateId
    )
    {
        var candidateExists = replacementLocations.Any(candidate => candidate.Id == candidateId);

        var alreadyClaimed = dataSetMapping.LocationMappings.Values.Any(other =>
            other.OriginalId != locationMapping.OriginalId && other.ReplacementId == candidateId
        );

        return candidateExists && !alreadyClaimed;
    }
}
