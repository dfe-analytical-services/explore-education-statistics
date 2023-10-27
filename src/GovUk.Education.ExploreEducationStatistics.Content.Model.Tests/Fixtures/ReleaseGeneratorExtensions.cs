#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class ReleaseGeneratorExtensions
{
    public static Generator<Release> DefaultRelease(this DataFixture fixture)
        => fixture.Generator<Release>().WithDefaults();

    public static Generator<Release> WithDefaults(this Generator<Release> generator)
        => generator.ForInstance(release => release.SetDefaults());

    public static Generator<Release> WithPublication(
        this Generator<Release> generator,
        Publication publication)
        => generator.ForInstance(release => release.SetPublication(publication));

    public static Generator<Release> WithApprovalStatus(
        this Generator<Release> generator,
        ReleaseApprovalStatus status)
        => generator.ForInstance(release => release.SetApprovalStatus(status));

    public static Generator<Release> WithPublished(
        this Generator<Release> generator,
        DateTime published)
        => generator.ForInstance(release => release.SetPublished(published));

    public static Generator<Release> WithPublishScheduled(
        this Generator<Release> generator,
        DateTime publishScheduled)
        => generator.ForInstance(release => release.SetPublishScheduled(publishScheduled));

    public static Generator<Release> WithApprovalStatuses(
        this Generator<Release> generator,
        IEnumerable<ReleaseApprovalStatus> statuses)
    {
        statuses.ForEach((status, index) =>
            generator.ForIndex(index, s => s.SetApprovalStatus(status)));

        return generator;
    }

    public static Generator<Release> WithDataBlockVersions(
        this Generator<Release> generator,
        IEnumerable<DataBlockVersion> dataBlockVersions)
        => generator.ForInstance(release => release.SetDataBlockVersions(dataBlockVersions));

    public static Generator<Release> WithContent(
        this Generator<Release> generator,
        IEnumerable<ContentSection> content)
        => generator.ForInstance(release => release.SetContentBlocks(content));

    public static InstanceSetters<Release> SetDefaults(this InstanceSetters<Release> setters)
        => setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.Slug)
            .SetDefault(p => p.Title)
            .SetDefault(p => p.DataGuidance)
            .Set(p => p.ReleaseName, (_, _, context) => $"{2000 + context.Index}");

    public static InstanceSetters<Release> SetPublication(
        this InstanceSetters<Release> setters,
        Publication publication)
        => setters.Set((_, release, _) =>
        {
            release.Publication = publication;
            release.PublicationId = publication.Id;
        });

    public static InstanceSetters<Release> SetApprovalStatus(
        this InstanceSetters<Release> setters,
        ReleaseApprovalStatus status)
        => setters.Set(release => release.ApprovalStatus, status);

    public static InstanceSetters<Release> SetDataBlockVersions(
        this InstanceSetters<Release> setters,
        IEnumerable<DataBlockVersion> dataBlockVersions)
    {
        var dataBlockVersionsList = dataBlockVersions.ToList();
        return setters
            .Set(release => release.DataBlockVersions, dataBlockVersionsList.ToList())
            .Set((_, release, _) =>
            {
                dataBlockVersionsList.ForEach(dataBlockVersion =>
                {
                    dataBlockVersion.Release = release;
                    dataBlockVersion.ReleaseId = release.Id;
                    dataBlockVersion.ContentBlock.Release = release;
                    dataBlockVersion.ContentBlock.ReleaseId = release.Id;
                });
            });
    }

    public static InstanceSetters<Release> SetContentBlocks(
        this InstanceSetters<Release> setters,
        IEnumerable<ContentSection> content)
        => setters.Set(release => release.Content, content.ToList());

    public static InstanceSetters<Release> SetPublished(
        this InstanceSetters<Release> setters,
        DateTime published)
        => setters.Set(release => release.Published, published);

    public static InstanceSetters<Release> SetPublishScheduled(
        this InstanceSetters<Release> setters,
        DateTime publishScheduled)
        => setters.Set(release => release.PublishScheduled, publishScheduled);
}
