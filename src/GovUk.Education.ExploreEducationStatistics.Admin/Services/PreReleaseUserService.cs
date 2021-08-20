using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static System.DateTime;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class PreReleaseUserService : IPreReleaseUserService
    {
        private readonly ContentDbContext _context;
        private readonly UsersAndRolesDbContext _usersAndRolesDbContext;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IPreReleaseService _preReleaseService;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PreReleaseUserService(
            ContentDbContext context,
            UsersAndRolesDbContext usersAndRolesDbContext,
            IConfiguration configuration,
            IEmailService emailService,
            IPreReleaseService preReleaseService,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _usersAndRolesDbContext = usersAndRolesDbContext;
            _configuration = configuration;
            _emailService = emailService;
            _preReleaseService = preReleaseService;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Either<ActionResult, List<PreReleaseUserViewModel>>> GetPreReleaseUsers(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanAssignPrereleaseContactsToRelease)
                .OnSuccess(
                    async _ =>
                    {
                        var activePrereleaseContacts = await _context
                            .UserReleaseRoles
                            .Include(r => r.User)
                            .Where(r => r.Role == ReleaseRole.PrereleaseViewer && r.ReleaseId == releaseId)
                            .Select(r => r.User.Email.ToLower())
                            .Distinct()
                            .Select(
                                email => new PreReleaseUserViewModel
                                {
                                    Email = email
                                }
                            )
                            .ToListAsync();

                        var invitedPrereleaseContacts = await _context
                            .UserReleaseInvites
                            .Where(
                                r => r.Role == ReleaseRole.PrereleaseViewer && !r.Accepted && r.ReleaseId == releaseId
                            )
                            .Select(i => i.Email.ToLower())
                            .Distinct()
                            .Select(
                                email => new PreReleaseUserViewModel
                                {
                                    Email = email
                                }
                            )
                            .ToListAsync();

                        return activePrereleaseContacts
                            .Concat(invitedPrereleaseContacts)
                            .Distinct()
                            .OrderBy(c => c.Email)
                            .ToList();
                    }
                );
        }

        public async Task<Either<ActionResult, PreReleaseUserViewModel>> AddPreReleaseUser(
            Guid releaseId,
            string email)
        {
            if (!new EmailAddressAttribute().IsValid(email))
            {
                return ValidationActionResult(InvalidEmailAddress);
            }

            return await _persistenceHelper
                .CheckEntityExists<Release>(
                    releaseId,
                    q => q.Include(r => r.Publication)
                )
                .OnSuccess(_userService.CheckCanAssignPrereleaseContactsToRelease)
                .OnSuccessDo(async release => await ValidateNewPreReleaseUser(release.Id, email))
                .OnSuccess(
                    async release =>
                    {
                        var existingUser = await _context
                            .Users
                            .Where(u => u.Email.ToLower() == email.ToLower())
                            .FirstOrDefaultAsync();
                        var isExistingUser = existingUser != null;

                        if (isExistingUser)
                        {
                            _context.Add(
                                new UserReleaseRole
                                {
                                    ReleaseId = releaseId,
                                    Role = ReleaseRole.PrereleaseViewer,
                                    UserId = existingUser.Id
                                }
                            );

                            await _context.SaveChangesAsync();
                        }

                        var releaseIsApproved = release.ApprovalStatus == ReleaseApprovalStatus.Approved;

                        if (releaseIsApproved)
                        {
                            SendPreReleaseInviteEmail(release, email, !isExistingUser);
                        }

                        await CreateNewUserReleaseInvite(releaseId, email, isExistingUser, emailAlreadySent: releaseIsApproved);

                        return new PreReleaseUserViewModel
                        {
                            Email = email
                        };
                    }
                );
        }

        public async Task<Either<ActionResult, Unit>> RemovePreReleaseUser(Guid releaseId, string email)
        {
            if (!new EmailAddressAttribute().IsValid(email))
            {
                return ValidationActionResult(InvalidEmailAddress);
            }

            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanAssignPrereleaseContactsToRelease)
                .OnSuccessVoid(
                    async () =>
                    {
                        _context.RemoveRange(
                            _context
                                .UserReleaseRoles
                                .Include(r => r.User)
                                .Where(
                                    r =>
                                        r.ReleaseId == releaseId
                                        && r.User.Email.ToLower() == email.ToLower()
                                        && r.Role == ReleaseRole.PrereleaseViewer
                                )
                        );
                        _context.RemoveRange(
                            _context
                                .UserReleaseInvites
                                .Where(
                                    i =>
                                        i.ReleaseId == releaseId
                                        && i.Email.ToLower() == email.ToLower()
                                        && i.Role == ReleaseRole.PrereleaseViewer
                                )
                        );
                        await _context.SaveChangesAsync();

                        var remainingReleaseInvites = await _context
                            .UserReleaseInvites
                            .Where(
                                i =>
                                    i.Email.ToLower() == email.ToLower()
                                    && i.Role == ReleaseRole.PrereleaseViewer
                            )
                            .CountAsync();

                        if (remainingReleaseInvites == 0)
                        {
                            var role = await GetPreReleaseUserRole();

                            _usersAndRolesDbContext.UserInvites.RemoveRange(
                                _usersAndRolesDbContext.UserInvites
                                    .Where(
                                        i =>
                                            i.Email.ToLower() == email.ToLower()
                                            && i.RoleId == role.Id
                                            && !i.Accepted
                                    )
                            );

                            await _usersAndRolesDbContext.SaveChangesAsync();
                        }
                    }
                );
        }

        public async Task<Either<ActionResult, Unit>> SendPreReleaseUserInviteEmails(Release release)
        {
            var userReleaseInvites = await _context.UserReleaseInvites
                .Where(i =>
                    i.ReleaseId == release.Id
                    && i.Role == ReleaseRole.PrereleaseViewer
                    && i.EmailSent == false)
                .ToListAsync();

            await userReleaseInvites.ForEachAsync(async invite =>
            {
                var user = await _context.Users
                    .SingleOrDefaultAsync(u => u.Email.ToLower() == invite.Email.ToLower());
                SendPreReleaseInviteEmail(release, invite.Email.ToLower(), user == null);
                invite.EmailSent = true;
                _context.Update(invite);
            });

            await _context.SaveChangesAsync();
            return Unit.Instance;
        }

        private async Task<Either<ActionResult, Unit>> ValidateNewPreReleaseUser(Guid releaseId, string email)
        {
            var hasExistingReleaseRole = await _context
                .UserReleaseRoles
                .Include(r => r.User)
                .Where(
                    r =>
                        r.ReleaseId == releaseId
                        && r.User.Email.ToLower() == email.ToLower()
                        && r.Role == ReleaseRole.PrereleaseViewer
                )
                .AnyAsync();

            if (hasExistingReleaseRole)
            {
                return ValidationActionResult(UserAlreadyExists);
            }

            var hasExistingReleaseInvite = await _context
                .UserReleaseInvites
                .Where(
                    i =>
                        i.ReleaseId == releaseId
                        && i.Email.ToLower() == email.ToLower()
                         && i.Role == ReleaseRole.PrereleaseViewer
                )
                .AnyAsync();

            if (hasExistingReleaseInvite)
            {
                return ValidationActionResult(UserAlreadyExists);
            }

            return Unit.Instance;
        }

        private async Task CreateNewUserReleaseInvite(Guid releaseId, string email, bool isExistingUser, bool emailAlreadySent)
        {
            var hasExistingInvite = await _context
                .UserReleaseInvites
                .Where(
                    i =>
                        i.ReleaseId == releaseId
                        && i.Email.ToLower() == email.ToLower()
                        && i.Role == ReleaseRole.PrereleaseViewer
                )
                .AnyAsync();

            if (hasExistingInvite)
            {
                return;
            }

            _context.Add(
                new UserReleaseInvite
                {
                    Email = email.ToLower(),
                    ReleaseId = releaseId,
                    Role = ReleaseRole.PrereleaseViewer,
                    Created = UtcNow,
                    CreatedById = _userService.GetUserId(),
                    Accepted = isExistingUser,
                    EmailSent = emailAlreadySent,
                }
            );
            await _context.SaveChangesAsync();

            var hasExistingSystemInvite = await _usersAndRolesDbContext
                .UserInvites
                .Where(i => i.Email.ToLower() == email.ToLower())
                .AnyAsync();

            // TODO EES-1181 - allow multiple invites per email address to allow people to
            // be assigned multiple roles upon first login
            if (!hasExistingSystemInvite)
            {
                var role = await GetPreReleaseUserRole();

                _usersAndRolesDbContext.Add(
                    new UserInvite
                    {
                        Email = email.ToLower(),
                        Role = role,
                        Created = UtcNow,
                        // TODO
                        CreatedBy = "",
                        Accepted = isExistingUser,
                    }
                );

                await _usersAndRolesDbContext.SaveChangesAsync();
            }
        }

        private void SendPreReleaseInviteEmail(Release release, string email, bool isNewUser)
        {
            var template = _configuration.GetValue<string>("NotifyPreReleaseTemplateId");

            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var host = _httpContextAccessor.HttpContext.Request.Host;

            var prereleaseUrl =
                $"{scheme}://{host}/publication/{release.PublicationId}/release/{release.Id}/prerelease/content";

            var preReleaseWindow = _preReleaseService.GetPreReleaseWindow(release);
            var preReleaseWindowStart = preReleaseWindow.Start.ConvertUtcToUkTimeZone();
            var publishScheduled = release.PublishScheduled?.ConvertUtcToUkTimeZone();
            // TODO EES-828 This time should depend on the Publisher schedule
            var publishScheduledTime = new TimeSpan(9, 30, 0);

            var preReleaseDay = FormatDayForEmail(preReleaseWindowStart);
            var preReleaseTime = FormatTimeForEmail(preReleaseWindowStart);
            var publishDay = FormatDayForEmail(publishScheduled);
            var publishTime = FormatTimeForEmail(publishScheduledTime);

            var emailValues = new Dictionary<string, dynamic>
            {
                {"newUser", isNewUser ? "yes" : "no"},
                {"release name", release.Title},
                {"publication name", release.Publication.Title},
                {"prerelease link", prereleaseUrl},
                {"prerelease day", preReleaseDay},
                {"prerelease time", preReleaseTime},
                {"publish day", publishDay},
                {"publish time", publishTime}
            };

            _emailService.SendEmail(email, template, emailValues);
        }

        private async Task<IdentityRole> GetPreReleaseUserRole()
        {
            // TODO represent Roles with an Enum
            return await _usersAndRolesDbContext
                .Roles
                .Where(r => r.Name == "Prerelease User")
                .FirstAsync();
        }

        private static string FormatTimeForEmail(DateTime dateTime)
        {
            return dateTime.ToString("HH:mm");
        }

        private static string FormatTimeForEmail(TimeSpan timeSpan)
        {
            return timeSpan.ToString(@"hh\:mm");
        }

        private static string FormatDayForEmail(DateTime? dateTime)
        {
            return dateTime?.ToString("dddd dd MMMM yyyy");
        }
    }

    public class PreReleaseUserViewModel
    {
        public string Email { get; set; }
    }
}
