#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Settings;

internal class PublicDataProcessorOptions
{
    public static readonly string Section = "PublicDataProcessor";

    public string Url { get; init; } = string.Empty;

    public Guid AppRegistrationClientId { get; init; }
}
