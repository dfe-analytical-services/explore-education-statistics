using System;
using System.Collections.Generic;
using System.Linq;
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
            .SetDefault(p => p.Title);

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

    public static InstanceSetters<Publication> SetLatestPublishedReleaseVersion(
        this InstanceSetters<Publication> setters,
        ReleaseVersion? releaseVersion)
        => setters.Set(p => p.LatestPublishedReleaseVersion, releaseVersion)
            .SetLatestPublishedReleaseVersionId(releaseVersion?.Id);

    public static InstanceSetters<Publication> SetLatestPublishedReleaseVersionId(
        this InstanceSetters<Publication> setters,
        Guid? latestPublishedReleaseVersionId)
        => setters.Set(p => p.LatestPublishedReleaseVersionId, latestPublishedReleaseVersionId);

    public static Generator<Publication> WithTopics(this Generator<Publication> generator,
        IEnumerable<Topic> topics)
    {
        topics.ForEach((topic, index) =>
            generator.ForIndex(index, s => s.SetTopic(topic)));

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
                    var list = releases.Invoke(context).ToList();

                    var releaseVersions = list.SelectMany(r => r.Versions)
                        .ToList();

                    releaseVersions.ForEach(releaseVersion =>
                    {
                        releaseVersion.Publication = publication;
                        releaseVersion.PublicationId = publication.Id;

                        // @MarkFix
                        //publication.ReleaseSeriesView.Add(new()
                        //{
                        //    ReleaseId = release.Id,
                        //    IsAmendment = release.Amendment,
                        //    IsDraft = !release.Published.HasValue,
                        //    IsLegacy = false,
                        //    Order = publication.ReleaseSeriesView.Count + 1,
                        //});
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

    private static InstanceSetters<Publication> SetLegacyReleases( // @MarkFix remove
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

                        // @MarkFix
                        //publication.ReleaseSeriesView.Add(new()
                        //{
                        //    ReleaseId = legacyRelease.Id,
                        //    IsAmendment = false,
                        //    IsDraft = false,
                        //    IsLegacy = true,
                        //    Order = publication.ReleaseSeriesView.Count + 1,
                        //});
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
