using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ErrorDetails;

public record LocationAllowedLevelErrorDetail(string Value, IReadOnlyList<string> AllowedLevels)
    : InvalidErrorDetail<string>(Value);
