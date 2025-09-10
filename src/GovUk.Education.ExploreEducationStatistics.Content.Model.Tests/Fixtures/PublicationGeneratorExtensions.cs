using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

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
            .SetDefault(p => p.Title)
            .SetContact(new Contact
            {
                Id = Guid.NewGuid(),
                ContactName = "Contact name",
                TeamEmail = "test@example.com",
                TeamName = "Team name",
                ContactTelNo = "01234 567890"
            });

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

    public static Generator<Publication> WithSlug(
        this Generator<Publication> generator,
        string slug)
        => generator.ForInstance(s => s.SetSlug(slug));

    public static Generator<Publication> WithUpdated(
        this Generator<Publication> generator,
        DateTime? updated = null)
    {
        return generator.ForInstance(s => s.SetUpdated(updated));
    }

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

    public static Generator<Publication> WithThemes(
        this Generator<Publication> generator,
        IEnumerable<Theme> themes)
    {
        themes.ForEach((theme, index) =>
            generator.ForIndex(index, s => s.SetTheme(theme)));

        return generator;
    }

    public static Generator<Publication> WithRedirects(
        this Generator<Publication> generator,
        IEnumerable<PublicationRedirect> publicationRedirects)
        => generator.ForInstance(s => s.SetRedirects(publicationRedirects));

    public static Generator<Publication> WithRedirects(
        this Generator<Publication> generator,
        Func<SetterContext, IEnumerable<PublicationRedirect>> publicationRedirects)
        => generator.ForInstance(s => s.SetRedirects(publicationRedirects.Invoke));

    public static InstanceSetters<Publication> SetReleases(
        this InstanceSetters<Publication> setters,
        IEnumerable<Release> releases)
        => setters.SetReleases(_ => releases);

    private static InstanceSetters<Publication> SetReleases(
        this InstanceSetters<Publication> setters,
        Func<SetterContext, IEnumerable<Release>> releases)
        => setters.Set(p => p.Releases,
                (_, publication, context) =>
                {
                    var list = releases.Invoke(context).ToList();

                    list.ForEach(release =>
                    {
                        release.Publication = publication;
                        release.PublicationId = publication.Id;
                    });

                    return list;
                })
            .Set(p => p.ReleaseSeries,
                (_, publication, context) =>
                    publication.Releases
                        .OrderByDescending(r => r.Year)
                        .ThenByDescending(r => r.TimePeriodCoverage)
                        .Select(release => context.Fixture
                            .DefaultReleaseSeriesItem()
                            .WithReleaseId(release.Id)
                            .Generate())
                        .ToList())
            .Set(
                p => p.ReleaseVersions,
                (_, publication, _) =>
                {
                    var releaseVersions = publication.Releases.SelectMany(r => r.Versions)
                        .ToList();

                    releaseVersions.ForEach(releaseVersion =>
                    {
                        releaseVersion.Publication = publication;
                        releaseVersion.PublicationId = publication.Id;
                    });

                    return releaseVersions;
                }
            )
            .Set(p => p.LatestPublishedReleaseVersion,
                (_, publication, _) =>
                {
                    var releaseVersions = publication.Releases.SelectMany(r => r.Versions)
                        .ToList();

                    var publishedVersions = releaseVersions
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
                            releaseVersion => new
                            {
                                releaseVersion.ReleaseId,
                                releaseVersion.Version
                            },
                            (_, releaseVersion) => releaseVersion)
                        .OrderByDescending(releaseVersion => releaseVersion.Release.Year)
                        .ThenByDescending(releaseVersion => releaseVersion.Release.TimePeriodCoverage)
                        .FirstOrDefault();
                })
            .Set(p => p.LatestPublishedReleaseVersionId,
                (_, publication, _) => publication.LatestPublishedReleaseVersion?.Id);

    private static InstanceSetters<Publication> SetContact(
        this InstanceSetters<Publication> setters,
        Contact contact)
        => setters.Set(p => p.Contact, contact)
            .SetContactId(contact.Id);

    private static InstanceSetters<Publication> SetContactId(
        this InstanceSetters<Publication> setters,
        Guid contactId)
        => setters.Set(p => p.ContactId, contactId);

    private static InstanceSetters<Publication> SetExternalMethodology(
        this InstanceSetters<Publication> setters,
        ExternalMethodology externalMethodology)
        => setters.Set(p => p.ExternalMethodology, externalMethodology);

    private static InstanceSetters<Publication> SetLegacyLinks(
        this InstanceSetters<Publication> setters,
        IEnumerable<ReleaseSeriesItem> legacyLinks) =>
        setters.SetLegacyLinks(_ => legacyLinks);

    private static InstanceSetters<Publication> SetLegacyLinks(
        this InstanceSetters<Publication> setters,
        Func<SetterContext, IEnumerable<ReleaseSeriesItem>> legacyLinks) =>
        setters.Set(
            p => p.ReleaseSeries,
            (_, publication, context) =>
            {
                var list = legacyLinks.Invoke(context).ToList();

                if (list.Any(rsi => !rsi.IsLegacyLink))
                {
                    throw new ArgumentException("List can only contain legacy links", nameof(legacyLinks));
                }

                return [.. publication.ReleaseSeries, .. list];
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

    public static InstanceSetters<Publication> SetSlug(
        this InstanceSetters<Publication> setters,
        string slug)
        => setters.Set(p => p.Slug, slug);

    public static InstanceSetters<Publication> SetRedirects(
        this InstanceSetters<Publication> setters,
        IEnumerable<PublicationRedirect> publicationRedirects)
        => setters.SetRedirects(_ => publicationRedirects);

    private static InstanceSetters<Publication> SetRedirects(
        this InstanceSetters<Publication> setters,
        Func<SetterContext, IEnumerable<PublicationRedirect>> publicationRedirects)
        => setters.Set(
            mv => mv.PublicationRedirects,
            (_, publication, context) =>
            {
                var list = publicationRedirects.Invoke(context).ToList();

                list.ForEach(publicationRedirect =>
                {
                    publicationRedirect.Publication = publication;
                    publicationRedirect.PublicationId = publication.Id;
                });

                return list;
            });

    private static InstanceSetters<Publication> SetUpdated(
        this InstanceSetters<Publication> setters,
        DateTime? updated)
        => setters.Set(p => p.Updated, updated);

    public static ConditionalGeneratorDeclaration If(this Generator<Publication> generator, bool condition) =>
        new(condition, generator);
        

    public class ConditionalGeneratorDeclaration(bool condition, Generator<Publication> generator)
    {
        public Generator<Publication> Then(Func<Generator<Publication>, Generator<Publication>> action) =>
            condition
                ? action(generator)
                : generator;
    }
}
