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
        => generator.ForInstance(d => d.SetDefaults());

    public static Generator<Release> WithApprovalStatus(
        this Generator<Release> generator,
        ReleaseApprovalStatus status)
        => generator.ForInstance(d => d.SetApprovalStatus(status));

    public static Generator<Release> WithPublished(
        this Generator<Release> generator,
        DateTime published)
        => generator.ForInstance(d => d.SetPublished(published));

    public static Generator<Release> WithPublishScheduled(
        this Generator<Release> generator,
        DateTime publishScheduled)
        => generator.ForInstance(d => d.SetPublishScheduled(publishScheduled));

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
        => generator.ForInstance(d => d.SetDataBlockVersions(dataBlockVersions));

    public static Generator<Release> WithContent(
        this Generator<Release> generator,
        IEnumerable<ContentSection> content)
        => generator.ForInstance(d => d.SetContentBlocks(content));

    public static InstanceSetters<Release> SetDefaults(this InstanceSetters<Release> setters)
        => setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.Slug)
            .SetDefault(p => p.Title)
            .SetDefault(p => p.DataGuidance)
            .Set(p => p.ReleaseName, (_, _, context) => $"{2000 + context.Index}");

    public static InstanceSetters<Release> SetApprovalStatus(
        this InstanceSetters<Release> setters,
        ReleaseApprovalStatus status)
        => setters.Set(d => d.ApprovalStatus, status);

    public static InstanceSetters<Release> SetDataBlockVersions(
        this InstanceSetters<Release> setters,
        IEnumerable<DataBlockVersion> dataBlockVersions)
        => setters.Set(d => d.DataBlockVersions, dataBlockVersions.ToList());

    public static InstanceSetters<Release> SetContentBlocks(
        this InstanceSetters<Release> setters,
        IEnumerable<ContentSection> content)
        => setters.Set(d => d.Content, content.ToList());

    public static InstanceSetters<Release> SetPublished(
        this InstanceSetters<Release> setters,
        DateTime published)
        => setters.Set(d => d.Published, published);

    public static InstanceSetters<Release> SetPublishScheduled(
        this InstanceSetters<Release> setters,
        DateTime publishScheduled)
        => setters.Set(d => d.PublishScheduled, publishScheduled);
}
