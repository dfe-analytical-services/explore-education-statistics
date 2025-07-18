#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class Publication
{
    public Guid Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string Title { get; set; } = string.Empty;

    [MaxLength(160)]
    public string Summary { get; set; } = string.Empty;

    public List<Release> Releases { get; set; } = [];

    [Obsolete("Use indirect relationship via Publication.Releases. This will be removed in EES-5818")] 
    public List<ReleaseVersion> ReleaseVersions { get; set; } = [];

    public List<PublicationRedirect> PublicationRedirects { get; set; } = [];

    public List<PublicationMethodology> Methodologies { get; set; } = [];

    public ExternalMethodology? ExternalMethodology { get; set; }

    public Guid ThemeId { get; set; }

    public Theme Theme { get; set; } = null!;

    public Guid ContactId { get; set; }

    public Contact Contact { get; set; } = null!;

    public Guid? SupersededById { get; set; }

    public Publication? SupersededBy { get; set; }

    public DateTime? Updated { get; set; }

    public bool Live => LatestPublishedReleaseVersionId.HasValue;

    public Guid? LatestPublishedReleaseVersionId { get; set; }

    public ReleaseVersion? LatestPublishedReleaseVersion { get; set; }

    public List<ReleaseSeriesItem> ReleaseSeries { get; set; } = [];
}
