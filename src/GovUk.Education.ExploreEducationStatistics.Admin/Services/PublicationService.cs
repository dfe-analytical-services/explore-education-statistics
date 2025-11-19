#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;
using GovUk.Education.ExploreEducationStatistics.Admin.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using ExternalMethodologyViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ExternalMethodologyViewModel;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;
using PublicationViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.PublicationViewModel;
using ReleaseVersionSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ReleaseVersionSummaryViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class PublicationService(
    ContentDbContext context,
    IMapper mapper,
    IPersistenceHelper<ContentDbContext> persistenceHelper,
    IUserService userService,
    IPublicationRepository publicationRepository,
    IReleaseVersionRepository releaseVersionRepository,
    IMethodologyService methodologyService,
    IPublicationCacheService publicationCacheService,
    IReleaseCacheService releaseCacheService,
    IMethodologyCacheService methodologyCacheService,
    IRedirectsCacheService redirectsCacheService,
    IAdminEventRaiser adminEventRaiser
) : IPublicationService
{
    public async Task<Either<ActionResult, List<PublicationViewModel>>> ListPublications(
        Guid? themeId = null,
        CancellationToken cancellationToken = default
    ) =>
        await userService
            .CheckCanAccessSystem()
            .OnSuccess(_ =>
                userService
                    .CheckCanViewAllPublications()
                    .OnSuccess(async () =>
                        await HydratePublication(
                                context.Publications.If(themeId.HasValue).ThenWhere(p => p.ThemeId == themeId)
                            )
                            .ToListAsync(cancellationToken: cancellationToken)
                    )
                    .OrElse(() =>
                        publicationRepository.ListPublicationsForUser(
                            userId: userService.GetUserId(),
                            themeId: themeId,
                            cancellationToken: cancellationToken
                        )
                    )
            )
            .OnSuccess(async publications =>
                await publications
                    .ToAsyncEnumerable()
                    .SelectAwait(async publication =>
                        await GeneratePublicationViewModel(publication, cancellationToken)
                    )
                    .OrderBy(publicationViewModel => publicationViewModel.Title)
                    .ToListAsync(cancellationToken: cancellationToken)
            );

    public async Task<Either<ActionResult, List<PublicationSummaryViewModel>>> ListPublicationSummaries()
    {
        return await userService
            .CheckCanViewAllPublications()
            .OnSuccess(_ =>
            {
                return context
                    .Publications.Select(publication => new PublicationSummaryViewModel(publication))
                    .ToList();
            });
    }

    public async Task<Either<ActionResult, PublicationCreateViewModel>> CreatePublication(
        PublicationCreateRequest publication
    )
    {
        return await ValidateSelectedTheme(publication.ThemeId)
            .OnSuccess(_ => ValidatePublicationSlug(publication.Slug))
            .OnSuccess(async _ =>
            {
                var contact = await context.Contacts.AddAsync(
                    new Contact
                    {
                        ContactName = publication.Contact.ContactName,
                        ContactTelNo = string.IsNullOrWhiteSpace(publication.Contact.ContactTelNo)
                            ? null
                            : publication.Contact.ContactTelNo,
                        TeamName = publication.Contact.TeamName,
                        TeamEmail = publication.Contact.TeamEmail,
                    }
                );

                var saved = await context.Publications.AddAsync(
                    new Publication
                    {
                        Contact = contact.Entity,
                        Title = publication.Title,
                        Summary = publication.Summary,
                        ThemeId = publication.ThemeId,
                        Slug = publication.Slug,
                    }
                );

                await context.SaveChangesAsync();

                return await persistenceHelper
                    .CheckEntityExists<Publication>(saved.Entity.Id, HydratePublication)
                    .OnSuccess(GeneratePublicationCreateViewModel);
            });
    }

    public async Task<Either<ActionResult, PublicationViewModel>> UpdatePublication(
        Guid publicationId,
        PublicationSaveRequest updatedPublication
    )
    {
        return await persistenceHelper
            .CheckEntityExists<Publication>(publicationId, publication => publication.Include(p => p.SupersededBy))
            .OnSuccess(userService.CheckCanUpdatePublicationSummary)
            .OnSuccessDo(async publication =>
            {
                if (publication.Title != updatedPublication.Title)
                {
                    return await userService.CheckCanUpdatePublication();
                }

                return Unit.Instance;
            })
            .OnSuccessDo(async publication =>
            {
                if (publication.SupersededById != updatedPublication.SupersededById)
                {
                    return await userService.CheckCanUpdatePublication();
                }

                return Unit.Instance;
            })
            .OnSuccessDo(async publication =>
            {
                if (publication.ThemeId != updatedPublication.ThemeId)
                {
                    return await ValidateSelectedTheme(updatedPublication.ThemeId);
                }

                return Unit.Instance;
            })
            .OnSuccessDo(async publication =>
            {
                if (publication.ThemeId != updatedPublication.ThemeId)
                {
                    return await userService.CheckCanUpdatePublication();
                }

                return Unit.Instance;
            })
            .OnSuccess(async publication =>
            {
                var previousTitle = publication.Title;
                var previousSlug = publication.Slug;
                var previousSummary = publication.Summary;
                var previousSupersededById = publication.SupersededById;

                var titleChanged = previousTitle != updatedPublication.Title;
                var slugChanged = previousSlug != updatedPublication.Slug;
                var summaryChanged = previousSummary != updatedPublication.Summary;

                if (slugChanged)
                {
                    var slugValidation = await ValidatePublicationSlug(updatedPublication.Slug, publication.Id);

                    if (slugValidation.IsLeft)
                    {
                        return new Either<ActionResult, PublicationViewModel>(slugValidation.Left);
                    }

                    publication.Slug = updatedPublication.Slug;

                    if (
                        publication.Live
                        && context.PublicationRedirects.All(pr =>
                            !(pr.PublicationId == publicationId && pr.Slug == previousSlug)
                        )
                    ) // don't create duplicate redirect
                    {
                        var publicationRedirect = new PublicationRedirect
                        {
                            Slug = previousSlug,
                            Publication = publication,
                            PublicationId = publication.Id,
                        };
                        context.PublicationRedirects.Add(publicationRedirect);
                    }

                    // If there is an existing redirects for the new slug, they're redundant. Remove them
                    var redundantRedirects = await context
                        .PublicationRedirects.Where(pr => pr.Slug == updatedPublication.Slug)
                        .ToListAsync();
                    if (redundantRedirects.Count > 0)
                    {
                        context.PublicationRedirects.RemoveRange(redundantRedirects);
                    }
                }

                publication.Title = updatedPublication.Title;
                publication.Summary = updatedPublication.Summary;
                publication.ThemeId = updatedPublication.ThemeId;
                publication.Updated = DateTime.UtcNow;
                publication.SupersededById = updatedPublication.SupersededById;

                context.Publications.Update(publication);

                await context.SaveChangesAsync();

                if (titleChanged || slugChanged)
                {
                    await methodologyService.PublicationTitleOrSlugChanged(
                        publicationId,
                        previousSlug,
                        publication.Title,
                        publication.Slug
                    );
                }

                if (publication.Live)
                {
                    await methodologyCacheService.UpdateSummariesTree();
                    await publicationCacheService.UpdatePublicationTree();
                    await publicationCacheService.UpdatePublication(publication.Slug);

                    if (slugChanged)
                    {
                        await publicationCacheService.RemovePublication(previousSlug);
                        await redirectsCacheService.UpdateRedirects();
                    }

                    await RaiseEventIfSupersededByChanged(publication, previousSupersededById);
                    await UpdateCachedSupersededPublications(publication);
                }

                if (publication.Live && (titleChanged || slugChanged || summaryChanged))
                {
                    await adminEventRaiser.OnPublicationChanged(publication);
                }

                return await GetPublication(publication.Id);
            });
    }

    private async Task RaiseEventIfSupersededByChanged(Publication publication, Guid? previousSupersededById)
    {
        if (publication.SupersededById == previousSupersededById)
        {
            return;
        }

        var previousSupersedingPublication = await GetSupersedingPublication(previousSupersededById);
        var newSupersedingPublication = await GetSupersedingPublication(publication.SupersededById);

        var transition = PublicationArchiveStatusTransitionResolver.GetTransition(
            previousSupersedingPublication,
            newSupersedingPublication
        );

        if (
            transition
            == PublicationArchiveStatusTransitionResolver.PublicationArchiveStatusTransition.NotArchivedToArchived
        )
        {
            await adminEventRaiser.OnPublicationArchived(
                publication.Id,
                publication.Slug,
                supersededByPublicationId: publication.SupersededById!.Value
            );
        }
        else if (
            transition
            == PublicationArchiveStatusTransitionResolver.PublicationArchiveStatusTransition.ArchivedToNotArchived
        )
        {
            await adminEventRaiser.OnPublicationRestored(
                publication.Id,
                publication.Slug,
                previousSupersededByPublicationId: previousSupersededById!.Value
            );
        }
    }

    private async Task<Publication?> GetSupersedingPublication(Guid? supersededByPublicationId)
    {
        return supersededByPublicationId != null
            ? await context.Publications.SingleAsync(p => p.Id == supersededByPublicationId)
            : null;
    }

    private async Task UpdateCachedSupersededPublications(Publication publication)
    {
        // NOTE: When a publication is updated, any publication that is superseded by it can be affected, so
        // update any superseded publications that are cached
        var supersededPublications = await context
            .Publications.Where(p => p.SupersededById == publication.Id)
            .ToListAsync();

        await supersededPublications
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(p => publicationCacheService.UpdatePublication(p.Slug));
    }

    private async Task<Either<ActionResult, Unit>> ValidateSelectedTheme(Guid themeId)
    {
        var theme = await context.Themes.FindAsync(themeId);

        if (theme is null)
        {
            return ValidationActionResult(ThemeDoesNotExist);
        }

        return await userService.CheckCanCreatePublicationForTheme(theme).OnSuccess(_ => Unit.Instance);
    }

    public async Task<Either<ActionResult, PublicationViewModel>> GetPublication(
        Guid publicationId,
        bool includePermissions = false,
        CancellationToken cancellationToken = default
    ) =>
        await persistenceHelper
            .CheckEntityExists<Publication>(publicationId, HydratePublication)
            .OnSuccess(userService.CheckCanViewPublication)
            .OnSuccess(async publication =>
            {
                var viewModel = await GeneratePublicationViewModel(publication, cancellationToken);
                if (includePermissions)
                {
                    viewModel.Permissions = await PermissionsUtils.GetPublicationPermissions(userService, publication);
                }
                return viewModel;
            });

    public async Task<Either<ActionResult, ExternalMethodologyViewModel>> GetExternalMethodology(Guid publicationId)
    {
        return await persistenceHelper
            .CheckEntityExists<Publication>(publicationId)
            .OnSuccessDo(userService.CheckCanViewPublication)
            .OnSuccess(publication =>
                publication.ExternalMethodology != null
                    ? new ExternalMethodologyViewModel(publication.ExternalMethodology)
                    : NotFound<ExternalMethodologyViewModel>()
            );
    }

    public async Task<Either<ActionResult, ExternalMethodologyViewModel>> UpdateExternalMethodology(
        Guid publicationId,
        ExternalMethodologySaveRequest updatedExternalMethodology
    )
    {
        return await persistenceHelper
            .CheckEntityExists<Publication>(publicationId)
            .OnSuccessDo(userService.CheckCanManageExternalMethodologyForPublication)
            .OnSuccess(async publication =>
            {
                context.Update(publication);
                publication.ExternalMethodology ??= new ExternalMethodology();
                publication.ExternalMethodology.Title = updatedExternalMethodology.Title;
                publication.ExternalMethodology.Url = updatedExternalMethodology.Url;
                await context.SaveChangesAsync();

                // Update publication cache because ExternalMethodology is in Content.Services.ViewModels.PublicationViewModel
                await publicationCacheService.UpdatePublication(publication.Slug);

                return new ExternalMethodologyViewModel(publication.ExternalMethodology);
            });
    }

    public async Task<Either<ActionResult, Unit>> RemoveExternalMethodology(Guid publicationId)
    {
        return await persistenceHelper
            .CheckEntityExists<Publication>(publicationId)
            .OnSuccessDo(userService.CheckCanManageExternalMethodologyForPublication)
            .OnSuccess(async publication =>
            {
                context.Update(publication);
                publication.ExternalMethodology = null;
                await context.SaveChangesAsync();

                // Clear cache because ExternalMethodology is in Content.Services.ViewModels.PublicationViewModel
                await publicationCacheService.UpdatePublication(publication.Slug);

                return Unit.Instance;
            });
    }

    public async Task<Either<ActionResult, ContactViewModel>> GetContact(Guid publicationId)
    {
        return await persistenceHelper
            .CheckEntityExists<Publication>(publicationId, query => query.Include(p => p.Contact))
            .OnSuccessDo(userService.CheckCanViewPublication)
            .OnSuccess(publication => mapper.Map<ContactViewModel>(publication.Contact));
    }

    public async Task<Either<ActionResult, ContactViewModel>> UpdateContact(
        Guid publicationId,
        ContactSaveRequest updatedContact
    )
    {
        return await persistenceHelper
            .CheckEntityExists<Publication>(publicationId, query => query.Include(p => p.Contact))
            .OnSuccessDo(userService.CheckCanUpdateContact)
            .OnSuccess(async publication =>
            {
                // Replace existing contact that is shared with another publication with a new
                // contact, as we want each publication to have its own contact.
                if (context.Publications.Any(p => p.ContactId == publication.ContactId && p.Id != publication.Id))
                {
                    publication.Contact = new Contact();
                }

                publication.Contact.ContactName = updatedContact.ContactName;
                publication.Contact.ContactTelNo = string.IsNullOrWhiteSpace(updatedContact.ContactTelNo)
                    ? null
                    : updatedContact.ContactTelNo;
                publication.Contact.TeamName = updatedContact.TeamName;
                publication.Contact.TeamEmail = updatedContact.TeamEmail;
                await context.SaveChangesAsync();

                // Clear cache because Contact is in Content.Services.ViewModels.PublicationViewModel
                await publicationCacheService.UpdatePublication(publication.Slug);

                return mapper.Map<ContactViewModel>(publication.Contact);
            });
    }

    public async Task<
        Either<ActionResult, PaginatedListViewModel<ReleaseVersionSummaryViewModel>>
    > ListReleaseVersionsPaginated(
        Guid publicationId,
        ReleaseVersionsType versionsType,
        int page = 1,
        int pageSize = 5,
        bool includePermissions = false
    )
    {
        return await ListReleaseVersions(publicationId, versionsType, includePermissions)
            .OnSuccess(releases =>
                // This is not ideal - we should paginate results in the database, however,
                // this is not possible as we need to iterate over all releases to get the
                // latest/active versions of releases. Ideally, we should be able to
                // pagination entirely in the database, but this requires re-modelling of releases.
                // TODO: EES-3663 Use database pagination when ReleaseVersions are introduced
                PaginatedListViewModel<ReleaseVersionSummaryViewModel>.Paginate(releases, page, pageSize)
            );
    }

    public async Task<Either<ActionResult, List<ReleaseVersionSummaryViewModel>>> ListReleaseVersions(
        Guid publicationId,
        ReleaseVersionsType versionsType,
        bool includePermissions = false
    )
    {
        return await persistenceHelper
            .CheckEntityExists<Publication>(publicationId)
            .OnSuccess(userService.CheckCanViewPublication)
            .OnSuccess(async () =>
            {
                var releaseVersions = await ListReleaseVersions(publicationId, versionsType);

                return await releaseVersions
                    .ToAsyncEnumerable()
                    .SelectAwait(async releaseVersion =>
                    {
                        await context.ReleaseVersions.Entry(releaseVersion).Reference(rv => rv.Release).LoadAsync();

                        return mapper.Map<ReleaseVersionSummaryViewModel>(releaseVersion) with
                        {
                            Permissions = includePermissions
                                ? await PermissionsUtils.GetReleasePermissions(userService, releaseVersion)
                                : null,
                        };
                    })
                    .ToListAsync();
            });
    }

    public async Task<Either<ActionResult, List<ReleaseSeriesTableEntryViewModel>>> GetReleaseSeries(Guid publicationId)
    {
        return await context
            .Publications.FirstOrNotFoundAsync(p => p.Id == publicationId)
            .OnSuccess(userService.CheckCanViewPublication)
            .OnSuccess(async publication =>
            {
                var result = new List<ReleaseSeriesTableEntryViewModel>();
                foreach (var seriesItem in publication.ReleaseSeries)
                {
                    if (seriesItem.IsLegacyLink)
                    {
                        result.Add(
                            new ReleaseSeriesTableEntryViewModel
                            {
                                Id = seriesItem.Id,
                                Description = seriesItem.LegacyLinkDescription!,
                                LegacyLinkUrl = seriesItem.LegacyLinkUrl,
                            }
                        );
                    }
                    else
                    {
                        var release = await context.Releases.SingleAsync(r => r.Id == seriesItem.ReleaseId);

                        var latestPublishedReleaseVersion = await context
                            .ReleaseVersions.LatestReleaseVersion(
                                releaseId: seriesItem.ReleaseId!.Value,
                                publishedOnly: true
                            )
                            .SingleOrDefaultAsync();

                        result.Add(
                            new ReleaseSeriesTableEntryViewModel
                            {
                                Id = seriesItem.Id,
                                ReleaseId = release.Id,
                                Description = release.Title,
                                ReleaseSlug = release.Slug,
                                IsLatest =
                                    publication.LatestPublishedReleaseVersionId != null
                                    && latestPublishedReleaseVersion?.Id == publication.LatestPublishedReleaseVersionId,
                                IsPublished = latestPublishedReleaseVersion != null,
                            }
                        );
                    }
                }

                return result;
            });
    }

    public async Task<Either<ActionResult, List<ReleaseSeriesTableEntryViewModel>>> AddReleaseSeriesLegacyLink(
        Guid publicationId,
        ReleaseSeriesLegacyLinkAddRequest newLegacyLink
    )
    {
        return await context
            .Publications.FirstOrNotFoundAsync(p => p.Id == publicationId)
            .OnSuccess(userService.CheckCanManageReleaseSeries)
            .OnSuccess(async publication =>
            {
                publication.ReleaseSeries.Add(
                    new ReleaseSeriesItem
                    {
                        Id = Guid.NewGuid(),
                        LegacyLinkDescription = newLegacyLink.Description,
                        LegacyLinkUrl = newLegacyLink.Url,
                    }
                );

                context.Publications.Update(publication);
                await context.SaveChangesAsync();

                await publicationCacheService.UpdatePublication(publication.Slug);

                return await GetReleaseSeries(publication.Id);
            });
    }

    public async Task<Either<ActionResult, List<ReleaseSeriesTableEntryViewModel>>> UpdateReleaseSeries(
        Guid publicationId,
        List<ReleaseSeriesItemUpdateRequest> updatedReleaseSeriesItems
    )
    {
        return await context
            .Publications.Include(p => p.SupersededBy)
            .FirstOrNotFoundAsync(p => p.Id == publicationId)
            .OnSuccess(userService.CheckCanManageReleaseSeries)
            .OnSuccess(async publication =>
            {
                // Check new series items details are correct
                foreach (var seriesItem in updatedReleaseSeriesItems)
                {
                    if (
                        seriesItem.ReleaseId != null
                        && (seriesItem.LegacyLinkDescription != null || seriesItem.LegacyLinkUrl != null)
                    )
                    {
                        throw new ArgumentException("LegacyLink details shouldn't be set if ReleaseId is set.");
                    }

                    if (
                        seriesItem.ReleaseId == null
                        && (seriesItem.LegacyLinkDescription == null || seriesItem.LegacyLinkUrl == null)
                    )
                    {
                        throw new ArgumentException("LegacyLink details should be set if ReleaseId is null.");
                    }
                }

                // Check all publication releases are included in updatedReleaseSeriesItems
                var publicationReleaseIds = await context
                    .Releases.Where(r => r.PublicationId == publicationId)
                    .Select(r => r.Id)
                    .ToListAsync();

                var updatedSeriesReleaseIds = updatedReleaseSeriesItems
                    .Where(rsi => rsi.ReleaseId.HasValue)
                    .Select(rsi => rsi.ReleaseId!.Value)
                    .ToList();

                if (!ComparerUtils.SequencesAreEqualIgnoringOrder(publicationReleaseIds, updatedSeriesReleaseIds))
                {
                    throw new ArgumentException(
                        "Missing or duplicate release in new release series. Expected ReleaseIds: "
                            + publicationReleaseIds.JoinToString(",")
                    );
                }

                // Work out the publication's new latest published release version (if any).
                // This is the latest published version of the first release which has a published version
                var allPublishedReleasesAndLatestVersion = await GetAllPublishedReleasesAndLatestVersions();

                async Task<ICollection<ReleaseAndVersion>> GetAllPublishedReleasesAndLatestVersions()
                {
                    var releaseAndVersions = new List<ReleaseAndVersion>();
                    foreach (var releaseId in updatedSeriesReleaseIds)
                    {
                        var latestPublishedReleaseVersionIdForReleaseId = (
                            await context
                                .ReleaseVersions.LatestReleaseVersion(releaseId: releaseId, publishedOnly: true)
                                .SingleOrDefaultAsync()
                        )?.Id;

                        if (latestPublishedReleaseVersionIdForReleaseId != null)
                        {
                            releaseAndVersions.Add(
                                new ReleaseAndVersion(releaseId, latestPublishedReleaseVersionIdForReleaseId.Value)
                            );
                        }
                    }
                    return releaseAndVersions;
                }

                // Get the about-to-be replaced release and version.
                var oldLatestPublishedReleaseAndVersion =
                    publication.LatestPublishedReleaseVersionId != null
                        ? allPublishedReleasesAndLatestVersion.Single(rav =>
                            rav.ReleaseVersionId == publication.LatestPublishedReleaseVersionId
                        )
                        : null;

                // Set the latest published release version
                var newLatestPublishedReleaseAndVersion = allPublishedReleasesAndLatestVersion.FirstOrDefault();
                publication.LatestPublishedReleaseVersionId = newLatestPublishedReleaseAndVersion?.ReleaseVersionId;

                publication.ReleaseSeries = updatedReleaseSeriesItems
                    .Select(request => new ReleaseSeriesItem
                    {
                        Id = Guid.NewGuid(),
                        ReleaseId = request.ReleaseId,
                        LegacyLinkDescription = request.LegacyLinkDescription,
                        LegacyLinkUrl = request.LegacyLinkUrl,
                    })
                    .ToList();

                await context.SaveChangesAsync();

                // Update the cached publication
                await publicationCacheService.UpdatePublication(publication.Slug);

                // If the publication's latest published release version has changed,
                // update the publication's cached latest release version
                if (
                    oldLatestPublishedReleaseAndVersion != newLatestPublishedReleaseAndVersion
                    && newLatestPublishedReleaseAndVersion != null
                )
                {
                    await releaseCacheService.UpdateRelease(
                        releaseVersionId: newLatestPublishedReleaseAndVersion.ReleaseVersionId,
                        publicationSlug: publication.Slug
                    );

                    // The reordering of the series implies that there was already a published release version,
                    // therefore, this should always have a value
                    if (oldLatestPublishedReleaseAndVersion != null)
                    {
                        await adminEventRaiser.OnPublicationLatestPublishedReleaseReordered(
                            publication,
                            oldLatestPublishedReleaseAndVersion.ReleaseId,
                            oldLatestPublishedReleaseAndVersion.ReleaseVersionId
                        );
                    }
                }

                return await GetReleaseSeries(publication.Id);
            });
    }

    private async Task<Either<ActionResult, Unit>> ValidatePublicationSlug(string newSlug, Guid? publicationId = null)
    {
        if (
            await context.Publications.AnyAsync(publication =>
                publication.Id != publicationId && publication.Slug == newSlug
            )
        )
        {
            return ValidationActionResult(PublicationSlugNotUnique);
        }

        var hasRedirect = await context.PublicationRedirects.AnyAsync(pr =>
            pr.PublicationId != publicationId // If publication previously used this slug, can change it back
            && pr.Slug == newSlug
        );

        if (hasRedirect)
        {
            return ValidationActionResult(PublicationSlugUsedByRedirect);
        }

        if (
            publicationId != null
            && context.PublicationMethodologies.Any(pm => pm.Publication.Id == publicationId && pm.Owner)
        // Strictly, we should also check whether the owned methodology inherits the publication slug - we don't
        // need to validate the new slug against methodologies if it isn't changing the methodology slug - but
        // this check is expensive and an unlikely edge case, so doesn't seem worth it.
        )
        {
            var methodologySlugValidation = await methodologyService.ValidateMethodologySlug(newSlug);
            if (methodologySlugValidation.IsLeft)
            {
                return methodologySlugValidation.Left;
            }
        }

        return Unit.Instance;
    }

    public static IQueryable<Publication> HydratePublication(IQueryable<Publication> values) =>
        values.Include(p => p.Contact).Include(p => p.Theme);

    private async Task<PublicationViewModel> GeneratePublicationViewModel(
        Publication publication,
        CancellationToken cancellationToken
    )
    {
        var isSuperseded = await publicationRepository.IsSuperseded(publication.Id, cancellationToken);
        return PublicationViewModel.FromModel(publication, isSuperseded);
    }

    private async Task<PublicationCreateViewModel> GeneratePublicationCreateViewModel(Publication publication)
    {
        var publicationCreateViewModel = mapper.Map<PublicationCreateViewModel>(publication);

        publicationCreateViewModel.IsSuperseded = await publicationRepository.IsSuperseded(publication.Id);

        return publicationCreateViewModel;
    }

    private async Task<List<ReleaseVersion>> ListReleaseVersions(Guid publicationId, ReleaseVersionsType versionsType)
    {
        return versionsType switch
        {
            ReleaseVersionsType.Latest => await releaseVersionRepository.ListLatestReleaseVersions(publicationId),
            ReleaseVersionsType.LatestPublished => await releaseVersionRepository.ListLatestReleaseVersions(
                publicationId,
                publishedOnly: true
            ),
            ReleaseVersionsType.NotPublished => (
                await releaseVersionRepository.ListLatestReleaseVersions(publicationId)
            )
                .Where(rv => rv.Live == false)
                .ToList(),
            _ => throw new Exception(),
        };
    }

    private record ReleaseAndVersion(Guid ReleaseId, Guid ReleaseVersionId);
}
