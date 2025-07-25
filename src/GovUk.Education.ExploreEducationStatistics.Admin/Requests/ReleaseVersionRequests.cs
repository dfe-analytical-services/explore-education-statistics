#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record ReleaseVersionUpdateRequest
{
    [Required]
    public ReleaseType? Type { get; init; }

    [JsonConverter(typeof(TimeIdentifierJsonConverter))]
    [Required]
    public TimeIdentifier TimePeriodCoverage { get; init; }

    public string PreReleaseAccessList { get; init; } = string.Empty;

    public string Slug => CreateReleaseSlug(year: Year, timePeriodCoverage: TimePeriodCoverage, label: Label);

    [Range(1000, 9999)]
    public int Year { get; init; }

    [MaxLength(50)]
    public string? Label { get; init; }

    public Guid[]? PublishingOrganisations { get; init; }
}
