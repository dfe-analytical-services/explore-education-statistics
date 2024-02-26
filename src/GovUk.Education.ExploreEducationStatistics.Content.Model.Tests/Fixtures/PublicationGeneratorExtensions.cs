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

    public static Generator<Publication> WithLatestPublishedRelease(
        this Generator<Publication> generator,
        Release release)
        => generator.ForInstance(s => s.SetLatestPublishedRelease(release));

    public static Generator<Publication> WithReleaseParents(
        this Generator<Publication> generator,
        IEnumerable<ReleaseParent> releaseParents)
        => generator.ForInstance(s => s.SetReleaseParents(releaseParents));

    public static Generator<Publication> WithReleaseParents(
        this Generator<Publication> generator,
        Func<SetterContext, IEnumerable<ReleaseParent>> releaseParents)
        => generator.ForInstance(s => s.SetReleaseParents(releaseParents.Invoke));

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

    public static Generator<Publication> WithLegacyReleases(
        this Generator<Publication> generator,
        IEnumerable<LegacyRelease> legacyReleases)
        => generator.ForInstance(p => p.SetLegacyReleases(legacyReleases));

    public static Generator<Publication> WithSupersededBy(
        this Generator<Publication> generator,
        Publication? supersededBy)
        => generator.ForInstance(p => p.SetSupersededBy(supersededBy));

    public static Generator<Publication> WithTopicId(
        this Generator<Publication> generator,
        Guid topicId)
        => generator.ForInstance(s => s.SetTopicId(topicId));

    public static Generator<Publication> WithTopic(
        this Generator<Publication> generator,
        Topic topic)
        => generator.ForInstance(s => s.SetTopic(topic));

    public static InstanceSetters<Publication> SetLatestPublishedRelease(
        this InstanceSetters<Publication> setters,
        Release? release)
        => setters.Set(p => p.LatestPublishedRelease, release)
            .SetLatestPublishedReleaseId(release?.Id);

    public static InstanceSetters<Publication> SetLatestPublishedReleaseId(
        this InstanceSetters<Publication> setters,
        Guid? latestPublishedReleaseId)
        => setters.Set(p => p.LatestPublishedReleaseId, latestPublishedReleaseId);

    public static Generator<Publication> WithTopics(this Generator<Publication> generator,
        IEnumerable<Topic> topics)
    {
        topics.ForEach((topic, index) =>
            generator.ForIndex(index, s => s.SetTopic(topic)));

        return generator;
    }

    public static InstanceSetters<Publication> SetReleaseParents(
        this InstanceSetters<Publication> setters,
        IEnumerable<ReleaseParent> releaseParents)
        => setters.SetReleaseParents(_ => releaseParents);

    private static InstanceSetters<Publication> SetReleaseParents(
        this InstanceSetters<Publication> setters,
        Func<SetterContext, IEnumerable<ReleaseParent>> releaseParents)
        => setters.Set(
                p => p.Releases,
                (_, publication, context) =>
                {
                    var list = releaseParents.Invoke(context).ToList();

                    var releases = list.SelectMany(rp => rp.Releases)
                        .ToList();

                    releases.ForEach(release =>
                    {
                        release.Publication = publication;
                        release.PublicationId = publication.Id;

                        publication.ReleaseSeriesView.Add(new()
                        {
                            ReleaseId = release.Id,
                            IsAmendment = release.Amendment,
                            IsDraft = !release.Published.HasValue,
                            IsLegacy = false,
                            Order = publication.ReleaseSeriesView.Count + 1,
                        });
                    });

                    return releases;
                }
            )
            .Set(p => p.LatestPublishedRelease, (_, publication, _) =>
            {
                var publishedVersions = publication.Releases
                    .Where(release => release.Published.HasValue)
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
                    .GroupBy(release => release.ReleaseParentId)
                    .Select(groupedReleases =>
                        new
                        {
                            ReleaseParentId = groupedReleases.Key,
                            Version = groupedReleases.Max(release => release.Version)
                        })
                    .Join(publishedVersions,
                        maxVersion => maxVersion,
                        release => new { release.ReleaseParentId, release.Version },
                        (_, release) => release)
                    .OrderByDescending(release => release.Year)
                    .ThenByDescending(release => release.TimePeriodCoverage)
                    .FirstOrDefault();
            })
            .Set(p => p.LatestPublishedReleaseId, (_, publication, _) => publication.LatestPublishedRelease?.Id);

    public static InstanceSetters<Publication> SetReleases(
        this InstanceSetters<Publication> setters,
        IEnumerable<Release> releases)
        => setters.SetReleases(_ => releases);

    private static InstanceSetters<Publication> SetReleases(
        this InstanceSetters<Publication> setters,
        Func<SetterContext, IEnumerable<Release>> releases)
        => setters.Set(
            p => p.Releases,
            (_, publication, context) =>
            {
                var list = releases.Invoke(context).ToList();

                list.ForEach(release => release.Publication = publication);

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

    private static InstanceSetters<Publication> SetLegacyReleases(
        this InstanceSetters<Publication> setters,
        IEnumerable<LegacyRelease> legacyReleases)
        => setters.Set(
                p => p.LegacyReleases,
                (_, publication, context) =>
                {
                    legacyReleases.ForEach(legacyRelease =>
                    {
                        legacyRelease.Publication = publication;
                        legacyRelease.PublicationId = publication.Id;

                        publication.ReleaseSeriesView.Add(new()
                        {
                            ReleaseId = legacyRelease.Id,
                            IsAmendment = false,
                            IsDraft = false,
                            IsLegacy = true,
                            Order = publication.ReleaseSeriesView.Count + 1,
                        });
                    });

                    return legacyReleases;
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

    private static InstanceSetters<Publication> SetTopicId(
        this InstanceSetters<Publication> setters,
        Guid topicId)
        => setters.Set(p => p.TopicId, topicId);

    private static InstanceSetters<Publication> SetTopic(
        this InstanceSetters<Publication> setters,
        Topic topic)
        => setters.Set(p => p.Topic, topic)
            .SetTopicId(topic.Id);
}
