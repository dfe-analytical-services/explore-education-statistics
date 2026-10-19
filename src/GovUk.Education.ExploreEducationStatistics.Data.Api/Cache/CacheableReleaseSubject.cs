#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;

public record CacheableReleaseSubject
{
    public ReleaseFile ReleaseFile { get; }
    private ReleaseVersion ContentReleaseVersion { get; }

    public CacheableReleaseSubject(ReleaseFile releaseFile, ReleaseVersion contentReleaseVersion)
    {
        ReleaseFile = releaseFile;
        ContentReleaseVersion = contentReleaseVersion;
    }

    public Guid SubjectId => ReleaseFile.File.SubjectId!.Value;
    public string ReleaseSlug => ContentReleaseVersion.Release.Slug;
    public string PublicationSlug => ContentReleaseVersion.Release.Publication.Slug;
}
