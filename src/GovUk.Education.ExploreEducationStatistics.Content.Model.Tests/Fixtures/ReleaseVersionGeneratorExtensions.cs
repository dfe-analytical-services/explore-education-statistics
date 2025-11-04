using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class ReleaseVersionGeneratorExtensions
{
    public static Generator<ReleaseVersion> DefaultReleaseVersion(this DataFixture fixture) =>
        fixture.Generator<ReleaseVersion>().WithDefaults();

    public static Generator<ReleaseVersion> WithDefaults(this Generator<ReleaseVersion> generator) =>
        generator.ForInstance(releaseVersion => releaseVersion.SetDefaults());

    public static Generator<ReleaseVersion> WithId(this Generator<ReleaseVersion> generator, Guid id) =>
        generator.ForInstance(releaseVersion => releaseVersion.SetId(id));

    public static Generator<ReleaseVersion> WithIds(this Generator<ReleaseVersion> generator, IEnumerable<Guid> ids)
    {
        ids.ForEach((id, index) => generator.ForIndex(index, s => s.SetId(id)));

        return generator;
    }

    [Obsolete("Provide relationship with Publication via Release. This will be removed in EES-5818")]
    public static Generator<ReleaseVersion> WithPublication(
        this Generator<ReleaseVersion> generator,
        Publication publication
    ) => generator.ForInstance(s => s.SetPublication(publication));

    public static Generator<ReleaseVersion> WithRelease(this Generator<ReleaseVersion> generator, Release release) =>
        generator.ForInstance(s => s.SetRelease(release));

    public static Generator<ReleaseVersion> WithReleaseId(this Generator<ReleaseVersion> generator, Guid releaseId) =>
        generator.ForInstance(s => s.SetReleaseId(releaseId));

    public static Generator<ReleaseVersion> WithApprovalStatus(
        this Generator<ReleaseVersion> generator,
        ReleaseApprovalStatus status
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetApprovalStatus(status));

    public static Generator<ReleaseVersion> WithPublished(
        this Generator<ReleaseVersion> generator,
        DateTime published
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetPublished(published));

    public static Generator<ReleaseVersion> WithPublishScheduled(
        this Generator<ReleaseVersion> generator,
        DateTime publishScheduled
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetPublishScheduled(publishScheduled));

    public static Generator<ReleaseVersion> WithPublishingOrganisations(
        this Generator<ReleaseVersion> generator,
        IEnumerable<Organisation> publishingOrganisations
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetPublishingOrganisations(publishingOrganisations));

    public static Generator<ReleaseVersion> WithApprovalStatuses(
        this Generator<ReleaseVersion> generator,
        IEnumerable<ReleaseApprovalStatus> statuses
    )
    {
        statuses.ForEach((status, index) => generator.ForIndex(index, s => s.SetApprovalStatus(status)));

        return generator;
    }

    public static Generator<ReleaseVersion> WithDataBlockVersions(
        this Generator<ReleaseVersion> generator,
        IEnumerable<DataBlockVersion> dataBlockVersions
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetDataBlockVersions(dataBlockVersions));

    public static Generator<ReleaseVersion> WithDataGuidance(
        this Generator<ReleaseVersion> generator,
        string? dataGuidance
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetDataGuidance(dataGuidance));

    public static Generator<ReleaseVersion> WithContent(
        this Generator<ReleaseVersion> generator,
        IEnumerable<ContentSection> content
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetContentBlocks(content));

    public static Generator<ReleaseVersion> WithReleaseStatuses(
        this Generator<ReleaseVersion> generator,
        IEnumerable<ReleaseStatus> status
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetReleaseStatuses(status));

    public static Generator<ReleaseVersion> WithCreated(
        this Generator<ReleaseVersion> generator,
        DateTime? created = null,
        Guid? createdById = null
    )
    {
        return generator.ForInstance(releaseVersion => releaseVersion.SetCreated(created, createdById));
    }

    public static Generator<ReleaseVersion> WithNextReleaseDate(
        this Generator<ReleaseVersion> generator,
        PartialDate? nextReleaseDate
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetNextReleaseDate(nextReleaseDate));

    public static Generator<ReleaseVersion> WithPreviousVersion(
        this Generator<ReleaseVersion> generator,
        ReleaseVersion previousVersion
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetPreviousVersion(previousVersion));

    public static Generator<ReleaseVersion> WithPreviousVersionId(
        this Generator<ReleaseVersion> generator,
        Guid previousVersionId
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetPreviousVersionId(previousVersionId));

    public static Generator<ReleaseVersion> WithSoftDeleted(this Generator<ReleaseVersion> generator) =>
        generator.ForInstance(releaseVersion => releaseVersion.SetSoftDeleted());

    public static Generator<ReleaseVersion> WithType(this Generator<ReleaseVersion> generator, ReleaseType type) =>
        generator.ForInstance(releaseVersion => releaseVersion.SetType(type));

    public static Generator<ReleaseVersion> WithVersion(this Generator<ReleaseVersion> generator, int version) =>
        generator.ForInstance(releaseVersion => releaseVersion.SetVersion(version));

    public static Generator<ReleaseVersion> WithNotifiedOn(
        this Generator<ReleaseVersion> generator,
        DateTime notifiedOn
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetNotifiedOn(notifiedOn));

    public static Generator<ReleaseVersion> WithNotifySubscribers(
        this Generator<ReleaseVersion> generator,
        bool notifySubscribers
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetNotifySubscribers(notifySubscribers));

    public static Generator<ReleaseVersion> WithUpdatePublishedDate(
        this Generator<ReleaseVersion> generator,
        bool updatePublishedDate
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetUpdatePublishedDate(updatePublishedDate));

    public static Generator<ReleaseVersion> WithPreReleaseAccessList(
        this Generator<ReleaseVersion> generator,
        string? preReleaseAccessList
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetPreReleaseAccessList(preReleaseAccessList));

    public static Generator<ReleaseVersion> WithRelatedInformation(
        this Generator<ReleaseVersion> generator,
        IEnumerable<Link> relatedInformation
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetRelatedInformation(relatedInformation));

    public static Generator<ReleaseVersion> WithUpdates(
        this Generator<ReleaseVersion> generator,
        IEnumerable<Update> updates
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetUpdates(updates));

    public static Generator<ReleaseVersion> WithKeyStatistics(
        this Generator<ReleaseVersion> generator,
        IEnumerable<KeyStatistic> keyStatistics
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetKeyStatistics(keyStatistics));

    public static Generator<ReleaseVersion> WithReleaseSummaryContent(
        this Generator<ReleaseVersion> generator,
        IEnumerable<ContentBlock> content
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetReleaseSummaryContent(content));

    public static Generator<ReleaseVersion> WithHeadlinesContent(
        this Generator<ReleaseVersion> generator,
        IEnumerable<ContentBlock> content
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetHeadlinesSectionContent(content));

    public static Generator<ReleaseVersion> WithRelatedDashboardContent(
        this Generator<ReleaseVersion> generator,
        IEnumerable<ContentBlock> content
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetRelatedDashboardsContent(content));

    public static Generator<ReleaseVersion> WithFeaturedTables(
        this Generator<ReleaseVersion> generator,
        IEnumerable<FeaturedTable> featuredTables
    ) => generator.ForInstance(releaseVersion => releaseVersion.SetFeaturedTables(featuredTables));

    public static InstanceSetters<ReleaseVersion> SetDefaults(this InstanceSetters<ReleaseVersion> setters) =>
        setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.DataGuidance)
            .Set(
                p => p.Type,
                f => f.PickRandom(EnumUtil.GetEnums<ReleaseType>().Except([ReleaseType.ExperimentalStatistics]))
            )
            .SetDefault(p => p.PublicationId)
            .SetDefault(p => p.ReleaseId)
            .SetApprovalStatus(ReleaseApprovalStatus.Draft)
            .SetDefault(p => p.PreReleaseAccessList)
            .Set(
                p => p.NextReleaseDate,
                (_, _, context) =>
                    new PartialDate
                    {
                        Day = "1",
                        Month = "1",
                        Year = $"{2000 + context.Index}",
                    }
            );

    public static InstanceSetters<ReleaseVersion> SetId(this InstanceSetters<ReleaseVersion> setters, Guid id) =>
        setters.Set(releaseVersion => releaseVersion.Id, id);

    [Obsolete("Set relationship with Publication via Release. This will be removed in EES-5818")]
    public static InstanceSetters<ReleaseVersion> SetPublication(
        this InstanceSetters<ReleaseVersion> setters,
        Publication publication
    ) =>
        setters
            .Set(
                releaseVersion => releaseVersion.Publication,
                (_, releaseVersion) =>
                {
                    publication.ReleaseVersions.Add(releaseVersion);

                    return publication;
                }
            )
            .SetPublicationId(publication.Id);

    [Obsolete("Set relationship with Publication via Release. This will be removed in EES-5818")]
    public static InstanceSetters<ReleaseVersion> SetPublicationId(
        this InstanceSetters<ReleaseVersion> setters,
        Guid publicationId
    ) => setters.Set(releaseVersion => releaseVersion.PublicationId, publicationId);

    public static InstanceSetters<ReleaseVersion> SetRelease(
        this InstanceSetters<ReleaseVersion> setters,
        Release release
    ) =>
        setters
            .Set(
                releaseVersion => releaseVersion.Release,
                (_, releaseVersion) =>
                {
                    release.Versions.Add(releaseVersion);

                    releaseVersion.Publication = release.Publication;
                    releaseVersion.PublicationId = release.PublicationId;

                    return release;
                }
            )
            .SetReleaseId(release.Id);

    public static InstanceSetters<ReleaseVersion> SetReleaseId(
        this InstanceSetters<ReleaseVersion> setters,
        Guid releaseId
    ) => setters.Set(releaseVersion => releaseVersion.ReleaseId, releaseId);

    public static InstanceSetters<ReleaseVersion> SetApprovalStatus(
        this InstanceSetters<ReleaseVersion> setters,
        ReleaseApprovalStatus status
    ) => setters.Set(releaseVersion => releaseVersion.ApprovalStatus, status);

    public static InstanceSetters<ReleaseVersion> SetReleaseStatuses(
        this InstanceSetters<ReleaseVersion> setters,
        IEnumerable<ReleaseStatus> releaseStatuses
    ) => setters.Set(releaseVersion => releaseVersion.ReleaseStatuses, releaseStatuses.ToList());

    public static InstanceSetters<ReleaseVersion> SetDataBlockVersions(
        this InstanceSetters<ReleaseVersion> setters,
        IEnumerable<DataBlockVersion> dataBlockVersions
    )
    {
        var dataBlockVersionsList = dataBlockVersions.ToList();
        return setters
            .Set(releaseVersion => releaseVersion.DataBlockVersions, dataBlockVersionsList.ToList())
            .Set(
                (_, releaseVersion, _) =>
                {
                    dataBlockVersionsList.ForEach(dataBlockVersion =>
                    {
                        dataBlockVersion.ReleaseVersion = releaseVersion;
                        dataBlockVersion.ReleaseVersionId = releaseVersion.Id;
                        dataBlockVersion.ContentBlock.ReleaseVersion = releaseVersion;
                        dataBlockVersion.ContentBlock.ReleaseVersionId = releaseVersion.Id;
                    });
                }
            );
    }

    public static InstanceSetters<ReleaseVersion> SetDataGuidance(
        this InstanceSetters<ReleaseVersion> setters,
        string? dataGuidance
    ) => setters.Set(releaseVersion => releaseVersion.DataGuidance, dataGuidance);

    public static InstanceSetters<ReleaseVersion> SetContentBlocks(
        this InstanceSetters<ReleaseVersion> setters,
        IEnumerable<ContentSection> content
    ) => setters.Set(releaseVersion => releaseVersion.Content, content.ToList());

    public static InstanceSetters<ReleaseVersion> SetPublished(
        this InstanceSetters<ReleaseVersion> setters,
        DateTime published
    ) => setters.Set(releaseVersion => releaseVersion.Published, published);

    public static InstanceSetters<ReleaseVersion> SetPublishScheduled(
        this InstanceSetters<ReleaseVersion> setters,
        DateTime publishScheduled
    ) => setters.Set(releaseVersion => releaseVersion.PublishScheduled, publishScheduled);

    public static InstanceSetters<ReleaseVersion> SetPublishingOrganisations(
        this InstanceSetters<ReleaseVersion> setters,
        IEnumerable<Organisation> publishingOrganisations
    ) => setters.Set(releaseVersion => releaseVersion.PublishingOrganisations, publishingOrganisations.ToList());

    public static InstanceSetters<ReleaseVersion> SetNextReleaseDate(
        this InstanceSetters<ReleaseVersion> setters,
        PartialDate? nextReleaseDate
    ) => setters.Set(releaseVersion => releaseVersion.NextReleaseDate, nextReleaseDate);

    public static InstanceSetters<ReleaseVersion> SetCreated(
        this InstanceSetters<ReleaseVersion> setters,
        DateTime? created = null,
        Guid? createdById = null
    )
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

    public static InstanceSetters<ReleaseVersion> SetPreviousVersion(
        this InstanceSetters<ReleaseVersion> setters,
        ReleaseVersion? previousVersion
    ) =>
        setters
            .Set(releaseVersion => releaseVersion.PreviousVersion, previousVersion)
            .SetPreviousVersionId(previousVersion?.Id);

    public static InstanceSetters<ReleaseVersion> SetPreviousVersionId(
        this InstanceSetters<ReleaseVersion> setters,
        Guid? previousVersionId
    ) => setters.Set(releaseVersion => releaseVersion.PreviousVersionId, previousVersionId);

    public static InstanceSetters<ReleaseVersion> SetSoftDeleted(this InstanceSetters<ReleaseVersion> setters) =>
        setters.Set(releaseVersion => releaseVersion.SoftDeleted, true);

    public static InstanceSetters<ReleaseVersion> SetType(
        this InstanceSetters<ReleaseVersion> setters,
        ReleaseType type
    ) => setters.Set(releaseVersion => releaseVersion.Type, type);

    public static InstanceSetters<ReleaseVersion> SetVersion(
        this InstanceSetters<ReleaseVersion> setters,
        int version
    ) => setters.Set(releaseVersion => releaseVersion.Version, version);

    public static InstanceSetters<ReleaseVersion> SetNotifySubscribers(
        this InstanceSetters<ReleaseVersion> setters,
        bool notifySubscribers
    ) => setters.Set(releaseVersion => releaseVersion.NotifySubscribers, notifySubscribers);

    public static InstanceSetters<ReleaseVersion> SetNotifiedOn(
        this InstanceSetters<ReleaseVersion> setters,
        DateTime notifiedOn
    ) => setters.Set(releaseVersion => releaseVersion.NotifiedOn, notifiedOn);

    public static InstanceSetters<ReleaseVersion> SetUpdatePublishedDate(
        this InstanceSetters<ReleaseVersion> setters,
        bool updatePublishedDate
    ) => setters.Set(releaseVersion => releaseVersion.UpdatePublishedDate, updatePublishedDate);

    public static InstanceSetters<ReleaseVersion> SetPreReleaseAccessList(
        this InstanceSetters<ReleaseVersion> setters,
        string? preReleaseAccessList
    ) => setters.Set(releaseVersion => releaseVersion.PreReleaseAccessList, preReleaseAccessList);

    public static InstanceSetters<ReleaseVersion> SetRelatedInformation(
        this InstanceSetters<ReleaseVersion> setters,
        IEnumerable<Link> relatedInformation
    ) => setters.Set(releaseVersion => releaseVersion.RelatedInformation, relatedInformation.ToList());

    public static InstanceSetters<ReleaseVersion> SetUpdates(
        this InstanceSetters<ReleaseVersion> setters,
        IEnumerable<Update> updates
    )
    {
        var updatesList = updates.ToList();
        return setters
            .Set(releaseVersion => releaseVersion.Updates, updatesList.ToList())
            .Set(
                (_, releaseVersion, _) =>
                    updatesList.ForEach(update =>
                    {
                        update.ReleaseVersion = releaseVersion;
                        update.ReleaseVersionId = releaseVersion.Id;
                    })
            );
    }

    public static InstanceSetters<ReleaseVersion> SetKeyStatistics(
        this InstanceSetters<ReleaseVersion> setters,
        IEnumerable<KeyStatistic> keyStatistics
    )
    {
        var keyStatisticsList = keyStatistics.ToList();
        return setters
            .Set(releaseVersion => releaseVersion.KeyStatistics, keyStatisticsList.ToList())
            .Set(
                releaseVersion => releaseVersion.KeyStatisticsSecondarySection,
                new ContentSection { Type = ContentSectionType.KeyStatisticsSecondary }
            )
            .Set(
                (_, releaseVersion, _) =>
                    keyStatisticsList.ForEach(keyStatistic =>
                    {
                        keyStatistic.ReleaseVersion = releaseVersion;
                        keyStatistic.ReleaseVersionId = releaseVersion.Id;
                    })
            );
    }

    public static InstanceSetters<ReleaseVersion> SetReleaseSummaryContent(
        this InstanceSetters<ReleaseVersion> setters,
        IEnumerable<ContentBlock> content
    ) =>
        setters.SetTopLevelContentSection(
            releaseVersion => releaseVersion.SummarySection,
            ContentSectionType.ReleaseSummary,
            content
        );

    public static InstanceSetters<ReleaseVersion> SetHeadlinesSectionContent(
        this InstanceSetters<ReleaseVersion> setters,
        IEnumerable<ContentBlock> content
    ) =>
        setters.SetTopLevelContentSection(
            releaseVersion => releaseVersion.HeadlinesSection,
            ContentSectionType.Headlines,
            content
        );

    public static InstanceSetters<ReleaseVersion> SetRelatedDashboardsContent(
        this InstanceSetters<ReleaseVersion> setters,
        IEnumerable<ContentBlock> content
    ) =>
        setters.SetTopLevelContentSection(
            releaseVersion => releaseVersion.RelatedDashboardsSection,
            ContentSectionType.RelatedDashboards,
            content
        );

    public static InstanceSetters<ReleaseVersion> SetFeaturedTables(
        this InstanceSetters<ReleaseVersion> setters,
        IEnumerable<FeaturedTable> featuredTables
    )
    {
        var featuredTablesList = featuredTables.ToList();
        return setters
            .Set(releaseVersion => releaseVersion.FeaturedTables, featuredTablesList)
            .Set(
                (_, releaseVersion, _) =>
                    featuredTablesList.ForEach(featuredTable =>
                    {
                        featuredTable.ReleaseVersion = releaseVersion;
                        featuredTable.ReleaseVersionId = releaseVersion.Id;
                    })
            );
    }

    private static InstanceSetters<ReleaseVersion> SetTopLevelContentSection(
        this InstanceSetters<ReleaseVersion> setters,
        Expression<Func<ReleaseVersion, ContentSection>> field,
        ContentSectionType type,
        IEnumerable<ContentBlock> content
    )
    {
        var contentList = content.ToList();
        return setters
            .Set(field, new ContentSection { Type = type })
            .Set(
                (_, releaseVersion, _) =>
                    contentList.ForEach(contentBlock =>
                    {
                        contentBlock.ReleaseVersion = releaseVersion;
                        contentBlock.ReleaseVersionId = releaseVersion.Id;
                    })
            );
    }
}
