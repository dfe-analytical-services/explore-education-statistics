#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
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
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

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
            .OnSuccess(async publicationSourceReleaseAndOrganisations =>
            {
                var (publication, sourceRelease, publishingOrganisations) = publicationSourceReleaseAndOrganisations;

                var release = new Release
                {
                    PublicationId = request.PublicationId,
                    Year = request.Year,
                    TimePeriodCoverage = request.TimePeriodCoverage,
                    Slug = request.Slug,
                    Label = FormatReleaseLabel(request.Label),
                    RolloverSourceReleaseId = sourceRelease.Id,
                };

                var newReleaseVersion = new ReleaseVersion
                {
                    Id = guidGenerator.NewGuid(),
                    Release = release,
                    Type = request.Type!.Value,
                    ApprovalStatus = ReleaseApprovalStatus.Draft,
                    PublishingOrganisations = [.. publishingOrganisations],
                    CreatedById = userService.GetUserId(),
                };

                InitialiseContentSections(newReleaseVersion);

                context.ReleaseVersions.Add(newReleaseVersion);

                publication.ReleaseSeries.Insert(
                    0,
                    new ReleaseSeriesItem { Id = Guid.NewGuid(), ReleaseId = release.Id }
                );
                context.Publications.Update(publication);

                switch (request.Mode)
                {
                    case RolloverMode.WithData:
                        // M1.4-M1.8 copy data blocks, content, featured tables/key stats, data files and footnotes.
                        break;
                    case RolloverMode.StructureOnly:
                        // M1.5 copies content sections/layout only, with no data-block id-map.
                        break;
                }

                await context.SaveChangesAsync();

                return await releaseVersionService.GetRelease(newReleaseVersion.Id);
            });
    }

    private async Task<Either<ActionResult, Release>> ValidateSourceRelease(ReleaseRolloverCreateRequest request)
    {
        return await context
            .Releases.SingleOrNotFoundAsync(r => r.Id == request.RolloverSourceReleaseId)
            .OnSuccess(sourceRelease => CheckSourceReleaseIsEligible(sourceRelease, request.PublicationId));
    }

    private async Task<Either<ActionResult, Release>> CheckSourceReleaseIsEligible(
        Release sourceRelease,
        Guid publicationId
    )
    {
        if (sourceRelease.PublicationId != publicationId)
        {
            return ValidationActionResult(RolloverSourceReleaseMustBeInSamePublication);
        }

        var hasPublishedVersion = await context
            .ReleaseVersions.LatestReleaseVersions(
                publicationId: sourceRelease.PublicationId,
                releaseSlug: sourceRelease.Slug,
                publishedOnly: true
            )
            .AnyAsync();

        if (!hasPublishedVersion)
        {
            return ValidationActionResult(RolloverSourceReleaseMustBePublished);
        }

        return sourceRelease;
    }

    private static void InitialiseContentSections(ReleaseVersion releaseVersion)
    {
        releaseVersion.HeadlinesSection = new ContentSection { Type = ContentSectionType.Headlines };
        releaseVersion.KeyStatisticsSecondarySection = new ContentSection
        {
            Type = ContentSectionType.KeyStatisticsSecondary,
        };
        releaseVersion.RelatedDashboardsSection = new ContentSection { Type = ContentSectionType.RelatedDashboards };
        releaseVersion.SummarySection = new ContentSection { Type = ContentSectionType.ReleaseSummary };
        releaseVersion.WarningSection = new ContentSection { Type = ContentSectionType.Warning };
    }
}
