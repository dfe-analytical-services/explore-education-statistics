#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseInviteService(
    ContentDbContext contentDbContext,
    IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
    IReleaseVersionRepository releaseVersionRepository,
    IUserRepository userRepository,
    IUserService userService,
    IUserRoleService userRoleService,
    IUserReleaseInviteRepository userReleaseInviteRepository,
    IUserReleaseRoleRepository userReleaseRoleRepository,
    IEmailTemplateService emailTemplateService,
    IUserResourceRoleNotificationService userResourceRoleNotificationService
) : IReleaseInviteService
{
    public async Task<Either<ActionResult, Unit>> InviteContributor(
        string email,
        Guid publicationId,
        HashSet<Guid> releaseVersionIds
    )
    {
        return await contentPersistenceHelper
            .CheckEntityExists<Publication>(publicationId)
            .OnSuccessDo(publication => userService.CheckCanUpdateReleaseRole(publication, Contributor))
            .OnSuccessDo(() => ValidateReleaseVersionIds(publicationId, releaseVersionIds))
            .OnSuccess(async publication =>
            {
                var activeUser = await userRepository.FindActiveUserByEmail(email);

                return activeUser is null
                    ? await CreateInactiveUserContributorInvite(releaseVersionIds, email, publication!.Title)
                    : await CreateActiveUserContributorInvite(releaseVersionIds, activeUser, publication!.Title);
            });
    }

    public async Task<Either<ActionResult, Unit>> RemoveByPublication(
        string email,
        Guid publicationId,
        ReleaseRole releaseRole
    )
    {
        return await contentPersistenceHelper
            .CheckEntityExists<Publication>(publicationId, query => query.Include(p => p.ReleaseVersions))
            .OnSuccessDo(publication => userService.CheckCanUpdateReleaseRole(publication, releaseRole))
            .OnSuccess(async publication =>
            {
                await userReleaseInviteRepository.RemoveByPublicationAndEmail(
                    publicationId: publication.Id,
                    email: email,
                    rolesToInclude: releaseRole
                );
                return Unit.Instance;
            });
    }

    private async Task<Either<ActionResult, Unit>> CreateInactiveUserContributorInvite(
        HashSet<Guid> releaseVersionIds,
        string email,
        string publicationTitle
    )
    {
        var normalizedEmail = email.Trim().ToLower();

        var emailResult = await emailTemplateService.SendContributorInviteEmail(
            publicationTitle: publicationTitle,
            releaseVersionIds: releaseVersionIds,
            email: normalizedEmail
        );

        if (emailResult.IsLeft)
        {
            return emailResult;
        }

        await userRepository.CreateOrUpdate(
            email: normalizedEmail,
            role: Role.Analyst,
            createdById: userService.GetUserId()
        );

        await userReleaseInviteRepository.CreateManyIfNotExists(
            releaseVersionIds: [.. releaseVersionIds],
            email: normalizedEmail,
            releaseRole: Contributor,
            emailSent: true,
            createdById: userService.GetUserId()
        );

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> CreateActiveUserContributorInvite(
        HashSet<Guid> releaseVersionIds,
        User user,
        string publicationTitle
    )
    {
        // check the user doesn't already have the user release roles
        var existingReleaseRoleReleaseVersionIds = contentDbContext
            .UserReleaseRoles.AsQueryable()
            .Where(urr =>
                releaseVersionIds.Contains(urr.ReleaseVersionId) && urr.Role == Contributor && urr.UserId == user.Id
            )
            .Select(urr => urr.ReleaseVersionId)
            .ToHashSet();

        var missingReleaseRoleReleaseVersionIds = releaseVersionIds
            .Except(existingReleaseRoleReleaseVersionIds)
            .ToHashSet();

        if (!missingReleaseRoleReleaseVersionIds.Any())
        {
            return ValidationActionResult(UserAlreadyHasReleaseRoles);
        }

        await userReleaseRoleRepository.CreateManyIfNotExists(
            userId: user.Id,
            releaseVersionIds: [.. missingReleaseRoleReleaseVersionIds],
            role: Contributor,
            createdById: userService.GetUserId()
        );

        await userResourceRoleNotificationService.NotifyUserOfNewContributorRoles(
            userId: user.Id,
            publicationTitle: publicationTitle,
            releaseVersionIds: missingReleaseRoleReleaseVersionIds
        );

        var globalRoleNameToSet = userRoleService.GetAssociatedGlobalRoleNameForReleaseRole(Contributor);
        return await userRoleService.UpgradeToGlobalRoleIfRequired(globalRoleNameToSet, user.Id);
    }

    private async Task<Either<ActionResult, Unit>> ValidateReleaseVersionIds(
        Guid publicationId,
        HashSet<Guid> releaseVersionIds
    )
    {
        var distinctReleaseVersionIds = releaseVersionIds.Distinct().ToList();
        if (distinctReleaseVersionIds.Count != releaseVersionIds.Count)
        {
            throw new ArgumentException(
                $"{nameof(releaseVersionIds)} should not contain duplicates",
                nameof(releaseVersionIds)
            );
        }

        var publicationReleaseVersionIds = await releaseVersionRepository.ListLatestReleaseVersionIds(publicationId);
        if (!releaseVersionIds.All(publicationReleaseVersionIds.Contains))
        {
            return ValidationActionResult(NotAllReleasesBelongToPublication);
        }

        return Unit.Instance;
    }
}
