#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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

    public static Generator<Publication> WithLatestPublishedRelease(
        this Generator<Publication> generator,
        Release release)
        => generator.ForInstance(s => s.SetLatestPublishedRelease(release));

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
        Release release)
        => setters.Set(p => p.LatestPublishedRelease, release);

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
        => setters.Set(m => m.Contact, contact);

    private static InstanceSetters<Publication> SetTopicId(
        this InstanceSetters<Publication> setters,
        Guid topicId)
        => setters.Set(p => p.TopicId, topicId);

    private static InstanceSetters<Publication> SetTopic(
        this InstanceSetters<Publication> setters,
        Topic topic)
        => setters.Set(p => p.Topic, topic);
}
