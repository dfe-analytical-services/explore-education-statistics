#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record ReleaseCreateRequest
{
    public Guid PublicationId { get; set; }

    [Required]
    public ReleaseType? Type { get; init; }

    [Required]
    [JsonConverter(typeof(TimeIdentifierJsonConverter))]
    public TimeIdentifier TimePeriodCoverage { get; init; }

    public string Slug => CreateReleaseSlug(year: Year, timePeriodCoverage: TimePeriodCoverage, label: Label);

    [Range(1000, 9999)]
    public int Year { get; init; }

    [MaxLength(50)]
    public string? Label { get; init; }

    public Guid? TemplateReleaseId { get; init; }
}
