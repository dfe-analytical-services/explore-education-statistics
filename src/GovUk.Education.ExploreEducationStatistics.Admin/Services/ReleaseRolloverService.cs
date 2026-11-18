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

                    ContentSectionUtils.InitialiseBaseContentSections(newReleaseVersion);

                    switch (request.Mode)
                    {
                        case RolloverMode.WithData:
                            var dataBlockVersionsByTemplateId = await CopyDataBlocks(
                                templateReleaseVersionId: sourceReleaseVersion.Id,
                                newReleaseVersionId: newReleaseVersion.Id
                            );
                            await CopyContent(
                                templateReleaseVersionId: sourceReleaseVersion.Id,
                                newReleaseVersion: newReleaseVersion,
                                dataBlockVersionMap: dataBlockVersionsByTemplateId
                            );
                            // M1.6-M1.8 copy featured tables/key stats, data files and footnotes,
                            // using dataBlockVersionsByTemplateId.
                            break;
                        case RolloverMode.StructureOnly:
                            await CopyContent(
                                templateReleaseVersionId: sourceReleaseVersion.Id,
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

    private async Task CopyContent(
        Guid templateReleaseVersionId,
        ReleaseVersion newReleaseVersion,
        Dictionary<Guid, DataBlockVersion>? dataBlockVersionMap
    )
    {
        var templateSections = await context
            .ContentSections.Where(section => section.ReleaseVersionId == templateReleaseVersionId)
            .Include(section => section.Content)
            .ToListAsync();

        foreach (var templateSection in templateSections)
        {
            ContentSection destinationSection;

            if (templateSection.Type == ContentSectionType.Generic)
            {
                destinationSection = new ContentSection
                {
                    Id = guidGenerator.NewGuid(),
                    ReleaseVersionId = newReleaseVersion.Id,
                    Type = ContentSectionType.Generic,
                    Heading = templateSection.Heading,
                    Order = templateSection.Order,
                };

                newReleaseVersion.Content.Add(destinationSection);
                context.ContentSections.Add(destinationSection);
            }
            else
            {
                // The 5 standard sections already exist, created empty by InitialiseBaseContentSections - copy
                // content into them rather than creating new rows.
                destinationSection = newReleaseVersion.Content.Single(section => section.Type == templateSection.Type);
            }

            foreach (var templateBlock in templateSection.Content)
            {
                if (templateBlock is HtmlBlock templateHtmlBlock)
                {
                    var newHtmlBlock = new HtmlBlock
                    {
                        Id = guidGenerator.NewGuid(),
                        ReleaseVersionId = newReleaseVersion.Id,
                        ContentSectionId = destinationSection.Id,
                        Order = templateHtmlBlock.Order,
                        Body = "",
                    };

                    destinationSection.Content.Add(newHtmlBlock);
                    context.ContentBlocks.Add(newHtmlBlock);

                    continue;
                }

                if (templateBlock is DataBlockVersionLink templateLink)
                {
                    if (dataBlockVersionMap is null)
                    {
                        // StructureOnly - no data blocks were copied, so there's nothing to link to.
                        continue;
                    }

                    var newDataBlockVersion = dataBlockVersionMap[templateLink.DataBlockVersionId];

                    var newDataBlockVersionLink = new DataBlockVersionLink
                    {
                        // Shares its Id with the DataBlockVersion it points at. This isn't an EF-enforced shared
                        // key - it's a convention: FeaturedTable/KeyStatisticDataBlock only store a
                        // DataBlockVersionId, and use it to find the ContentBlock that displays that version by
                        // looking up a DataBlockVersionLink with that same value as its own Id. A fresh, unrelated
                        // Id here would break that lookup.
                        Id = newDataBlockVersion.Id,
                        DataBlockVersionId = newDataBlockVersion.Id,
                        ReleaseVersionId = newReleaseVersion.Id,
                        ContentSectionId = destinationSection.Id,
                        Order = templateLink.Order,
                        Created = DateTime.UtcNow,
                    };

                    destinationSection.Content.Add(newDataBlockVersionLink);
                    context.ContentBlocks.Add(newDataBlockVersionLink);

                    continue;
                }

                // Dropped: EmbedBlockLink (never carried over).
            }
        }
    }
}
