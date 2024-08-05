#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Settings;

internal class PublicDataProcessorOptions
{
    public const string Section = "PublicDataProcessor";

    public string Url { get; init; } = string.Empty;

    public Guid AppRegistrationClientId { get; init; }
}
