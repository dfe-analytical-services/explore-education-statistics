#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class ReleaseGeneratorExtensions
{
    public static Generator<Release> DefaultRelease(this DataFixture fixture)
        => fixture.Generator<Release>().WithDefaults();

    public static Generator<Release> WithDefaults(this Generator<Release> generator)
        => generator.ForInstance(release => release.SetDefaults());

    public static Generator<Release> WithId(
        this Generator<Release> generator,
        Guid id)
        => generator.ForInstance(release => release.SetId(id));

    public static Generator<Release> WithIds(
        this Generator<Release> generator,
        IEnumerable<Guid> ids)
    {
        ids.ForEach((id, index) =>
            generator.ForIndex(index, s => s.SetId(id)));

        return generator;
    }

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

    public static Generator<Release> WithReleaseStatuses(
        this Generator<Release> generator,
        IEnumerable<ReleaseStatus> status)
        => generator.ForInstance(release => release.SetReleaseStatuses(status));

    public static Generator<Release> WithCreated(
        this Generator<Release> generator,
        DateTime? created = null,
        Guid? createdById = null)
    {
        return generator.ForInstance(r => r.SetCreated(created, createdById));
    }
    public static Generator<Release> WithNextReleaseDate(
        this Generator<Release> generator,
        PartialDate nextReleaseDate)
        => generator.ForInstance(release => release.SetNextReleaseDate(nextReleaseDate));

    public static Generator<Release> WithPreviousVersionId(
        this Generator<Release> generator,
        Guid previousVersionId)
        => generator.ForInstance(release => release.SetPreviousVersionId(previousVersionId));

    public static Generator<Release> WithYear(
        this Generator<Release> generator,
        int year)
        => generator.ForInstance(release => release.SetYear(year));

    public static Generator<Release> WithType(
        this Generator<Release> generator,
        ReleaseType type)
        => generator.ForInstance(release => release.SetType(type));

    public static Generator<Release> WithVersion(
        this Generator<Release> generator,
        int version)
        => generator.ForInstance(release => release.SetVersion(version));

    public static Generator<Release> WithNotifiedOn(
        this Generator<Release> generator,
        DateTime notifiedOn)
        => generator.ForInstance(release => release.SetNotifiedOn(notifiedOn));

    public static Generator<Release> WithTimePeriodCoverage(
        this Generator<Release> generator,
        TimeIdentifier timePeriodCoverage)
        => generator.ForInstance(release => release.SetTimePeriodCoverage(timePeriodCoverage));

    public static Generator<Release> WithNotifySubscribers(
        this Generator<Release> generator,
        bool notifySubscribers)
        => generator.ForInstance(release => release.SetNotifySubscribers(notifySubscribers));

    public static Generator<Release> WithUpdatePublishedDate(
        this Generator<Release> generator,
        bool updatePublishedDate)
        => generator.ForInstance(release => release.SetUpdatePublishedDate(updatePublishedDate));

    public static Generator<Release> WithPreReleaseAccessList(
        this Generator<Release> generator,
        string preReleaseAccessList)
        => generator.ForInstance(release => release.SetPreReleaseAccessList(preReleaseAccessList));

    public static Generator<Release> WithRelatedInformation(
        this Generator<Release> generator,
        IEnumerable<Link> relatedInformation)
        => generator.ForInstance(release => release.SetRelatedInformation(relatedInformation));

    public static Generator<Release> WithUpdates(
        this Generator<Release> generator,
        IEnumerable<Update> updates)
        => generator.ForInstance(release => release.SetUpdates(updates));

    public static Generator<Release> WithKeyStatistics(
        this Generator<Release> generator,
        IEnumerable<KeyStatistic> keyStatistics)
        => generator.ForInstance(release => release.SetKeyStatistics(keyStatistics));

    public static Generator<Release> WithReleaseSummaryContent(
        this Generator<Release> generator,
        IEnumerable<ContentBlock> content)
        => generator.ForInstance(release => release.SetReleaseSummaryContent(content));

    public static Generator<Release> WithHeadlinesContent(
        this Generator<Release> generator,
        IEnumerable<ContentBlock> content)
        => generator.ForInstance(release => release.SetHeadlinesSectionContent(content));

    public static Generator<Release> WithRelatedDashboardContent(
        this Generator<Release> generator,
        IEnumerable<ContentBlock> content)
        => generator.ForInstance(release => release.SetRelatedDashboardsContent(content));

    public static Generator<Release> WithFeaturedTables(
        this Generator<Release> generator,
        IEnumerable<FeaturedTable> featuredTables)
        => generator.ForInstance(release => release.SetFeaturedTables(featuredTables));

    public static InstanceSetters<Release> SetDefaults(this InstanceSetters<Release> setters)
        => setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.Slug)
            .SetDefault(p => p.Title)
            .SetDefault(p => p.DataGuidance)
            .Set(p => p.ReleaseName, (_, _, context) => $"{2000 + context.Index}");

    public static InstanceSetters<Release> SetId(
        this InstanceSetters<Release> setters,
        Guid id)
        => setters.Set(r => r.Id, id);

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

    public static InstanceSetters<Release> SetReleaseStatuses(
        this InstanceSetters<Release> setters,
        IEnumerable<ReleaseStatus> releaseStatuses)
        => setters.Set(release => release.ReleaseStatuses, releaseStatuses.ToList());


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

    public static InstanceSetters<Release> SetNextReleaseDate(
        this InstanceSetters<Release> setters,
        PartialDate nextReleaseDate)
        => setters.Set(release => release.NextReleaseDate, nextReleaseDate);

    public static InstanceSetters<Release> SetCreated(
        this InstanceSetters<Release> setters,
        DateTime? created = null,
        Guid? createdById = null)
    {
        if (created != null)
        {
            setters = setters.Set(d => d.Created, created);
        }

        if (createdById != null)
        {
            setters = setters.Set(d => d.CreatedById, createdById);
        }

        return setters;
    }

    public static InstanceSetters<Release> SetPreviousVersionId(
        this InstanceSetters<Release> setters,
        Guid previousVersionId)
        => setters.Set(release => release.PreviousVersionId, previousVersionId);

    public static InstanceSetters<Release> SetYear(
        this InstanceSetters<Release> setters,
        int year)
        => setters.Set(release => release.ReleaseName, year.ToString());

    public static InstanceSetters<Release> SetType(
        this InstanceSetters<Release> setters,
        ReleaseType type)
        => setters.Set(release => release.Type, type);

    public static InstanceSetters<Release> SetVersion(
        this InstanceSetters<Release> setters,
        int version)
        => setters.Set(release => release.Version, version);

    public static InstanceSetters<Release> SetTimePeriodCoverage(
        this InstanceSetters<Release> setters,
        TimeIdentifier timePeriodCoverage)
        => setters.Set(release => release.TimePeriodCoverage, timePeriodCoverage);

    public static InstanceSetters<Release> SetNotifySubscribers(
        this InstanceSetters<Release> setters,
        bool notifySubscribers)
        => setters.Set(release => release.NotifySubscribers, notifySubscribers);

    public static InstanceSetters<Release> SetNotifiedOn(
        this InstanceSetters<Release> setters,
        DateTime notifiedOn)
        => setters.Set(release => release.NotifiedOn, notifiedOn);

    public static InstanceSetters<Release> SetUpdatePublishedDate(
        this InstanceSetters<Release> setters,
        bool updatePublishedDate)
        => setters.Set(release => release.UpdatePublishedDate, updatePublishedDate);

    public static InstanceSetters<Release> SetPreReleaseAccessList(
        this InstanceSetters<Release> setters,
        string preReleaseAccessList)
        => setters.Set(release => release.PreReleaseAccessList, preReleaseAccessList);

    public static InstanceSetters<Release> SetRelatedInformation(
        this InstanceSetters<Release> setters,
        IEnumerable<Link> relatedInformation)
        => setters.Set(release => release.RelatedInformation, relatedInformation.ToList());

    public static InstanceSetters<Release> SetUpdates(
        this InstanceSetters<Release> setters,
        IEnumerable<Update> updates)
    {
        var updatesList = updates.ToList();
        return setters
            .Set(release => release.Updates, updatesList.ToList())
            .Set((_, release, _) => updatesList.ForEach(r =>
            {
                r.Release = release;
                r.ReleaseId = release.Id;
            }));
    }

    public static InstanceSetters<Release> SetKeyStatistics(
        this InstanceSetters<Release> setters,
        IEnumerable<KeyStatistic> keyStatistics)
    {
        var keyStatisticsList = keyStatistics.ToList();
        return setters
            .Set(release => release.KeyStatistics, keyStatisticsList.ToList())
            .Set(release => release.KeyStatisticsSecondarySection, new ContentSection
            {
                Type = ContentSectionType.KeyStatisticsSecondary
            })
            .Set((_, release, _) => keyStatisticsList.ForEach(r =>
            {
                r.Release = release;
                r.ReleaseId = release.Id;
            }));
    }

    public static InstanceSetters<Release> SetReleaseSummaryContent(
        this InstanceSetters<Release> setters,
        IEnumerable<ContentBlock> content)
        => setters.SetTopLevelContentSection(
            release => release.HeadlinesSection, ContentSectionType.ReleaseSummary, content);

    public static InstanceSetters<Release> SetHeadlinesSectionContent(
        this InstanceSetters<Release> setters,
        IEnumerable<ContentBlock> content)
        => setters.SetTopLevelContentSection(
            release => release.HeadlinesSection, ContentSectionType.Headlines, content);

    public static InstanceSetters<Release> SetRelatedDashboardsContent(
        this InstanceSetters<Release> setters,
        IEnumerable<ContentBlock> content)
        => setters.SetTopLevelContentSection(
            release => release.RelatedDashboardsSection, ContentSectionType.RelatedDashboards, content);

    public static InstanceSetters<Release> SetFeaturedTables(
        this InstanceSetters<Release> setters,
        IEnumerable<FeaturedTable> featuredTables)
    {
        var featuredTablesList = featuredTables.ToList();
        return setters
            .Set(release => release.FeaturedTables, featuredTablesList)
            .Set((_, release, _) => featuredTablesList.ForEach(featuredTable =>
            {
                featuredTable.Release = release;
                featuredTable.ReleaseId = release.Id;
            }));
    }

    private static InstanceSetters<Release> SetTopLevelContentSection(
        this InstanceSetters<Release> setters,
        Expression<Func<Release, ContentSection>> field,
        ContentSectionType type,
        IEnumerable<ContentBlock> content)
    {
        var contentList = content.ToList();
        return setters
            .Set(field, new ContentSection
            {
                Type = type
            })
            .Set((_, release, _) => contentList.ForEach(contentBlock =>
            {
                contentBlock.Release = release;
                contentBlock.ReleaseId = release.Id;
            }));
    }
}
