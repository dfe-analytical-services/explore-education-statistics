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
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

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
        private readonly IUserReleaseInviteRepository _userReleaseInviteRepository;
        private readonly UserManager<ApplicationUser> _identityUserManager;

        public UserRoleService(UsersAndRolesDbContext usersAndRolesDbContext,
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper,
            IEmailTemplateService emailTemplateService,
            IUserService userService,
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository,
            IUserReleaseInviteRepository userReleaseInviteRepository,
            UserManager<ApplicationUser> identityUserManager
            )
        {
            _usersAndRolesDbContext = usersAndRolesDbContext;
            _contentDbContext = contentDbContext;
            _contentPersistenceHelper = contentPersistenceHelper;
            _usersAndRolesPersistenceHelper = usersAndRolesPersistenceHelper;
            _emailTemplateService = emailTemplateService;
            _userService = userService;
            _userPublicationRoleRepository = userPublicationRoleRepository;
            _userReleaseRoleRepository = userReleaseRoleRepository;
            _userReleaseInviteRepository = userReleaseInviteRepository;
            _identityUserManager = identityUserManager;
        }

        public async Task<Either<ActionResult, Unit>> SetGlobalRole(string userId, string roleId)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(async () =>
                {
                    return await _usersAndRolesPersistenceHelper
                        .CheckEntityExists<ApplicationUser, string>(userId)
                        .OnSuccessCombineWith(_ =>_usersAndRolesPersistenceHelper
                            .CheckEntityExists<IdentityRole, string>(roleId))
                        .OnSuccessVoid(async tuple =>
                        {
                            var (user, role) = tuple;
                            await SetExclusiveGlobalRole(role.Name, user);
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
                        .OnSuccessCombineWith(_ => _contentPersistenceHelper.CheckEntityExists<Publication>(publicationId))
                        .OnSuccessDo(_ => ValidatePublicationRoleCanBeAdded(userId, publicationId, role))
                        .OnSuccess(async tuple =>
                        {
                            var (user, publication) = tuple;

                            await _userPublicationRoleRepository.Create(
                                userId: userId,
                                publicationId: publication.Id,
                                role: role,
                                createdById: _userService.GetUserId());

                            await UpgradeToGlobalRoleIfRequired(GetAssociatedGlobalRoleNameForPublicationRole(role), user);

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
                                    userId,
                                    release.Id,
                                    role,
                                    createdById: _userService.GetUserId());

                                var globalRole = GetAssociatedGlobalRoleNameForReleaseRole(role);
                                await UpgradeToGlobalRoleIfRequired(globalRole, user);

                                return _emailTemplateService.SendReleaseRoleEmail(user.Email, release, role);
                            });
                    })
                );
        }

        private async Task SetExclusiveGlobalRole(string? globalRoleNameToSet, ApplicationUser user)
        {
            var existingRoleNames = await _identityUserManager
                .GetRolesAsync(user) ?? new List<string>();

            if (globalRoleNameToSet == null)
            {
                await _identityUserManager.RemoveFromRolesAsync(user, existingRoleNames);
                return;
            }

            if (!existingRoleNames.Contains(globalRoleNameToSet))
            {
                await _identityUserManager.AddToRoleAsync(user, globalRoleNameToSet);
            }

            var rolesToRemove = existingRoleNames
                .Where(roleName => roleName != globalRoleNameToSet)
                .ToList();

            if (rolesToRemove.Count > 0)
            {
                await _identityUserManager.RemoveFromRolesAsync(user, rolesToRemove);
            }
        }

        public async Task<Either<ActionResult, Unit>> UpgradeToGlobalRoleIfRequired(string globalRoleNameToSet, Guid userId)
        {
            return await _usersAndRolesPersistenceHelper
                .CheckEntityExists<ApplicationUser, string>(userId.ToString())
                .OnSuccessVoid(user => UpgradeToGlobalRoleIfRequired(globalRoleNameToSet, user));
        }

        private async Task UpgradeToGlobalRoleIfRequired(string globalRoleNameToSet, ApplicationUser user)
        {
            var existingRoleNames = await _identityUserManager
                .GetRolesAsync(user) ?? new List<string>();

            var userAlreadyAssignedToRole = existingRoleNames.Contains(globalRoleNameToSet);

            var higherRoleAlreadyAssigned = !existingRoleNames
                .Intersect(GetHigherRoles(globalRoleNameToSet))
                .IsNullOrEmpty();

            if (!userAlreadyAssignedToRole && !higherRoleAlreadyAssigned)
            {
                await _identityUserManager.AddToRoleAsync(user, globalRoleNameToSet);
            }

            var lowerRolesToRemove = GetLowerRoles(globalRoleNameToSet)
                .Intersect(existingRoleNames)
                .ToList();

            if (lowerRolesToRemove.Count > 0)
            {
                await _identityUserManager.RemoveFromRolesAsync(user, lowerRolesToRemove);
            }
        }

        private async Task DowngradeFromGlobalRoleIfRequired(ApplicationUser user, string globalRoleNameToDowngradeFrom)
        {
            var existingGlobalRoleNames = await _identityUserManager
                .GetRolesAsync(user) ?? new List<string>();

            var higherPrecedenceExistingGlobalRoleNames = existingGlobalRoleNames
                .Where(role => GlobalRolePrecedenceOrder.IndexOf(role) >
                    GlobalRolePrecedenceOrder.IndexOf(globalRoleNameToDowngradeFrom));

            var requiredGlobalRoleNames =
                await GetRequiredGlobalRoleNamesForResourceRoles(user);

            var highestPrecedenceRoleNameToRetain = higherPrecedenceExistingGlobalRoleNames
                .Concat(requiredGlobalRoleNames)
                .OrderBy(GlobalRolePrecedenceOrder.IndexOf)
                .LastOrDefault();

            await SetExclusiveGlobalRole(highestPrecedenceRoleNameToRetain, user);
        }

        private async Task<List<string>> GetRequiredGlobalRoleNamesForResourceRoles(ApplicationUser user)
        {
            var releaseRoles = await _userReleaseRoleRepository.GetDistinctRolesByUser(Guid.Parse(user.Id));
            var publicationRoles = await _userPublicationRoleRepository.GetDistinctRolesByUser(Guid.Parse(user.Id));
            var requiredGlobalRoleNames =
                releaseRoles
                    .Select(GetAssociatedGlobalRoleNameForReleaseRole)
                    .Concat(publicationRoles.Select(GetAssociatedGlobalRoleNameForPublicationRole))
                    .Distinct()
                    .ToList();
            return requiredGlobalRoleNames;
        }

        public string GetAssociatedGlobalRoleNameForReleaseRole(ReleaseRole role)
        {
            switch (role)
            {
                case Viewer:
                case Contributor:
                case Lead:
                case Approver:
                    return RoleNames.Analyst;
                case PrereleaseViewer:
                    return RoleNames.PrereleaseUser;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(role),
                        role,
                        "Unable to find associated Global Role for Release Role");
            }
        }

        // For simplicity of coding styles between dealing with ReleaseRoles and PublicationRoles, leaving this `role`
        // field here even though currently we only have a single Analyst return type.
        private string GetAssociatedGlobalRoleNameForPublicationRole(PublicationRole role)
        {
            switch (role)
            {
                case PublicationRole.Owner:
                case PublicationRole.Approver:
                    return RoleNames.Analyst;
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role,
                        "Unable to find associated Global Role for Publication Role");
            }
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

        public async Task<Either<ActionResult, List<UserPublicationRoleViewModel>>> GetPublicationRolesForUser(Guid userId)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(_ => _contentPersistenceHelper.CheckEntityExists<User>(userId))
                .OnSuccess(async () =>
                {
                    return await _contentDbContext.UserPublicationRoles
                        .Include(userPublicationRole => userPublicationRole.Publication)
                        .Include(userPublicationRole => userPublicationRole.User)
                        .Where(userPublicationRole => userPublicationRole.UserId == userId)
                        .OrderBy(userPublicationRole => userPublicationRole.Publication.Title)
                        .Select(userPublicationRole => new UserPublicationRoleViewModel
                        {
                            Id = userPublicationRole.Id,
                            Publication = userPublicationRole.Publication.Title,
                            Role = userPublicationRole.Role,
                            UserName = userPublicationRole.User.DisplayName,
                            Email = userPublicationRole.User.Email
                        })
                        .ToListAsync();
                });
        }
        
        public async Task<Either<ActionResult, List<UserPublicationRoleViewModel>>> GetPublicationRolesForPublication(Guid publicationId)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(_userService.CheckCanViewPublication)
                .OnSuccess(async () =>
                {
                    return (await _contentDbContext
                        .UserPublicationRoles
                        .Include(userPublicationRole => userPublicationRole.Publication)
                        .Include(userPublicationRole => userPublicationRole.User)
                        .Where(userPublicationRole => userPublicationRole.PublicationId == publicationId)
                        .Select(userPublicationRole => new UserPublicationRoleViewModel
                        {
                            Id = userPublicationRole.Id,
                            Publication = userPublicationRole.Publication.Title,
                            Role = userPublicationRole.Role,
                            UserName = userPublicationRole.User.DisplayName,
                            Email = userPublicationRole.User.Email
                        })
                        .ToListAsync())
                        .OrderBy(userPublicationRole => userPublicationRole.UserName)
                        .ToList();
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

        public async Task<Either<ActionResult, Unit>> RemoveUserPublicationRole(Guid userPublicationRoleId)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(() => _contentPersistenceHelper.CheckEntityExists<UserPublicationRole>(userPublicationRoleId))
                .OnSuccessVoid(async role =>
                {
                    _contentDbContext.Remove(role);
                    await _contentDbContext.SaveChangesAsync();

                    var associatedGlobalRoleName = GetAssociatedGlobalRoleNameForPublicationRole(role.Role);

                    await _usersAndRolesPersistenceHelper
                        .CheckEntityExists<ApplicationUser, string>(role.UserId.ToString())
                        .OnSuccessDo(user => DowngradeFromGlobalRoleIfRequired(user, associatedGlobalRoleName));
                });
        }

        public async Task<Either<ActionResult, Unit>> RemoveUserReleaseRole(Guid userReleaseRoleId)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<UserReleaseRole>(userReleaseRoleId, query => query
                    .Include(userReleaseRole => userReleaseRole.User)
                    .Include(userReleaseRole => userReleaseRole.Release)
                    .ThenInclude(release => release.Publication))
                .OnSuccess(async userReleaseRole =>
                {
                    return await _userService
                        .CheckCanUpdateReleaseRole(userReleaseRole.Release.Publication, userReleaseRole.Role)
                        .OnSuccessVoid(async () =>
                        {
                            await _userReleaseInviteRepository.Remove(
                                userReleaseRole.Release.Id, userReleaseRole.User.Email, userReleaseRole.Role);

                            await _userReleaseRoleRepository.Remove(userReleaseRole,
                                deletedById: _userService.GetUserId());

                            var associatedGlobalRoleName = GetAssociatedGlobalRoleNameForReleaseRole(userReleaseRole.Role);

                            await _usersAndRolesPersistenceHelper
                                .CheckEntityExists<ApplicationUser, string>(userReleaseRole.UserId.ToString())
                                .OnSuccessDo(user => DowngradeFromGlobalRoleIfRequired(user, associatedGlobalRoleName));
                        });
                });
        }

        public async Task<Either<ActionResult, Unit>> RemoveAllUserResourceRoles(Guid userId)
        {
            return await _userService.CheckCanManageAllUsers()
                .OnSuccess(async _ =>
                {
                    return await _contentPersistenceHelper
                        .CheckEntityExists<User>(userId)
                        .OnSuccess(async _ =>
                        {
                            var userReleaseRoles =
                                await _contentDbContext.UserReleaseRoles.Where(urr => urr.UserId == userId)
                                    .ToListAsync();

                            if (userReleaseRoles.Any())
                            {
                                await _userReleaseRoleRepository.RemoveMany(
                                    userReleaseRoles,
                                    deletedById: _userService.GetUserId()
                                );
                            }

                            var userPublicationRoles =
                                await _contentDbContext.UserPublicationRoles
                                    .Where(upr => upr.UserId == userId)
                                    .ToListAsync();

                            if (userPublicationRoles.Any())
                            {
                                await _userPublicationRoleRepository.RemoveMany(
                                    userPublicationRoles,
                                    deletedById: _userService.GetUserId()
                                );
                            }

                            await _usersAndRolesPersistenceHelper
                                .CheckEntityExists<ApplicationUser, string>(userId.ToString())
                                .OnSuccessDo(async user =>
                                {
                                    var existingRoleNames = await _identityUserManager.GetRolesAsync(user) ?? new List<string>();

                                    await _identityUserManager.RemoveFromRolesAsync(user, existingRoleNames);
                                });

                            return Unit.Instance;
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
            if (await _userReleaseRoleRepository.HasUserReleaseRole(userId, releaseId, role))
            {
                return ValidationActionResult(UserAlreadyHasResourceRole);
            }

            return Unit.Instance;
        }
    }
}
