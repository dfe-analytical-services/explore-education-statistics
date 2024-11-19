using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.TimePeriodLabelFormatter;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record ReleaseCreateRequest
{
    public Guid PublicationId { get; set; }

    [Required] public ReleaseType Type { get; set; }

    [Required]
    [JsonConverter(typeof(TimeIdentifierJsonConverter))]
    public TimeIdentifier TimePeriodCoverage { get; init; }

    public string Slug => SlugFromTitle(Title);

    private string Title => Format(Year, TimePeriodCoverage);

    [Range(1000, 9999)]
    public int Year { get; init; }

    public Guid? TemplateReleaseId { get; init; }
}

public record ReleaseUpdateRequest
{
    [Required] public ReleaseType Type { get; init; }

    [JsonConverter(typeof(TimeIdentifierJsonConverter))]
    [Required]
    public TimeIdentifier TimePeriodCoverage { get; init; }

    public string PreReleaseAccessList { get; init; } = string.Empty;

    public string Slug => SlugFromTitle(Title);

    private string Title => Format(Year, TimePeriodCoverage);

    [Range(1000, 9999)]
    public int Year { get; init; }
}
