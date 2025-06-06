#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using IReleaseVersionRepository =
    GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseInviteService : IReleaseInviteService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
    private readonly IReleaseVersionRepository _releaseVersionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserService _userService;
    private readonly IUserRoleService _userRoleService;
    private readonly IUserInviteRepository _userInviteRepository;
    private readonly IUserReleaseInviteRepository _userReleaseInviteRepository;
    private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;
    private readonly IEmailService _emailService;
    private readonly IOptions<AppOptions> _appOptions;
    private readonly IOptions<NotifyOptions> _notifyOptions;

    public ReleaseInviteService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
        IReleaseVersionRepository releaseVersionRepository,
        IUserRepository userRepository,
        IUserService userService,
        IUserRoleService userRoleService,
        IUserInviteRepository userInviteRepository,
        IUserReleaseInviteRepository userReleaseInviteRepository,
        IUserReleaseRoleRepository userReleaseRoleRepository,
        IEmailService emailService,
        IOptions<AppOptions> appOptions,
        IOptions<NotifyOptions> notifyOptions)

    {
        _contentDbContext = contentDbContext;
        _contentPersistenceHelper = contentPersistenceHelper;
        _releaseVersionRepository = releaseVersionRepository;
        _userRepository = userRepository;
        _userService = userService;
        _userRoleService = userRoleService;
        _userInviteRepository = userInviteRepository;
        _userReleaseInviteRepository = userReleaseInviteRepository;
        _userReleaseRoleRepository = userReleaseRoleRepository;
        _emailService = emailService;
        _appOptions = appOptions;
        _notifyOptions = notifyOptions;
    }

    public async Task<Either<ActionResult, Unit>> InviteContributor(
        string email,
        Guid publicationId,
        List<Guid> releaseVersionIds)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<Publication>(publicationId)
            .OnSuccessDo(publication => _userService.CheckCanUpdateReleaseRole(publication, Contributor))
            .OnSuccessDo(() => ValidateReleaseVersionIds(publicationId, releaseVersionIds))
            .OnSuccess(async publication =>
            {
                var sanitisedEmail = email.Trim();

                var user = await _userRepository.FindByEmail(sanitisedEmail);
                if (user == null)
                {
                    return await CreateNewUserContributorInvite(releaseVersionIds, sanitisedEmail, publication.Title);
                }

                return await CreateExistingUserContributorInvite(releaseVersionIds, user.Id, sanitisedEmail, publication.Title);
            });
    }

    public async Task<Either<ActionResult, Unit>> RemoveByPublication(
        string email,
        Guid publicationId,
        ReleaseRole releaseRole)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<Publication>(publicationId, query => query
                .Include(p => p.ReleaseVersions))
            .OnSuccessDo(
                publication => _userService.CheckCanUpdateReleaseRole(publication, releaseRole))
            .OnSuccess(async publication =>
            {
                await _userReleaseInviteRepository.RemoveByPublication(publication, email, releaseRole);
                return Unit.Instance;
            });
    }

    private async Task<Either<ActionResult, Unit>> CreateNewUserContributorInvite(
        List<Guid> releaseVersionIds,
        string email,
        string publicationTitle)
    {
        if (await _userReleaseInviteRepository.UserHasInvites(
                releaseVersionIds: releaseVersionIds,
                email: email,
                role: Contributor))
        {
            // if the user already has UserReleaseInvites,
            // we assume they also have a UserInvite outstanding
            return ValidationActionResult(UserAlreadyHasReleaseRoleInvites);
        }

        var emailResult = await SendContributorInviteEmail(
            publicationTitle: publicationTitle,
            releaseVersionIds: releaseVersionIds,
            email: email);
        if (emailResult.IsLeft)
        {
            return emailResult;
        }

        await _userInviteRepository.CreateOrUpdate(
            email: email,
            role: Role.Analyst,
            createdById: _userService.GetUserId());

        await _userReleaseInviteRepository.CreateManyIfNotExists(
            releaseVersionIds: releaseVersionIds,
            email: email,
            releaseRole: Contributor,
            emailSent: true,
            createdById: _userService.GetUserId());

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> CreateExistingUserContributorInvite(
        List<Guid> releaseVersionIds,
        Guid userId,
        string email,
        string publicationTitle)
    {
        // check the user doesn't already have the user release roles
        var existingReleaseRoleReleaseVersionIds = _contentDbContext.UserReleaseRoles
            .AsQueryable()
            .Where(urr =>
                releaseVersionIds.Contains(urr.ReleaseVersionId)
                && urr.Role == Contributor
                && urr.UserId == userId)
            .Select(urr => urr.ReleaseVersionId)
            .ToList();

        var missingReleaseRoleReleaseVersionIds = releaseVersionIds
            .Except(existingReleaseRoleReleaseVersionIds)
            .ToList();

        if (!missingReleaseRoleReleaseVersionIds.Any())
        {
            return ValidationActionResult(UserAlreadyHasReleaseRoles);
        }

        var emailResult = await SendContributorInviteEmail(
            publicationTitle: publicationTitle,
            releaseVersionIds: missingReleaseRoleReleaseVersionIds,
            email: email);
        if (emailResult.IsLeft)
        {
            return emailResult;
        }

        await _userReleaseRoleRepository.CreateManyIfNotExists(
            userId: userId,
            releaseVersionIds: missingReleaseRoleReleaseVersionIds,
            role: Contributor,
            createdById: _userService.GetUserId()
        );

        var globalRoleNameToSet = _userRoleService
            .GetAssociatedGlobalRoleNameForReleaseRole(Contributor);
        return await _userRoleService.UpgradeToGlobalRoleIfRequired(globalRoleNameToSet, userId);
    }

    private async Task<Either<ActionResult, Unit>> ValidateReleaseVersionIds(
        Guid publicationId,
        List<Guid> releaseVersionIds)
    {
        var distinctReleaseVersionIds = releaseVersionIds.Distinct().ToList();
        if (distinctReleaseVersionIds.Count != releaseVersionIds.Count)
        {
            throw new ArgumentException($"{nameof(releaseVersionIds)} should not contain duplicates",
                nameof(releaseVersionIds));
        }

        var publicationReleaseVersionIds = await _releaseVersionRepository.ListLatestReleaseVersionIds(publicationId);
        if (!releaseVersionIds.All(publicationReleaseVersionIds.Contains))
        {
            return ValidationActionResult(NotAllReleasesBelongToPublication);
        }

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> SendContributorInviteEmail(
        string publicationTitle,
        List<Guid> releaseVersionIds,
        string email)
    {
        if (releaseVersionIds.IsNullOrEmpty())
        {
            throw new ArgumentException("List of release versions cannot be empty");
        }

        var url = _appOptions.Value.Url;
        var template = _notifyOptions.Value.ContributorTemplateId;

        var releases = await _contentDbContext.Releases
            .Where(r => r.Versions.Any(rv => releaseVersionIds.Contains(rv.Id)))
            .ToListAsync();

        var releaseTitles = releases
            .OrderBy(r => r.Year)
            .ThenBy(r => r.TimePeriodCoverage)
            .Select(r => $"* {r.Title}")
            .JoinToString('\n');

        var emailValues = new Dictionary<string, dynamic>
        {
            { "url", url },
            { "publication name", publicationTitle },
            { "release list", releaseTitles }
        };

        return _emailService.SendEmail(email, template, emailValues);
    }
}
