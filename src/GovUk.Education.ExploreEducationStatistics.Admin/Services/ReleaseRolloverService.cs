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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseRolloverService(
    IOptions<AppOptions> appOptions,
    ContentDbContext context,
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
                        rolloverSourceReleaseId: sourceReleaseVersion.ReleaseId
                    );

                    var (templateSections, destinationSectionsByTemplateSectionId) = await InitialiseContentSections(
                        templateReleaseVersionId: sourceReleaseVersion.Id,
                        newReleaseVersion: newReleaseVersion
                    );

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
                            // M1.7-M1.8 copy data files and footnotes, using dataBlockVersionsByTemplateId.
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

                    return await releaseVersionService.GetRelease(newReleaseVersion.Id);
                })
            );
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

    // Only ever called from WithData - a FeaturedTable without a copied DataBlockVersion to repoint at would be
    // meaningless, and StructureOnly never produces a dataBlockVersionMap.
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

    // Only ever called from WithData - see CopyFeaturedTables.
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
}
