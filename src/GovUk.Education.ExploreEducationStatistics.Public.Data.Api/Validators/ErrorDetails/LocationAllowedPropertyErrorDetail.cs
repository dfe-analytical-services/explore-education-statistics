using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ErrorDetails;

public record LocationAllowedPropertyErrorDetail(string Value, IEnumerable<string> AllowedProperties)
    : InvalidErrorDetail<string>(Value);
