#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseService(
    ContentDbContext context,
    IReleaseVersionService releaseVersionService,
    IUserService userService,
    IGuidGenerator guidGenerator) : IReleaseService
{
    public async Task<Either<ActionResult, ReleaseVersionViewModel>> CreateRelease(ReleaseCreateRequest releaseCreate)
    {
        return await ReleaseCreateRequestValidator.Validate(releaseCreate)
            .OnSuccess(async () => await context.Publications
                .SingleOrNotFoundAsync(p => p.Id == releaseCreate.PublicationId))
            .OnSuccess(userService.CheckCanCreateReleaseForPublication)
            .OnSuccessDo(async _ =>
                await ValidateReleaseSlugUniqueToPublication(releaseCreate.Slug, releaseCreate.PublicationId))
            .OnSuccess(async publication =>
            {
                var release = new Release
                {
                    PublicationId = releaseCreate.PublicationId,
                    Year = releaseCreate.Year,
                    TimePeriodCoverage = releaseCreate.TimePeriodCoverage,
                    Slug = releaseCreate.Slug,
                    Label = string.IsNullOrWhiteSpace(releaseCreate.Label) ? null : releaseCreate.Label.Trim(),
                };

                var newReleaseVersion = new ReleaseVersion
                {
                    Id = guidGenerator.NewGuid(),
                    Release = release,
                    Type = releaseCreate.Type!.Value,
                    ApprovalStatus = ReleaseApprovalStatus.Draft,
                    PublicationId = release.PublicationId,
                };

                if (releaseCreate.TemplateReleaseId.HasValue)
                {
                    await CreateGenericContentFromTemplate(releaseCreate.TemplateReleaseId.Value,
                        newReleaseVersion);
                }
                else
                {
                    newReleaseVersion.GenericContent = new List<ContentSection>();
                }

                newReleaseVersion.SummarySection = new ContentSection
                {
                    Type = ContentSectionType.ReleaseSummary,
                };
                newReleaseVersion.KeyStatisticsSecondarySection = new ContentSection
                {
                    Type = ContentSectionType.KeyStatisticsSecondary,
                };
                newReleaseVersion.HeadlinesSection = new ContentSection
                {
                    Type = ContentSectionType.Headlines,
                };
                newReleaseVersion.RelatedDashboardsSection = new ContentSection
                {
                    Type = ContentSectionType.RelatedDashboards,
                };
                newReleaseVersion.Created = DateTime.UtcNow;
                newReleaseVersion.CreatedById = userService.GetUserId();

                await context.ReleaseVersions.AddAsync(newReleaseVersion);

                publication.ReleaseSeries.Insert(0, new ReleaseSeriesItem
                {
                    Id = Guid.NewGuid(),
                    ReleaseId = release.Id
                });
                context.Publications.Update(publication);

                await context.SaveChangesAsync();
                return await releaseVersionService.GetRelease(newReleaseVersion.Id);
            });
    }

    private async Task<Either<ActionResult, Unit>> ValidateReleaseSlugUniqueToPublication(
        string slug,
        Guid publicationId,
        Guid? releaseId = null)
    {
        var slugAlreadyExists = await context.Releases
            .Where(r => r.PublicationId == publicationId)
            .AnyAsync(r => r.Slug == slug && r.Id != releaseId);

        return slugAlreadyExists 
            ? ValidationActionResult(SlugNotUnique) 
            : Unit.Instance;
    }

    private async Task CreateGenericContentFromTemplate(
        Guid templateReleaseVersionId,
        ReleaseVersion newReleaseVersion)
    {
        var templateReleaseVersion = await context
            .ReleaseVersions
            .AsNoTracking()
            .Include(releaseVersion => releaseVersion.Content)
            .FirstAsync(rv => rv.Id == templateReleaseVersionId);

        newReleaseVersion.Content = templateReleaseVersion
            .Content
            .Where(section => section.Type == ContentSectionType.Generic)
            .Select(section => CloneContentSectionFromReleaseTemplate(section, newReleaseVersion))
            .ToList();
    }

    private static ContentSection CloneContentSectionFromReleaseTemplate(
        ContentSection originalSection,
        ReleaseVersion newReleaseVersion)
    {
        // Create a new ContentSection based upon the original template.
        return new ContentSection
        {
            // Assign a new Id.
            Id = Guid.NewGuid(),

            // Assign it to the new release version.
            ReleaseVersionId = newReleaseVersion.Id,

            // Copy certain fields from the original.
            Caption = originalSection.Caption,
            Heading = originalSection.Heading,
            Order = originalSection.Order,
            Type = originalSection.Type
        };
    }
}
