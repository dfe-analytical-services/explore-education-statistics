#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

public class PublicDataApiOptions
{
    public const string Section = "PublicDataApi";

    public string Url { get; init; } = string.Empty;

    public string DocsUrl { get; init; } = string.Empty;

    public Guid AppRegistrationClientId { get; init; }
}
