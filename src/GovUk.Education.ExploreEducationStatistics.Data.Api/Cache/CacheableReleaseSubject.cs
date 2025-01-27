#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;

public record CacheableReleaseSubject
{
    public ReleaseSubject ReleaseSubject { get; }
    private ReleaseVersion ContentReleaseVersion { get; }

    public CacheableReleaseSubject(ReleaseSubject releaseSubject, ReleaseVersion contentReleaseVersion)
    {
        ReleaseSubject = releaseSubject;
        ContentReleaseVersion = contentReleaseVersion;
    }

    public Guid SubjectId => ReleaseSubject.SubjectId;
    public string ReleaseSlug => ContentReleaseVersion.Release.Slug;
    public string PublicationSlug => ContentReleaseVersion.Release.Publication.Slug;
}
