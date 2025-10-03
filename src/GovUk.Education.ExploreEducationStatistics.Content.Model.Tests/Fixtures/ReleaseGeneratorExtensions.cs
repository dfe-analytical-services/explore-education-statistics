using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class ReleaseGeneratorExtensions
{
    private const int DefaultStartYear = 2000;
    private const TimeIdentifier DefaultTimePeriodCoverage = TimeIdentifier.AcademicYear;

    public static Generator<Release> DefaultRelease(this DataFixture fixture) =>
        fixture.Generator<Release>().WithDefaults();

    public static Generator<Release> DefaultRelease(
        this DataFixture fixture,
        int publishedVersions,
        bool draftVersion = false,
        int? year = null
    )
    {
        ReleaseVersion? previousVersion = null;
        return fixture
            .Generator<Release>()
            .WithDefaults(year: year)
            .WithVersions(
                fixture
                    .DefaultReleaseVersion()
                    .ForInstance(s => s.Set(p => p.Version, (_, _, context) => context.Index))
                    .ForRange(
                        ..publishedVersions,
                        s =>
                            s.SetApprovalStatus(ReleaseApprovalStatus.Approved)
                                .Set(
                                    p => p.Published,
                                    (f, releaseVersion) =>
                                        releaseVersion.Version == 0
                                            ? f.Date.Past()
                                            : f.Date.Between(previousVersion!.Published!.Value, DateTime.UtcNow)
                                )
                                .Set(
                                    rv => rv.PublishScheduled,
                                    (_, rv) => rv.Published!.Value.AsStartOfDayUtcForTimeZone()
                                )
                    )
                    .ForRange(1.., s => s.SetPreviousVersion(previousVersion))
                    .FinishWith(releaseVersion => previousVersion = releaseVersion)
                    .Generate(draftVersion ? publishedVersions + 1 : publishedVersions)
            );
    }

    public static Generator<Release> WithDefaults(this Generator<Release> generator, int? year = null) =>
        generator.ForInstance(s => s.SetDefaults(year: year));

    public static Generator<Release> WithId(this Generator<Release> generator, Guid id) =>
        generator.ForInstance(s => s.SetId(id));

    public static Generator<Release> WithPublication(this Generator<Release> generator, Publication publication) =>
        generator.ForInstance(s => s.SetPublication(publication));

    public static Generator<Release> WithPublicationId(this Generator<Release> generator, Guid publicationId) =>
        generator.ForInstance(s => s.SetPublicationId(publicationId));

    public static Generator<Release> WithVersions(
        this Generator<Release> generator,
        Func<SetterContext, IEnumerable<ReleaseVersion>> releaseVersions
    ) => generator.ForInstance(s => s.SetVersions(releaseVersions.Invoke));

    public static Generator<Release> WithVersions(
        this Generator<Release> generator,
        IEnumerable<ReleaseVersion> releaseVersions
    ) => generator.ForInstance(s => s.SetVersions(releaseVersions));

    public static Generator<Release> WithCreated(this Generator<Release> generator, DateTime created)
    {
        return generator.ForInstance(s => s.SetCreated(created));
    }

    public static Generator<Release> WithTimePeriodCoverage(
        this Generator<Release> generator,
        TimeIdentifier timePeriodCoverage
    ) => generator.ForInstance(s => s.SetTimePeriodCoverage(timePeriodCoverage));

    public static Generator<Release> WithUpdated(this Generator<Release> generator, DateTime? updated = null)
    {
        return generator.ForInstance(s => s.SetUpdated(updated));
    }

    public static Generator<Release> WithYear(this Generator<Release> generator, int year) =>
        generator.ForInstance(s => s.SetYear(year));

    public static Generator<Release> WithSlug(this Generator<Release> generator, string slug) =>
        generator.ForInstance(s => s.SetSlug(slug));

    public static Generator<Release> WithRedirects(
        this Generator<Release> generator,
        IEnumerable<ReleaseRedirect> releaseRedirects
    ) => generator.ForInstance(s => s.SetRedirects(releaseRedirects));

    public static Generator<Release> WithRedirects(
        this Generator<Release> generator,
        Func<SetterContext, IEnumerable<ReleaseRedirect>> releaseRedirects
    ) => generator.ForInstance(s => s.SetRedirects(releaseRedirects.Invoke));

    public static Generator<Release> WithLabel(this Generator<Release> generator, string? label) =>
        generator.ForInstance(s => s.SetLabel(label));

    public static InstanceSetters<Release> SetDefaults(this InstanceSetters<Release> setters, int? year = null) =>
        setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.PublicationId)
            .Set(p => p.Year, (_, _, context) => year ?? DefaultStartYear + context.Index)
            .SetTimePeriodCoverage(DefaultTimePeriodCoverage)
            .Set(p => p.Slug, (_, r) => NamingUtils.SlugFromTitle(r.YearTitle))
            .Set(p => p.Created, f => f.Date.Past())
            .Set(p => p.Updated, (f, r) => f.Date.Soon(refDate: r.Created));

    public static InstanceSetters<Release> SetId(this InstanceSetters<Release> setters, Guid id) =>
        setters.Set(p => p.Id, id);

    public static InstanceSetters<Release> SetPublication(
        this InstanceSetters<Release> setters,
        Publication publication
    ) =>
        setters
            .Set(
                p => p.Publication,
                (_, release) =>
                {
                    publication.Releases.Add(release);

                    var releaseVersions = release.Versions;

                    releaseVersions.ForEach(rv =>
                    {
                        rv.Publication = publication;
                        rv.PublicationId = publication.Id;
                    });

                    publication.ReleaseVersions.AddRange(releaseVersions);

                    return publication;
                }
            )
            .SetPublicationId(publication.Id)
            .Set(
                (_, release, context) =>
                {
                    publication.ReleaseSeries.Add(
                        context.Fixture.DefaultReleaseSeriesItem().WithReleaseId(release.Id).Generate()
                    );
                }
            )
            .Set(
                (_, _) =>
                {
                    var releaseVersions = publication.Releases.SelectMany(r => r.Versions).ToList();

                    var publishedVersions = releaseVersions
                        .Where(releaseVersion => releaseVersion.Published.HasValue)
                        .ToList();

                    if (publishedVersions.Count > 0)
                    {
                        publication.LatestPublishedReleaseVersion =
                            publishedVersions.Count == 1
                                ? publishedVersions[0]
                                : publishedVersions
                                    .GroupBy(releaseVersion => releaseVersion.ReleaseId)
                                    .Select(groupedReleases => new
                                    {
                                        ReleaseId = groupedReleases.Key,
                                        Version = groupedReleases.Max(releaseVersion => releaseVersion.Version),
                                    })
                                    .Join(
                                        publishedVersions,
                                        maxVersion => maxVersion,
                                        releaseVersion => new { releaseVersion.ReleaseId, releaseVersion.Version },
                                        (_, releaseVersion) => releaseVersion
                                    )
                                    .OrderByDescending(releaseVersion => releaseVersion.Release.Year)
                                    .ThenByDescending(releaseVersion => releaseVersion.Release.TimePeriodCoverage)
                                    .FirstOrDefault();
                    }

                    publication.LatestPublishedReleaseVersionId = publication.LatestPublishedReleaseVersion?.Id;
                }
            );

    public static InstanceSetters<Release> SetPublicationId(
        this InstanceSetters<Release> setters,
        Guid publicationId
    ) => setters.Set(p => p.PublicationId, publicationId);

    public static InstanceSetters<Release> SetVersions(
        this InstanceSetters<Release> setters,
        IEnumerable<ReleaseVersion> releaseVersions
    ) => setters.SetVersions(_ => releaseVersions);

    private static InstanceSetters<Release> SetVersions(
        this InstanceSetters<Release> setters,
        Func<SetterContext, IEnumerable<ReleaseVersion>> releaseVersions
    ) =>
        setters.Set(
            p => p.Versions,
            (_, release, context) =>
            {
                var list = releaseVersions.Invoke(context).ToList();

                list.ForEach(releaseVersion =>
                {
                    releaseVersion.Release = release;
                    releaseVersion.ReleaseId = release.Id;

                    releaseVersion.Publication = release.Publication;
                    releaseVersion.PublicationId = release.PublicationId;
                });

                return list;
            }
        );

    public static InstanceSetters<Release> SetCreated(this InstanceSetters<Release> setters, DateTime created) =>
        setters.Set(p => p.Created, created);

    public static InstanceSetters<Release> SetTimePeriodCoverage(
        this InstanceSetters<Release> setters,
        TimeIdentifier timePeriodCoverage
    ) => setters.Set(p => p.TimePeriodCoverage, timePeriodCoverage);

    public static InstanceSetters<Release> SetUpdated(this InstanceSetters<Release> setters, DateTime? updated) =>
        setters.Set(p => p.Updated, updated);

    public static InstanceSetters<Release> SetYear(this InstanceSetters<Release> setters, int year) =>
        setters.Set(p => p.Year, year);

    public static InstanceSetters<Release> SetSlug(this InstanceSetters<Release> setters, string slug) =>
        setters.Set(p => p.Slug, slug);

    public static InstanceSetters<Release> SetRedirects(
        this InstanceSetters<Release> setters,
        IEnumerable<ReleaseRedirect> releaseRedirects
    ) => setters.SetRedirects(_ => releaseRedirects);

    private static InstanceSetters<Release> SetRedirects(
        this InstanceSetters<Release> setters,
        Func<SetterContext, IEnumerable<ReleaseRedirect>> releaseRedirects
    ) =>
        setters.Set(
            mv => mv.ReleaseRedirects,
            (_, release, context) =>
            {
                var list = releaseRedirects.Invoke(context).ToList();

                list.ForEach(releaseRedirect =>
                {
                    releaseRedirect.Release = release;
                    releaseRedirect.ReleaseId = release.Id;
                });

                return list;
            }
        );

    public static InstanceSetters<Release> SetLabel(this InstanceSetters<Release> setters, string? label) =>
        setters.Set(p => p.Label, label);
}
