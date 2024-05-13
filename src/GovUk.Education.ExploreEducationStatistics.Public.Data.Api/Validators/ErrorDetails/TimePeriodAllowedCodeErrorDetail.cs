using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ErrorDetails;

public record TimePeriodAllowedCodeErrorDetail(string Value, IReadOnlyList<string> AllowedCodes)
    : InvalidErrorDetail<string>(Value);
