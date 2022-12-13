#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Microsoft.Extensions.Configuration;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseInviteService : IReleaseInviteService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IUserRoleService _userRoleService;
        private readonly IUserInviteRepository _userInviteRepository;
        private readonly IUserReleaseInviteRepository _userReleaseInviteRepository;
        private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public ReleaseInviteService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IUserRepository userRepository,
            IUserService userService,
            IUserRoleService userRoleService,
            IUserInviteRepository userInviteRepository,
            IUserReleaseInviteRepository userReleaseInviteRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository,
            IConfiguration configuration,
            IEmailService emailService)

        {
            _contentDbContext = contentDbContext;
            _contentPersistenceHelper = contentPersistenceHelper;
            _userRepository = userRepository;
            _userService = userService;
            _userRoleService = userRoleService;
            _userInviteRepository = userInviteRepository;
            _userReleaseInviteRepository = userReleaseInviteRepository;
            _userReleaseRoleRepository = userReleaseRoleRepository;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<Either<ActionResult, Unit>> InviteContributor(string email, Guid publicationId,
            List<Guid> releaseIds)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Publication>(publicationId, query => query
                    .Include(p => p.Releases))
                .OnSuccessDo(
                    publication => _userService.CheckCanUpdateReleaseRole(publication, Contributor))
                .OnSuccess(publication => ValidateReleaseIds(publication, releaseIds))
                .OnSuccess(async publication =>
                {
                    var sanitisedEmail = email.Trim();

                    var user = await _userRepository.FindByEmail(sanitisedEmail);
                    if (user == null)
                    {
                        return await CreateNewUserContributorInvite(releaseIds, sanitisedEmail, publication.Title);
                    }

                    return await CreateExistingUserContributorInvite(releaseIds, user.Id, sanitisedEmail, publication.Title);
                });
        }

        public async Task<Either<ActionResult, Unit>> RemoveByPublication(
            string email,
            Guid publicationId,
            ReleaseRole releaseRole)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Publication>(publicationId, query => query
                    .Include(p => p.Releases))
                .OnSuccessDo(
                    publication => _userService.CheckCanUpdateReleaseRole(publication, releaseRole))
                .OnSuccess(async publication =>
                {
                    await _userReleaseInviteRepository.RemoveByPublication(publication, email, releaseRole);
                    return Unit.Instance;
                });
        }

        private async Task<Either<ActionResult, Unit>> CreateNewUserContributorInvite(List<Guid> releaseIds,
            string email, string publicationTitle)
        {
            if (await _userReleaseInviteRepository.UserHasInvites(
                    releaseIds: releaseIds,
                    email: email,
                    role: Contributor))
            {
                // if the user already has UserReleaseInvites,
                // we assume they also have a UserInvite outstanding
                return ValidationActionResult(UserAlreadyHasReleaseRoleInvites);
            }

            var emailResult = await SendContributorInviteEmail(
                publicationTitle: publicationTitle,
                releaseIds: releaseIds,
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
                releaseIds: releaseIds,
                email: email,
                releaseRole: Contributor,
                emailSent: true,
                createdById: _userService.GetUserId());

            return Unit.Instance;
        }

        private async Task<Either<ActionResult, Unit>> CreateExistingUserContributorInvite(List<Guid> releaseIds,
            Guid userId, string email, string publicationTitle)
        {
            // check the user doesn't already have the user release roles
            var existingReleaseRoleReleaseIds = _contentDbContext.UserReleaseRoles
                .AsQueryable()
                .Where(urr =>
                    releaseIds.Contains(urr.ReleaseId)
                    && urr.Role == Contributor
                    && urr.UserId == userId)
                .Select(urr => urr.ReleaseId)
                .ToList();

            var missingReleaseRoleReleaseIds = releaseIds
                .Except(existingReleaseRoleReleaseIds)
                .ToList();

            if (!missingReleaseRoleReleaseIds.Any())
            {
                return ValidationActionResult(UserAlreadyHasReleaseRoles);
            }

            var emailResult = await SendContributorInviteEmail(
                publicationTitle: publicationTitle,
                releaseIds: missingReleaseRoleReleaseIds,
                email: email);
            if (emailResult.IsLeft)
            {
                return emailResult;
            }

            await _userReleaseRoleRepository.CreateManyIfNotExists(
                userId: userId,
                releaseIds: missingReleaseRoleReleaseIds,
                role: Contributor,
                createdById: _userService.GetUserId()
            );

            var globalRoleNameToSet = _userRoleService
                .GetAssociatedGlobalRoleNameForReleaseRole(Contributor);
            return await _userRoleService.UpgradeToGlobalRoleIfRequired(globalRoleNameToSet, userId);
        }

        private async Task<Either<ActionResult, Publication>> ValidateReleaseIds(Publication publication,
            List<Guid> releaseIds)
        {
            var distinctReleaseIds = releaseIds.Distinct().ToList();
            if (distinctReleaseIds.Count != releaseIds.Count)
            {
                throw new ArgumentException($"{nameof(releaseIds)} should not contain duplicates",
                    nameof(releaseIds));
            }

            await _contentDbContext
                .Entry(publication)
                .Collection(p => p.Releases)
                .LoadAsync();

            var publicationReleaseIds = publication
                .ListActiveReleases()
                .Select(r => r.Id)
                .ToList();

            if (!releaseIds.All(publicationReleaseIds.Contains))
            {
                return ValidationActionResult(NotAllReleasesBelongToPublication);
            }

            return publication;
        }

        private async Task<Either<ActionResult, Unit>> SendContributorInviteEmail(
            string publicationTitle, List<Guid> releaseIds, string email)
        {
            if (releaseIds.IsNullOrEmpty())
            {
                throw new ArgumentException("List of releases cannot be empty");
            }

            var uri = _configuration.GetValue<string>("AdminUri");
            var template = _configuration.GetValue<string>("NotifyContributorTemplateId");

            var releases = await _contentDbContext.Releases
                .AsQueryable()
                .Where(r => releaseIds.Contains(r.Id))
                .ToListAsync();

            var releaseList = releases
                .OrderBy(r => r.Year)
                .ThenBy(r => r.TimePeriodCoverage)
                .Select(r => $"* {r.Title}")
                .ToList()
                .JoinToString("\n");

            var emailValues = new Dictionary<string, dynamic>
            {
                { "url", $"https://{uri}/" },
                { "publication name", publicationTitle },
                { "release list", releaseList },
            };

            return _emailService.SendEmail(email, template, emailValues);
        }
    }
}
