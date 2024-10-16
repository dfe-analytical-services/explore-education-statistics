using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class PublicationGeneratorExtensions
{
    public static Generator<Publication> DefaultPublication(this DataFixture fixture)
        => fixture.Generator<Publication>().WithDefaults();

    public static Generator<Publication> WithDefaults(this Generator<Publication> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<Publication> SetDefaults(this InstanceSetters<Publication> setters)
        => setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.Slug)
            .SetDefault(p => p.Summary)
            .SetDefault(p => p.Title);

    public static Generator<Publication> WithId(
        this Generator<Publication> generator,
        Guid id)
        => generator.ForInstance(s => s.SetId(id));

    public static Generator<Publication> WithLatestPublishedReleaseVersion(
        this Generator<Publication> generator,
        ReleaseVersion releaseVersion)
        => generator.ForInstance(s => s.SetLatestPublishedReleaseVersion(releaseVersion));

    public static Generator<Publication> WithReleases(
        this Generator<Publication> generator,
        IEnumerable<Release> releases)
        => generator.ForInstance(s => s.SetReleases(releases));

    public static Generator<Publication> WithReleases(
        this Generator<Publication> generator,
        Func<SetterContext, IEnumerable<Release>> releases)
        => generator.ForInstance(s => s.SetReleases(releases.Invoke));

    public static Generator<Publication> WithReleaseVersions(
        this Generator<Publication> generator,
        IEnumerable<ReleaseVersion> releaseVersions)
        => generator.ForInstance(s => s.SetReleaseVersions(releaseVersions));

    public static Generator<Publication> WithReleaseVersions(
        this Generator<Publication> generator,
        Func<SetterContext, IEnumerable<ReleaseVersion>> releaseVersions)
        => generator.ForInstance(s => s.SetReleaseVersions(releaseVersions.Invoke));

    public static Generator<Publication> WithContact(
        this Generator<Publication> generator,
        Contact contact)
        => generator.ForInstance(p => p.SetContact(contact));

    public static Generator<Publication> WithExternalMethodology(
        this Generator<Publication> generator,
        ExternalMethodology externalMethodology)
        => generator.ForInstance(p => p.SetExternalMethodology(externalMethodology));

    public static Generator<Publication> WithLegacyLinks(
        this Generator<Publication> generator,
        IEnumerable<ReleaseSeriesItem> releaseSeries)
        => generator.ForInstance(p => p.SetLegacyLinks(releaseSeries));

    public static Generator<Publication> WithSupersededBy(
        this Generator<Publication> generator,
        Publication? supersededBy)
        => generator.ForInstance(p => p.SetSupersededBy(supersededBy));

    public static Generator<Publication> WithThemeId(
        this Generator<Publication> generator,
        Guid themeId)
        => generator.ForInstance(s => s.SetThemeId(themeId));

    public static Generator<Publication> WithTheme(
        this Generator<Publication> generator,
        Theme theme)
        => generator.ForInstance(s => s.SetTheme(theme));

    public static InstanceSetters<Publication> SetId(
        this InstanceSetters<Publication> setters,
        Guid id)
        => setters.Set(p => p.Id, id);

    public static InstanceSetters<Publication> SetLatestPublishedReleaseVersion(
        this InstanceSetters<Publication> setters,
        ReleaseVersion? releaseVersion)
        => setters.Set(p => p.LatestPublishedReleaseVersion, releaseVersion)
            .SetLatestPublishedReleaseVersionId(releaseVersion?.Id);

    public static InstanceSetters<Publication> SetLatestPublishedReleaseVersionId(
        this InstanceSetters<Publication> setters,
        Guid? latestPublishedReleaseVersionId)
        => setters.Set(p => p.LatestPublishedReleaseVersionId, latestPublishedReleaseVersionId);

    public static Generator<Publication> WithThemes(this Generator<Publication> generator,
        IEnumerable<Theme> themes)
    {
        themes.ForEach((theme, index) =>
            generator.ForIndex(index, s => s.SetTheme(theme)));

        return generator;
    }

    public static InstanceSetters<Publication> SetReleases(
        this InstanceSetters<Publication> setters,
        IEnumerable<Release> releases)
        => setters.SetReleases(_ => releases);

    private static InstanceSetters<Publication> SetReleases(
        this InstanceSetters<Publication> setters,
        Func<SetterContext, IEnumerable<Release>> releases)
        => setters.Set(
                p => p.ReleaseVersions,
                (_, publication, context) =>
                {
                    var releaseList = releases.Invoke(context).ToList();

                    releaseList.ForEach(release =>
                    {
                        publication.ReleaseSeries.Insert(0, new ReleaseSeriesItem
                        {
                            Id = Guid.NewGuid(),
                            ReleaseId = release.Id,
                        });
                    });

                    var releaseVersions = releaseList.SelectMany(r => r.Versions)
                        .ToList();

                    releaseVersions.ForEach(releaseVersion =>
                    {
                        releaseVersion.Publication = publication;
                        releaseVersion.PublicationId = publication.Id;
                    });

                    return releaseVersions;
                }
            )
            .Set(p => p.LatestPublishedReleaseVersion, (_, publication, _) =>
            {
                var publishedVersions = publication.ReleaseVersions
                    .Where(releaseVersion => releaseVersion.Published.HasValue)
                    .ToList();

                if (publishedVersions.Count == 0)
                {
                    return null;
                }

                if (publishedVersions.Count == 1)
                {
                    return publishedVersions[0];
                }

                return publishedVersions
                    .GroupBy(releaseVersion => releaseVersion.ReleaseId)
                    .Select(groupedReleases =>
                        new
                        {
                            ReleaseId = groupedReleases.Key,
                            Version = groupedReleases.Max(releaseVersion => releaseVersion.Version)
                        })
                    .Join(publishedVersions,
                        maxVersion => maxVersion,
                        releaseVersion => new { releaseVersion.ReleaseId, releaseVersion.Version },
                        (_, release) => release)
                    .OrderByDescending(releaseVersion => releaseVersion.Year)
                    .ThenByDescending(releaseVersion => releaseVersion.TimePeriodCoverage)
                    .FirstOrDefault();
            })
            .Set(p => p.LatestPublishedReleaseVersionId,
                (_, publication, _) => publication.LatestPublishedReleaseVersion?.Id);

    public static InstanceSetters<Publication> SetReleaseVersions(
        this InstanceSetters<Publication> setters,
        IEnumerable<ReleaseVersion> releaseVersions)
        => setters.SetReleaseVersions(_ => releaseVersions);

    private static InstanceSetters<Publication> SetReleaseVersions(
        this InstanceSetters<Publication> setters,
        Func<SetterContext, IEnumerable<ReleaseVersion>> releaseVersions)
        => setters.Set(
            p => p.ReleaseVersions,
            (_, publication, context) =>
            {
                var list = releaseVersions.Invoke(context).ToList();

                list.ForEach(releaseVersion => releaseVersion.Publication = publication);

                return list;
            }
        );

    private static InstanceSetters<Publication> SetContact(
        this InstanceSetters<Publication> setters,
        Contact contact)
        => setters.Set(p => p.Contact, contact);

    private static InstanceSetters<Publication> SetExternalMethodology(
        this InstanceSetters<Publication> setters,
        ExternalMethodology externalMethodology)
        => setters.Set(p => p.ExternalMethodology, externalMethodology);

    private static InstanceSetters<Publication> SetLegacyLinks(
        this InstanceSetters<Publication> setters,
        IEnumerable<ReleaseSeriesItem> legacyLinks)
        => setters.Set(
                p => p.ReleaseSeries,
                (_, publication, context) =>
                {
                    legacyLinks.ForEach(legacyLink =>
                    {
                        publication.ReleaseSeries.Add(new()
                        {
                            Id = legacyLink.Id,
                            LegacyLinkDescription = legacyLink.LegacyLinkDescription,
                            LegacyLinkUrl = legacyLink.LegacyLinkUrl,
                        });
                    });

                    return publication.ReleaseSeries;
                }
            );

    public static InstanceSetters<Publication> SetSupersededBy(
        this InstanceSetters<Publication> setters,
        Publication? supersededBy)
        => setters.Set(p => p.SupersededBy, supersededBy)
            .SetSupersededById(supersededBy?.Id);

    private static InstanceSetters<Publication> SetSupersededById(
        this InstanceSetters<Publication> setters,
        Guid? supersededById)
        => setters.Set(p => p.SupersededById, supersededById);

    private static InstanceSetters<Publication> SetThemeId(
        this InstanceSetters<Publication> setters,
        Guid themeId)
        => setters.Set(p => p.ThemeId, themeId);

    private static InstanceSetters<Publication> SetTheme(
        this InstanceSetters<Publication> setters,
        Theme theme)
        => setters.Set(p => p.Theme, theme)
            .SetThemeId(theme.Id);
}
