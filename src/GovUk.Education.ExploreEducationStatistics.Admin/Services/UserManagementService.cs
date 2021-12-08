#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UsersAndRolesDbContext _usersAndRolesDbContext;
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IPersistenceHelper<UsersAndRolesDbContext> _usersAndRolesPersistenceHelper;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IUserRoleService _userRoleService;
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IUserInviteRepository _userInviteRepository;
        private readonly IUserReleaseInviteRepository _userReleaseInviteRepository;
        private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserManagementService(UsersAndRolesDbContext usersAndRolesDbContext,
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper,
            IEmailTemplateService emailTemplateService,
            IUserRoleService userRoleService,
            IUserRepository userRepository,
            IUserService userService,
            IUserInviteRepository userInviteRepository,
            IUserReleaseInviteRepository userReleaseInviteRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository,
            IConfiguration configuration,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor)
        {
            _usersAndRolesDbContext = usersAndRolesDbContext;
            _contentDbContext = contentDbContext;
            _contentPersistenceHelper = contentPersistenceHelper;
            _usersAndRolesPersistenceHelper = usersAndRolesPersistenceHelper;
            _emailTemplateService = emailTemplateService;
            _userRoleService = userRoleService;
            _userRepository = userRepository;
            _userService = userService;
            _userInviteRepository = userInviteRepository;
            _userReleaseInviteRepository = userReleaseInviteRepository;
            _userReleaseRoleRepository = userReleaseRoleRepository;
            _configuration = configuration;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Either<ActionResult, List<UserViewModel>>> ListAllUsers()
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(() =>
                {
                    return _usersAndRolesDbContext.Users
                        .AsQueryable()
                        .Join(
                            _usersAndRolesDbContext.UserRoles,
                            user => user.Id,
                            userRole => userRole.UserId,
                            (user, userRole) => new
                            {
                                user,
                                userRoleId = userRole.RoleId
                            }
                        )
                        .Join(
                            _usersAndRolesDbContext.Roles,
                            prev => prev.userRoleId,
                            role => role.Id,
                            (prev, role) => new UserViewModel
                            {
                                Id = Guid.Parse(prev.user.Id),
                                Name = prev.user.FirstName + " " + prev.user.LastName,
                                Email = prev.user.Email,
                                Role = role.Name
                            }
                        )
                        .Where(uvm => uvm.Role != Role.PrereleaseUser.GetEnumLabel())
                        .OrderBy(uvm => uvm.Name)
                        .ToListAsync();
                });
        }

        public async Task<Either<ActionResult, List<TitleAndIdViewModel>>> ListPublications()
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(_ =>
                {
                    return _contentDbContext.Publications
                        .AsQueryable()
                        .Select(p => new TitleAndIdViewModel
                        {
                            Id = p.Id,
                            Title = p.Title
                        })
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, List<TitleAndIdViewModel>>> ListReleases()
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(_ =>
                {
                    return _contentDbContext.Releases
                        .Include(r => r.Publication)
                        .ToList()
                        .Where(r => r.Publication.IsLatestVersionOfRelease(r.Id))
                        .Select(r => new TitleAndIdViewModel
                        {
                            Id = r.Id,
                            Title = $"{r.Publication.Title} - {r.Title}"
                        })
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, List<RoleViewModel>>> ListRoles()
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(async () =>
                {
                    return await _usersAndRolesDbContext.Roles
                        .AsQueryable()
                        .Select(r => new RoleViewModel
                        {
                            Id = r.Id,
                            Name = r.Name,
                            NormalizedName = r.NormalizedName
                        })
                        .OrderBy(x => x.Name)
                        .ToListAsync();
                });
        }

        public async Task<List<UserViewModel>> ListPreReleaseUsersAsync()
        {
            return await _usersAndRolesDbContext.Users
                .AsQueryable()
                .Join(
                    _usersAndRolesDbContext.UserRoles,
                    user => user.Id,
                    userRole => userRole.UserId,
                    (user, userRole) => new
                    {
                        user,
                        userRoleId = userRole.RoleId
                    }
                )
                .Join(
                    _usersAndRolesDbContext.Roles,
                    prev => prev.userRoleId,
                    role => role.Id,
                    (prev, role) => new UserViewModel
                    {
                        Id = Guid.Parse(prev.user.Id),
                        Name = prev.user.FirstName + " " + prev.user.LastName,
                        Email = prev.user.Email,
                        Role = role.Name
                    }
                )
                .OrderBy(x => x.Name)
                .Where(u => u.Role == Role.PrereleaseUser.GetEnumLabel())
                .ToListAsync();
        }

        public async Task<Either<ActionResult, UserViewModel>> GetUser(Guid id)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(async () =>
                {
                    return await _usersAndRolesPersistenceHelper
                        .CheckEntityExists<ApplicationUser, string>(id.ToString())
                        .OnSuccess(async user =>
                        {
                            return await _userRoleService.GetGlobalRoles(user.Id)
                                .OnSuccessCombineWith(globalRoles => _userRoleService.GetPublicationRoles(id))
                                .OnSuccessCombineWith(globalAndPublicationRoles => _userRoleService.GetReleaseRoles(id))
                                .OnSuccess(tuple =>
                                {
                                    var (globalRoles, publicationRoles, releaseRoles) = tuple;

                                    // Currently we only allow a user to have a maximum of one global role
                                    var globalRole = globalRoles.First();

                                    return new UserViewModel
                                    {
                                        Id = id,
                                        Name = user.FirstName + " " + user.LastName,
                                        Email = user.Email,
                                        Role = globalRole.Id,
                                        UserPublicationRoles = publicationRoles,
                                        UserReleaseRoles = releaseRoles
                                    };
                                });
                        });
                });
        }

        public async Task<Either<ActionResult, List<UserViewModel>>> ListPendingInvites()
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(async () =>
                    await _usersAndRolesDbContext.UserInvites
                        .AsQueryable()
                        .Where(ui => !ui.Accepted)
                        .OrderBy(ui => ui.Email)
                        .Include(ui => ui.Role)
                        .Select(ui => new UserViewModel
                        {
                            Email = ui.Email,
                            Role = ui.Role.Name
                        }).ToListAsync()
                );
        }

        public async Task<Either<ActionResult, UserInvite>> InviteUser(string email, string roleId)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(() => ValidateEmail(email))
                .OnSuccess(() => ValidateUserDoesNotExist(email))
                .OnSuccess<ActionResult, Unit, UserInvite>(async () =>
                {
                    var role = await _usersAndRolesDbContext.Roles
                        .AsQueryable()
                        .FirstOrDefaultAsync(r => r.Id == roleId);

                    if (role == null)
                    {
                        return ValidationActionResult(InvalidUserRole);
                    }

                    return await _userInviteRepository.CreateIfNoOtherUserInvite(
                        email: email.ToLower(),
                        roleId: roleId,
                        createdById: _userService.GetUserId());
                })
                .OnSuccess(invite =>
                {
                    return _emailTemplateService
                        .SendInviteEmail(email)
                        .OnSuccess(() => invite);
                });
        }

        public async Task<Either<ActionResult, Unit>> InviteContributor(string email, Guid publicationId,
            List<Guid> releaseIds)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Publication>(publicationId, query => query
                    .Include(p => p.Releases))
                .OnSuccessDo(async publication => await _userService.CheckCanUpdateReleaseRole(publication, Contributor))
                .OnSuccessDo(() => ValidateEmail(email))
                .OnSuccess(publication => ValidateReleaseIds(publication, releaseIds))
                .OnSuccess<ActionResult, Publication, Unit>(async publication =>
                {
                    var user = await _userRepository.FindByEmail(email);
                    if (user == null)
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
                            publicationTitle: publication.Title,
                            releaseIds: releaseIds,
                            email: email);
                        if (emailResult.IsLeft)
                        {
                            return emailResult;
                        }

                        await _userInviteRepository.CreateIfNoOtherUserInvite(
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

                    // check the user doesn't already have the user release roles
                    var existingReleaseRoleReleaseIds = _contentDbContext.UserReleaseRoles
                        .AsQueryable()
                        .Where(urr =>
                            releaseIds.Contains(urr.ReleaseId)
                            && urr.Role == Contributor
                            && urr.UserId == user.Id)
                        .Select(urr => urr.ReleaseId)
                        .ToList();

                    var missingReleaseRoleReleaseIds = releaseIds
                        .Where(releaseId => !existingReleaseRoleReleaseIds.Contains(releaseId))
                        .ToList();

                    if (missingReleaseRoleReleaseIds.IsNullOrEmpty())
                    {
                        return ValidationActionResult(UserAlreadyHasReleaseRoles);
                    }

                    // @MarkFix emailResult2 name
                    var emailResult2 = await SendContributorInviteEmail(
                        publicationTitle: publication.Title,
                        releaseIds: missingReleaseRoleReleaseIds,
                        email: email);
                    if (emailResult2.IsLeft)
                    {
                        return emailResult2;
                    }

                    // create user release roles and accepted invites for release roles the user doesn't already have
                    await _userReleaseRoleRepository.CreateManyIfNotExists(
                        userId: user.Id,
                        releaseIds: missingReleaseRoleReleaseIds,
                        role: Contributor,
                        createdById: _userService.GetUserId()
                    );

                    await _userReleaseInviteRepository.CreateManyIfNotExists(
                        releaseIds: missingReleaseRoleReleaseIds,
                        email: email,
                        releaseRole: Contributor,
                        emailSent: true,
                        createdById: _userService.GetUserId(),
                        accepted: true);

                    return Unit.Instance;
                });
        }

        public async Task<Either<ActionResult, Unit>> CancelInvite(string email)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess<ActionResult, Unit, Unit>(async () =>
                {
                    var invite = await _usersAndRolesDbContext.UserInvites
                        .AsQueryable()
                        .FirstOrDefaultAsync(i => i.Email == email);

                    if (invite == null)
                    {
                        return ValidationActionResult(InviteNotFound);
                    }

                    _usersAndRolesDbContext.UserInvites.Remove(invite);
                    await _usersAndRolesDbContext.SaveChangesAsync();

                    return Unit.Instance;
                });
        }

        public async Task<Either<ActionResult, Unit>> UpdateUser(string userId, string roleId)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(async () =>
                {
                    // Currently we only allow a user to have a maximum of one global role
                    var existingRole =
                        await _usersAndRolesDbContext.UserRoles
                            .AsQueryable()
                            .FirstOrDefaultAsync(userRole => userRole.UserId == userId);

                    if (existingRole == null)
                    {
                        return await _userRoleService.AddGlobalRole(userId, roleId);
                    }

                    return await _userRoleService.RemoveGlobalRole(userId, existingRole.RoleId)
                        .OnSuccess(() => _userRoleService.AddGlobalRole(userId, roleId));
                });
        }

        private async Task<Either<ActionResult, Unit>> ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return ValidationActionResult(InvalidEmailAddress);
            }

            return Unit.Instance;
        }

        private async Task<Either<ActionResult, Unit>> ValidateUserDoesNotExist(string email)
        {
            if (_usersAndRolesDbContext.Users.Any(u => u.Email.ToLower() == email.ToLower()))
            {
                return ValidationActionResult(UserAlreadyExists);
            }

            return Unit.Instance;
        }

        private async Task<Either<ActionResult, Publication>> ValidateReleaseIds(Publication publication,
            List<Guid> releaseIds)
        {
            await _contentDbContext
                .Entry(publication)
                .Collection(p => p.Releases)
                .LoadAsync();

            var publicationReleaseIds = publication
                .ListActiveReleases()
                .Select(r => r.Id)
                .ToList();

            var intersectedReleaseIds = releaseIds.Intersect(publicationReleaseIds).ToList();

            if (!intersectedReleaseIds.All(releaseIds.Contains)) // if lists don't contain same elements
            {
                return ValidationActionResult(NotAllReleasesBelongToPublication);
            }

            return publication;
        }

        private async Task<Either<ActionResult, Unit>> SendContributorInviteEmail(
            string publicationTitle, List<Guid> releaseIds, string email)
        {
            var template = _configuration.GetValue<string>("NotifyContributorTemplateId");
            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var host = _httpContextAccessor.HttpContext.Request.Host;
            var url = $"{scheme}://{host}/";

            var releaseTitleBullets = await _contentDbContext.Releases
                .AsAsyncEnumerable()
                .Where(r => releaseIds.Contains(r.Id))
                .Select(r => $"* {r.Title}")
                .ToListAsync();
            var releaseList = string.Join("\n", releaseTitleBullets);
            if (releaseList.IsNullOrEmpty())
            {
                releaseList = "* No specific release Contributor access granted\n";
            }

            var emailValues = new Dictionary<string, dynamic>
            {
                { "url", url },
                { "publication name", publicationTitle },
                { "release list", releaseList },
            };

            return _emailService.SendEmail(email, template, emailValues);
        }
    }
}
