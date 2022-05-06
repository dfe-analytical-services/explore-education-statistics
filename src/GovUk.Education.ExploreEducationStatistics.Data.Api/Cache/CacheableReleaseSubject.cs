#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;

public record CacheableReleaseSubject
{
    public ReleaseSubject ReleaseSubject { get; }
    private Release ContentRelease { get; }

    public CacheableReleaseSubject(ReleaseSubject releaseSubject, Release contentRelease)
    {
        ReleaseSubject = releaseSubject;
        ContentRelease = contentRelease;
    }

    public Guid SubjectId => ReleaseSubject.SubjectId;
    public string ReleaseSlug => ContentRelease.Slug;
    public string PublicationSlug => ContentRelease.Publication.Slug;
}
