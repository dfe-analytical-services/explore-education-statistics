#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseRolloverService(
    IOptions<AppOptions> appOptions,
    ContentDbContext context,
    StatisticsDbContext statisticsDbContext,
    IFootnoteRepository footnoteRepository,
    IUserService userService,
    IReleaseVersionService releaseVersionService,
    IGuidGenerator guidGenerator,
    IOrganisationsValidator organisationsValidator,
    IReleaseSlugValidator releaseSlugValidator
) : IReleaseRolloverService
{
    private readonly bool _releaseRolloverEnabled = appOptions.Value.EnableReleaseRollover;

    public async Task<Either<ActionResult, ReleaseVersionViewModel>> CreateReleaseRollover(
        ReleaseRolloverCreateRequest request
    )
    {
        if (!_releaseRolloverEnabled)
        {
            return new ForbidResult();
        }

        return await context
            .Publications.SingleOrNotFoundAsync(p => p.Id == request.PublicationId)
            .OnSuccess(userService.CheckCanCreateReleaseForPublication)
            .OnSuccessCombineWith(_ => ValidateSourceRelease(request))
            .OnSuccessDo(_ =>
                releaseSlugValidator.ValidateNewSlug(newReleaseSlug: request.Slug, publicationId: request.PublicationId)
            )
            .OnSuccessCombineWith(_ =>
                organisationsValidator.ValidateOrganisations(
                    organisationIds: request.PublishingOrganisations,
                    path: nameof(ReleaseRolloverCreateRequest.PublishingOrganisations).ToLowerFirst()
                )
            )
            .OnSuccess(publicationSourceReleaseVersionAndOrganisations =>
                context.RequireTransaction(async () =>
                {
                    var (publication, sourceReleaseVersion, publishingOrganisations) =
                        publicationSourceReleaseVersionAndOrganisations;

                    var newReleaseVersion = ReleaseCreationUtils.BuildReleaseVersion(
                        context: context,
                        guidGenerator: guidGenerator,
                        userService: userService,
                        publication: publication,
                        releaseCreateRequest: request,
                        publishingOrganisations: publishingOrganisations,
                        // Only a copy WITH DATA is a rollover. A copy WITHOUT DATA is an ordinary release.
                        rolloverSourceReleaseId: request.Mode == RolloverMode.WithData
                            ? sourceReleaseVersion.ReleaseId
                            : null
                    );

                    var (templateSections, destinationSectionsByTemplateSectionId) = await InitialiseContentSections(
                        templateReleaseVersionId: sourceReleaseVersion.Id,
                        newReleaseVersion: newReleaseVersion
                    );

                    List<Guid> rolledOverSubjectIds = [];

                    switch (request.Mode)
                    {
                        case RolloverMode.WithData:
                            var dataBlockVersionsByTemplateId = await CopyDataBlocks(
                                templateReleaseVersionId: sourceReleaseVersion.Id,
                                newReleaseVersionId: newReleaseVersion.Id
                            );
                            CopyContent(
                                templateSections: templateSections,
                                destinationSectionsByTemplateSectionId: destinationSectionsByTemplateSectionId,
                                newReleaseVersion: newReleaseVersion,
                                dataBlockVersionMap: dataBlockVersionsByTemplateId
                            );
                            await CopyFeaturedTables(
                                templateReleaseVersionId: sourceReleaseVersion.Id,
                                newReleaseVersionId: newReleaseVersion.Id,
                                dataBlockVersionMap: dataBlockVersionsByTemplateId
                            );
                            await CopyKeyStatisticDataBlocks(
                                templateReleaseVersionId: sourceReleaseVersion.Id,
                                newReleaseVersionId: newReleaseVersion.Id,
                                dataBlockVersionMap: dataBlockVersionsByTemplateId
                            );
                            rolledOverSubjectIds = await CopyDataFiles(
                                templateReleaseVersionId: sourceReleaseVersion.Id,
                                newReleaseVersionId: newReleaseVersion.Id
                            );
                            break;
                        case RolloverMode.StructureOnly:
                            CopyContent(
                                templateSections: templateSections,
                                destinationSectionsByTemplateSectionId: destinationSectionsByTemplateSectionId,
                                newReleaseVersion: newReleaseVersion,
                                dataBlockVersionMap: null
                            );
                            break;
                    }

                    await context.SaveChangesAsync();

                    return (
                        TemplateReleaseVersionId: sourceReleaseVersion.Id,
                        NewReleaseVersionId: newReleaseVersion.Id,
                        RolledOverSubjectIds: rolledOverSubjectIds
                    );
                })
            )
            // The Statistics DB is a separate database, so it can't join the Content DB transaction above without
            // a distributed transaction, which isn't enabled. As in ReleaseAmendmentService, it's copied once the
            // Content DB side has committed instead.
            .OnSuccessDo(async rollover =>
            {
                if (request.Mode != RolloverMode.WithData)
                {
                    return;
                }

                await CopyStatisticsReleaseVersionAndSubjects(
                    templateReleaseVersionId: rollover.TemplateReleaseVersionId,
                    newReleaseVersionId: rollover.NewReleaseVersionId,
                    rolledOverSubjectIds: rollover.RolledOverSubjectIds
                );
                await CopyFootnotes(
                    templateReleaseVersionId: rollover.TemplateReleaseVersionId,
                    newReleaseVersionId: rollover.NewReleaseVersionId
                );
            })
            .OnSuccess(rollover => releaseVersionService.GetRelease(rollover.NewReleaseVersionId));
    }

    private async Task<Either<ActionResult, ReleaseVersion>> ValidateSourceRelease(ReleaseRolloverCreateRequest request)
    {
        return await context
            .Releases.SingleOrNotFoundAsync(r => r.Id == request.RolloverSourceReleaseId)
            .OnSuccess(sourceRelease => CheckSourceReleaseIsEligible(sourceRelease, request.PublicationId));
    }

    private async Task<Either<ActionResult, ReleaseVersion>> CheckSourceReleaseIsEligible(
        Release sourceRelease,
        Guid publicationId
    )
    {
        if (sourceRelease.PublicationId != publicationId)
        {
            return ValidationActionResult(RolloverSourceReleaseMustBeInSamePublication);
        }

        var latestPublishedReleaseVersion = await context
            .ReleaseVersions.LatestReleaseVersion(releaseId: sourceRelease.Id, publishedOnly: true)
            .SingleOrDefaultAsync();

        if (latestPublishedReleaseVersion is null)
        {
            return ValidationActionResult(RolloverSourceReleaseMustBePublished);
        }

        return latestPublishedReleaseVersion;
    }

    // Creates every ContentSection the new release needs - the 5 standard ones (always) plus one new row per
    // Generic section in the template (if any) - all in one place, before any content gets copied in.
    private async Task<(
        List<ContentSection> TemplateSections,
        Dictionary<Guid, ContentSection> DestinationSectionsByTemplateSectionId
    )> InitialiseContentSections(Guid templateReleaseVersionId, ReleaseVersion newReleaseVersion)
    {
        ContentSectionUtils.InitialiseBaseContentSections(newReleaseVersion);

        var templateSections = await context
            .ContentSections.Where(section => section.ReleaseVersionId == templateReleaseVersionId)
            .Include(section => section.Content)
            .ToListAsync();

        var destinationSectionsByTemplateSectionId = new Dictionary<Guid, ContentSection>();

        foreach (var templateSection in templateSections)
        {
            destinationSectionsByTemplateSectionId[templateSection.Id] = templateSection.Type switch
            {
                ContentSectionType.Generic => CreateGenericDestinationSection(templateSection, newReleaseVersion),
                ContentSectionType.Headlines
                or ContentSectionType.KeyStatisticsSecondary
                or ContentSectionType.RelatedDashboards
                or ContentSectionType.ReleaseSummary
                or ContentSectionType.Warning =>
                // The 5 standard sections already exist, created empty by InitialiseBaseContentSections -
                // copy content into them rather than creating new rows.
                newReleaseVersion.Content.Single(section => section.Type == templateSection.Type),
                _ => throw new ArgumentException(
                    $"Unhandled {nameof(ContentSectionType)} value {templateSection.Type} when copying content for rollover."
                ),
            };
        }

        return (templateSections, destinationSectionsByTemplateSectionId);
    }

    private ContentSection CreateGenericDestinationSection(
        ContentSection templateSection,
        ReleaseVersion newReleaseVersion
    )
    {
        var destinationSection = new ContentSection
        {
            Id = guidGenerator.NewGuid(),
            ReleaseVersionId = newReleaseVersion.Id,
            Type = ContentSectionType.Generic,
            Heading = templateSection.Heading,
            Order = templateSection.Order,
        };

        newReleaseVersion.Content.Add(destinationSection);
        context.ContentSections.Add(destinationSection);

        return destinationSection;
    }

    private async Task<Dictionary<Guid, DataBlockVersion>> CopyDataBlocks(
        Guid templateReleaseVersionId,
        Guid newReleaseVersionId
    )
    {
        var templateDataBlockVersions = await context
            .DataBlockVersions.Where(dataBlockVersion => dataBlockVersion.ReleaseVersionId == templateReleaseVersionId)
            .ToListAsync();

        var newDataBlockVersionsByTemplateId = templateDataBlockVersions.ToDictionary(
            templateDataBlockVersion => templateDataBlockVersion.Id,
            templateDataBlockVersion => new DataBlockVersion
            {
                Id = guidGenerator.NewGuid(),
                DataBlock = new DataBlock { Id = guidGenerator.NewGuid() },
                ReleaseVersionId = newReleaseVersionId,
                Version = 0,
                Heading = templateDataBlockVersion.Heading,
                Name = templateDataBlockVersion.Name,
                Source = templateDataBlockVersion.Source,
                Query = templateDataBlockVersion.Query,
                Charts = templateDataBlockVersion.Charts,
                Table = templateDataBlockVersion.Table,
                Created = DateTime.UtcNow,
                Updated = null,
                Published = null,
            }
        );

        context.DataBlockVersions.AddRange(newDataBlockVersionsByTemplateId.Values);
        await context.SaveChangesAsync();

        foreach (var newDataBlockVersion in newDataBlockVersionsByTemplateId.Values)
        {
            newDataBlockVersion.DataBlock.LatestDraftVersionId = newDataBlockVersion.Id;
        }
        await context.SaveChangesAsync();

        return newDataBlockVersionsByTemplateId;
    }

    // Purely copies blocks into already-resolved sections - every section it needs already exists by the time
    // this runs, created by InitialiseContentSections.
    private void CopyContent(
        List<ContentSection> templateSections,
        Dictionary<Guid, ContentSection> destinationSectionsByTemplateSectionId,
        ReleaseVersion newReleaseVersion,
        Dictionary<Guid, DataBlockVersion>? dataBlockVersionMap
    )
    {
        foreach (var templateSection in templateSections)
        {
            var destinationSection = destinationSectionsByTemplateSectionId[templateSection.Id];

            foreach (var templateBlock in templateSection.Content)
            {
                switch (templateBlock)
                {
                    case HtmlBlock templateHtmlBlock:
                    {
                        var newHtmlBlock = new HtmlBlock
                        {
                            Id = guidGenerator.NewGuid(),
                            ReleaseVersionId = newReleaseVersion.Id,
                            Order = templateHtmlBlock.Order,
                            Body = "",
                        };

                        // ContentSectionId isn't set explicitly - EF's relationship fixup sets it from
                        // destinationSection's own Id (whatever that ends up being) once it's added below.
                        destinationSection.Content.Add(newHtmlBlock);
                        context.ContentBlocks.Add(newHtmlBlock);
                        break;
                    }

                    case DataBlockVersionLink templateLink:
                    {
                        if (dataBlockVersionMap is null)
                        {
                            // StructureOnly - no data blocks were copied, so there's nothing to link to.
                            break;
                        }

                        var newDataBlockVersion = dataBlockVersionMap[templateLink.DataBlockVersionId];

                        var newDataBlockVersionLink = new DataBlockVersionLink
                        {
                            // Shares its Id with the DataBlockVersion it points at. This isn't an EF-enforced
                            // shared key - it's a convention: FeaturedTable/KeyStatisticDataBlock only store a
                            // DataBlockVersionId, and use it to find the ContentBlock that displays that version
                            // by looking up a DataBlockVersionLink with that same value as its own Id. A fresh,
                            // unrelated Id here would break that lookup.
                            Id = newDataBlockVersion.Id,
                            DataBlockVersionId = newDataBlockVersion.Id,
                            ReleaseVersionId = newReleaseVersion.Id,
                            Order = templateLink.Order,
                            Created = DateTime.UtcNow,
                        };

                        // ContentSectionId isn't set explicitly - see the HtmlBlock case above.
                        destinationSection.Content.Add(newDataBlockVersionLink);
                        context.ContentBlocks.Add(newDataBlockVersionLink);
                        break;
                    }

                    case EmbedBlockLink:
                        // Dropped: never carried over.
                        break;

                    default:
                        throw new ArgumentException(
                            $"Unhandled ContentBlock type {templateBlock.GetType()} when copying content for rollover."
                        );
                }
            }
        }
    }

    private async Task CopyFeaturedTables(
        Guid templateReleaseVersionId,
        Guid newReleaseVersionId,
        Dictionary<Guid, DataBlockVersion> dataBlockVersionMap
    )
    {
        var templateFeaturedTables = await context
            .FeaturedTables.Where(featuredTable => featuredTable.ReleaseVersionId == templateReleaseVersionId)
            .ToListAsync();

        var newFeaturedTables = templateFeaturedTables.Select(templateFeaturedTable =>
        {
            var newDataBlockVersion = dataBlockVersionMap[templateFeaturedTable.DataBlockVersionId];

            return new FeaturedTable
            {
                Id = guidGenerator.NewGuid(),
                ReleaseVersionId = newReleaseVersionId,
                // Repoint both ids at the new copies - unlike ReleaseAmendmentService's equivalent, which
                // deliberately keeps DataBlockId shared (same identity, new version) because that's correct for
                // an amendment. Rollover creates a brand-new DataBlock identity per copy, so both must repoint.
                DataBlockVersionId = newDataBlockVersion.Id,
                DataBlockId = newDataBlockVersion.DataBlockId,
                Name = templateFeaturedTable.Name,
                Description = templateFeaturedTable.Description,
                Order = templateFeaturedTable.Order,
                Created = DateTime.UtcNow,
                CreatedById = userService.GetUserId(),
            };
        });

        context.FeaturedTables.AddRange(newFeaturedTables);
    }

    private async Task CopyKeyStatisticDataBlocks(
        Guid templateReleaseVersionId,
        Guid newReleaseVersionId,
        Dictionary<Guid, DataBlockVersion> dataBlockVersionMap
    )
    {
        var templateKeyStatisticDataBlocks = await context
            .KeyStatisticsDataBlock.Where(keyStatistic => keyStatistic.ReleaseVersionId == templateReleaseVersionId)
            .ToListAsync();

        var newKeyStatisticDataBlocks = templateKeyStatisticDataBlocks.Select(templateKeyStatisticDataBlock =>
        {
            var newDataBlockVersion = dataBlockVersionMap[templateKeyStatisticDataBlock.DataBlockVersionId];

            return new KeyStatisticDataBlock
            {
                Id = guidGenerator.NewGuid(),
                ReleaseVersionId = newReleaseVersionId,
                // Repoint both ids - see CopyFeaturedTables.
                DataBlockVersionId = newDataBlockVersion.Id,
                DataBlockId = newDataBlockVersion.DataBlockId,
                Trend = templateKeyStatisticDataBlock.Trend,
                GuidanceTitle = templateKeyStatisticDataBlock.GuidanceTitle,
                GuidanceText = templateKeyStatisticDataBlock.GuidanceText,
                Order = templateKeyStatisticDataBlock.Order,
                Created = DateTime.UtcNow,
                CreatedById = userService.GetUserId(),
            };
        });

        context.KeyStatisticsDataBlock.AddRange(newKeyStatisticDataBlocks);
    }

    private async Task<List<Guid>> CopyDataFiles(Guid templateReleaseVersionId, Guid newReleaseVersionId)
    {
        var templateDataReleaseFiles = await context
            .ReleaseFiles.Include(releaseFile => releaseFile.File)
            .Where(releaseFile =>
                releaseFile.ReleaseVersionId == templateReleaseVersionId
                && releaseFile.File.Type == FileType.Data
                // Excludes a file that's itself still mid-replacement - shouldn't occur for a published
                // source release, but mirrors the established "not a replacement" filter used elsewhere
                // (e.g. DataSetCandidateService) rather than assuming it can never happen.
                && releaseFile.File.ReplacingId == null
            )
            .ToListAsync();

        var templateMetadataReleaseFilesBySubjectId = await context
            .ReleaseFiles.Include(releaseFile => releaseFile.File)
            .Where(releaseFile =>
                releaseFile.ReleaseVersionId == templateReleaseVersionId
                && releaseFile.File.Type == FileType.Metadata
                && releaseFile.File.ReplacingId == null
            )
            .ToDictionaryAsync(releaseFile => releaseFile.File.SubjectId!.Value);

        foreach (var templateDataReleaseFile in templateDataReleaseFiles)
        {
            var templateMetadataReleaseFile = templateMetadataReleaseFilesBySubjectId[
                templateDataReleaseFile.File.SubjectId!.Value
            ];

            context.ReleaseFiles.Add(BuildNewReleaseFile(templateDataReleaseFile, newReleaseVersionId));
            context.ReleaseFiles.Add(BuildNewReleaseFile(templateMetadataReleaseFile, newReleaseVersionId));
        }

        return templateDataReleaseFiles
            .Select(templateDataReleaseFile => templateDataReleaseFile.File.SubjectId!.Value)
            .ToList();
    }

    private ReleaseFile BuildNewReleaseFile(ReleaseFile templateReleaseFile, Guid newReleaseVersionId) =>
        new()
        {
            Id = guidGenerator.NewGuid(),
            ReleaseVersionId = newReleaseVersionId,
            // Points at the exact same underlying File as the template - nothing is re-uploaded.
            FileId = templateReleaseFile.FileId,
            Name = templateReleaseFile.Name,
            Summary = templateReleaseFile.Summary,
            Order = templateReleaseFile.Order,
            FilterSequence = templateReleaseFile.FilterSequence,
            IndicatorSequence = templateReleaseFile.IndicatorSequence,
            PairedIndicators = templateReleaseFile.PairedIndicators,
            Published = templateReleaseFile.Published,
            PublicApiDataSetId = templateReleaseFile.PublicApiDataSetId,
            PublicApiDataSetVersion = templateReleaseFile.PublicApiDataSetVersion,
            PublicApiCompatible = templateReleaseFile.PublicApiCompatible,
        };

    private async Task CopyStatisticsReleaseVersionAndSubjects(
        Guid templateReleaseVersionId,
        Guid newReleaseVersionId,
        List<Guid> rolledOverSubjectIds
    )
    {
        var templateStatsReleaseVersion = await statisticsDbContext.ReleaseVersion.SingleOrDefaultAsync(
            releaseVersion => releaseVersion.Id == templateReleaseVersionId
        );

        // A release doesn't have to have any stats uploaded.
        if (templateStatsReleaseVersion is null)
        {
            return;
        }

        statisticsDbContext.ReleaseVersion.Add(
            new Data.Model.ReleaseVersion
            {
                Id = newReleaseVersionId,
                PublicationId = templateStatsReleaseVersion.PublicationId,
            }
        );

        statisticsDbContext.ReleaseSubject.AddRange(
            rolledOverSubjectIds.Select(subjectId => new Data.Model.ReleaseSubject
            {
                ReleaseVersionId = newReleaseVersionId,
                SubjectId = subjectId,
            })
        );

        await statisticsDbContext.SaveChangesAsync();
    }

    private async Task<Dictionary<Guid, Guid>> CopyFootnotes(Guid templateReleaseVersionId, Guid newReleaseVersionId)
    {
        var templateFootnotes = await footnoteRepository.GetFootnotes(templateReleaseVersionId);

        var newFootnoteIdToTemplateFootnoteId = new Dictionary<Guid, Guid>();

        foreach (var templateFootnote in templateFootnotes)
        {
            var newFootnote = await footnoteRepository.CreateFootnote(
                releaseVersionId: newReleaseVersionId,
                content: templateFootnote.Content,
                filterIds: templateFootnote.Filters.Select(filterFootnote => filterFootnote.FilterId).ToHashSet(),
                filterGroupIds: templateFootnote
                    .FilterGroups.Select(filterGroupFootnote => filterGroupFootnote.FilterGroupId)
                    .ToHashSet(),
                filterItemIds: templateFootnote
                    .FilterItems.Select(filterItemFootnote => filterItemFootnote.FilterItemId)
                    .ToHashSet(),
                indicatorIds: templateFootnote
                    .Indicators.Select(indicatorFootnote => indicatorFootnote.IndicatorId)
                    .ToHashSet(),
                subjectIds: templateFootnote.Subjects.Select(subjectFootnote => subjectFootnote.SubjectId).ToHashSet(),
                order: templateFootnote.Order
            );

            newFootnoteIdToTemplateFootnoteId[newFootnote.Id] = templateFootnote.Id;
        }

        return newFootnoteIdToTemplateFootnoteId;
    }
}
