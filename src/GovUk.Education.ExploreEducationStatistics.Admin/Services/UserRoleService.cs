#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly UsersAndRolesDbContext _usersAndRolesDbContext;
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IPersistenceHelper<UsersAndRolesDbContext> _usersAndRolesPersistenceHelper;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IUserService _userService;
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;
        private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRoleService(UsersAndRolesDbContext usersAndRolesDbContext,
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper,
            IEmailTemplateService emailTemplateService,
            IUserService userService,
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository,
            UserManager<ApplicationUser> userManager)
        {
            _usersAndRolesDbContext = usersAndRolesDbContext;
            _contentDbContext = contentDbContext;
            _contentPersistenceHelper = contentPersistenceHelper;
            _usersAndRolesPersistenceHelper = usersAndRolesPersistenceHelper;
            _emailTemplateService = emailTemplateService;
            _userService = userService;
            _userPublicationRoleRepository = userPublicationRoleRepository;
            _userReleaseRoleRepository = userReleaseRoleRepository;
            _userManager = userManager;
        }

        public async Task<Either<ActionResult, Unit>> AddGlobalRole(string userId, string roleId)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(async () =>
                {
                    return await _usersAndRolesPersistenceHelper
                        .CheckEntityExists<ApplicationUser, string>(userId)
                        .OnSuccessCombineWith(user =>_usersAndRolesPersistenceHelper
                            .CheckEntityExists<IdentityRole, string>(roleId))
                        .OnSuccessVoid(async tuple =>
                        {
                            var (user, role) = tuple;
                            await _userManager.AddToRoleAsync(user, role.Name);
                        });
                });
        }

        public async Task<Either<ActionResult, Unit>> AddPublicationRole(Guid userId, Guid publicationId, PublicationRole role)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(async () =>
                {
                    return await _usersAndRolesPersistenceHelper
                        .CheckEntityExists<ApplicationUser, string>(userId.ToString())
                        .OnSuccessCombineWith(user => _contentPersistenceHelper.CheckEntityExists<Publication>(publicationId))
                        .OnSuccessDo(release => ValidatePublicationRoleCanBeAdded(userId, publicationId, role))
                        .OnSuccess(async tuple =>
                        {
                            var (user, publication) = tuple;
                            await _userPublicationRoleRepository.Create(
                                userId: userId,
                                publicationId: publication.Id,
                                role: role,
                                createdById: _userService.GetUserId());
                            return _emailTemplateService.SendPublicationRoleEmail(user.Email, publication, role);
                        });
                });
        }

        public async Task<Either<ActionResult, Unit>> AddReleaseRole(Guid userId, Guid releaseId, ReleaseRole role)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId, query => query
                    .Include(r => r.Publication))
                .OnSuccess(release =>
                    _userService.CheckCanUpdateReleaseRole(release.Publication, role)
                    .OnSuccess(async () =>
                    {
                        return await _usersAndRolesPersistenceHelper
                            .CheckEntityExists<ApplicationUser, string>(userId.ToString())
                            .OnSuccessDo(_ => ValidateReleaseRoleCanBeAdded(userId, releaseId, role))
                            .OnSuccess(async user =>
                            {
                                await _userReleaseRoleRepository.Create(
                                    userId: userId,
                                    releaseId: release.Id,
                                    role: role,
                                    createdById: _userService.GetUserId());
                                return _emailTemplateService.SendReleaseRoleEmail(user.Email, release, role);
                            });
                    })
                );
        }

        public async Task<Either<ActionResult, List<RoleViewModel>>> GetAllGlobalRoles()
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

        public async Task<Either<ActionResult, Dictionary<string, List<string>>>> GetAllResourceRoles()
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(_ =>
                {
                    return new Dictionary<string, List<string>>
                    {
                        {
                            "Publication",
                            Enum.GetNames(typeof(PublicationRole))
                                .OrderBy(name => name)
                                .ToList()
                        },
                        {
                            "Release",
                            Enum.GetNames(typeof(ReleaseRole))
                                .OrderBy(name => name)
                                .ToList()
                        }
                    };
                });
        }

        public async Task<Either<ActionResult, List<RoleViewModel>>> GetGlobalRoles(string userId)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(_ => _usersAndRolesPersistenceHelper.CheckEntityExists<ApplicationUser, string>(userId))
                .OnSuccess(async () =>
                {
                    var roleIds = await _usersAndRolesDbContext.UserRoles
                        .AsQueryable()
                        .Where(r => r.UserId == userId)
                        .Select(r => r.RoleId)
                        .ToListAsync();

                    return await _usersAndRolesDbContext.Roles
                        .AsQueryable()
                        .Where(r => roleIds.Contains(r.Id))
                        .OrderBy(r => r.Name)
                        .Select(r => new RoleViewModel
                        {
                            Id = r.Id,
                            Name = r.Name,
                            NormalizedName = r.NormalizedName
                        })
                        .ToListAsync();
                });
        }

        public async Task<Either<ActionResult, List<UserPublicationRoleViewModel>>> GetPublicationRoles(Guid userId)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(_ => _contentPersistenceHelper.CheckEntityExists<User>(userId))
                .OnSuccess(async () =>
                {
                    return await _contentDbContext.UserPublicationRoles
                        .Include(userPublicationRole => userPublicationRole.Publication)
                        .Where(userPublicationRole => userPublicationRole.UserId == userId)
                        .OrderBy(userPublicationRole => userPublicationRole.Publication.Title)
                        .Select(userPublicationRole => new UserPublicationRoleViewModel
                        {
                            Id = userPublicationRole.Id,
                            Publication = userPublicationRole.Publication.Title,
                            Role = userPublicationRole.Role
                        })
                        .ToListAsync();
                });
        }

        public async Task<Either<ActionResult, List<UserReleaseRoleViewModel>>> GetReleaseRoles(Guid userId)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(_ => _contentPersistenceHelper.CheckEntityExists<User>(userId))
                .OnSuccess(async () =>
                {
                    var allReleaseRoles = await _contentDbContext.UserReleaseRoles
                        .Include(userReleaseRole => userReleaseRole.Release)
                        .ThenInclude(release => release.Publication)
                        .Where(userReleaseRole => userReleaseRole.UserId == userId)
                        .ToListAsync();

                    var latestReleaseRoles = allReleaseRoles
                        .Where(userReleaseRole => userReleaseRole.Release.Publication.IsLatestVersionOfRelease(
                            userReleaseRole.Release.Id))
                        .OrderBy(userReleaseRole => userReleaseRole.Release.Publication.Title)
                        .ThenBy(userReleaseRole => userReleaseRole.Release.Year)
                        .ThenBy(userReleaseRole => userReleaseRole.Release.TimePeriodCoverage)
                        .ToList();

                    return latestReleaseRoles.Select(userReleaseRole => new UserReleaseRoleViewModel
                        {
                            Id = userReleaseRole.Id,
                            Publication = userReleaseRole.Release.Publication.Title,
                            Release = userReleaseRole.Release.Title,
                            Role = userReleaseRole.Role
                        })
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, Unit>> RemoveGlobalRole(string userId, string roleId)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(async () =>
                {
                    return await _usersAndRolesPersistenceHelper
                        .CheckEntityExists<ApplicationUser, string>(userId)
                        .OnSuccessCombineWith(user =>_usersAndRolesPersistenceHelper
                            .CheckEntityExists<IdentityRole, string>(roleId))
                        .OnSuccessVoid(async tuple =>
                        {
                            var (user, role) = tuple;
                            await _userManager.RemoveFromRoleAsync(user, role.Name);
                        });
                });
        }

        public async Task<Either<ActionResult, Unit>> RemoveUserPublicationRole(Guid userPublicationRoleId)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(() => _contentPersistenceHelper.CheckEntityExists<UserPublicationRole>(userPublicationRoleId))
                .OnSuccessVoid(async userPublicationRole =>
                {
                    _contentDbContext.Remove(userPublicationRole);
                    await _contentDbContext.SaveChangesAsync();
                });
        }

        public async Task<Either<ActionResult, Unit>> RemoveUserReleaseRole(Guid userReleaseRoleId)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<UserReleaseRole>(userReleaseRoleId)
                .OnSuccess(async userReleaseRole =>
                {
                    return await _contentPersistenceHelper
                        .CheckEntityExists<Release>(query => query
                            .Include(r => r.Publication)
                            .Where(r => r.Id == userReleaseRole.ReleaseId)
                        )
                        .OnSuccess(async release =>
                        {
                            return await _userService
                                .CheckCanUpdateReleaseRole(release.Publication, userReleaseRole.Role)
                                .OnSuccessVoid(async () =>
                                {
                                    await _userReleaseRoleRepository.Remove(userReleaseRole,
                                        deletedById: _userService.GetUserId());
                                });
                        });
                });
        }

        private async Task<Either<ActionResult, Unit>> ValidatePublicationRoleCanBeAdded(Guid userId,
            Guid publicationId,
            PublicationRole role)
        {
            if (await _userPublicationRoleRepository.UserHasRoleOnPublication(userId, publicationId, role))
            {
                return ValidationActionResult(UserAlreadyHasResourceRole);
            }

            return Unit.Instance;
        }

        private async Task<Either<ActionResult, Unit>> ValidateReleaseRoleCanBeAdded(Guid userId,
            Guid releaseId,
            ReleaseRole role)
        {
            if (await _userReleaseRoleRepository.UserHasReleaseRole(userId, releaseId, role))
            {
                return ValidationActionResult(UserAlreadyHasResourceRole);
            }

            return Unit.Instance;
        }
    }
}
