#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Settings;

public class PublicDataApiOptions
{
    public const string Section = "PublicDataApi";

    public string Url { get; init; } = string.Empty;

    public Guid AppRegistrationClientId { get; init; }
}
