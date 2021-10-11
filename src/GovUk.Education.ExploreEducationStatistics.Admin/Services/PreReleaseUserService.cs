#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
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
        private readonly IUserRepository _userRepository;
        private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PreReleaseUserService(ContentDbContext context,
            UsersAndRolesDbContext usersAndRolesDbContext,
            IConfiguration configuration,
            IEmailService emailService,
            IPreReleaseService preReleaseService,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IUserRepository userRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _usersAndRolesDbContext = usersAndRolesDbContext;
            _configuration = configuration;
            _emailService = emailService;
            _preReleaseService = preReleaseService;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _userRepository = userRepository;
            _userReleaseRoleRepository = userReleaseRoleRepository;
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
                            .Select(email => new PreReleaseUserViewModel(email))
                            .ToListAsync();

                        var invitedPrereleaseContacts = await _context
                            .UserReleaseInvites
                            .AsQueryable()
                            .Where(
                                r => r.Role == ReleaseRole.PrereleaseViewer && !r.Accepted && r.ReleaseId == releaseId
                            )
                            .Select(i => i.Email.ToLower())
                            .Distinct()
                            .Select(email => new PreReleaseUserViewModel(email))
                            .ToListAsync();

                        return activePrereleaseContacts
                            .Concat(invitedPrereleaseContacts)
                            .Distinct()
                            .OrderBy(c => c.Email)
                            .ToList();
                    }
                );
        }

        public async Task<Either<ActionResult, PreReleaseInvitePlan>> GetPreReleaseUsersInvitePlan(
            Guid releaseId,
            string emails)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanAssignPrereleaseContactsToRelease)
                .OnSuccess(release => ValidateEmailsAddresses(emails))
                .OnSuccess<ActionResult, List<string>, PreReleaseInvitePlan>(async validEmails =>
                {
                    var plan = new PreReleaseInvitePlan();
                    await validEmails
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(async email =>
                        {
                            if (await UserHasPreReleaseRole(releaseId, email))
                            {
                                plan.AlreadyAccepted.Add(email);
                            }
                            else
                            {
                                if (await UserHasPreReleaseInvite(releaseId, email))
                                {
                                    plan.AlreadyInvited.Add(email);
                                }
                                else
                                {
                                    plan.Invitable.Add(email);
                                }
                            }
                        });

                    if (plan.Invitable.Count == 0)
                    {
                        return ValidationActionResult(NoInvitableEmails);
                    }

                    return plan;
                });
        }

        public async Task<Either<ActionResult, List<PreReleaseUserViewModel>>> InvitePreReleaseUsers(
            Guid releaseId,
            string emails)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanAssignPrereleaseContactsToRelease)
                .OnSuccessCombineWith(release => GetPreReleaseUsersInvitePlan(releaseId, emails))
                .OnSuccess(async releaseAndPlan =>
                {
                    var (release, plan) = releaseAndPlan;

                    var results = await plan.Invitable
                        .ToAsyncEnumerable()
                        .SelectAwait(async email => await InvitePreReleaseUser(release, email))
                        .ToListAsync();

                    var failure = results.FirstOrDefault(sendResult => sendResult.IsLeft)?.Left;
                    var successes = results.Where(sendResult => sendResult.IsRight)
                        .Select(sendResult => sendResult.Right)
                        .ToList();

                    return failure != null
                        ? new Either<ActionResult, List<PreReleaseUserViewModel>>(failure)
                        : successes;
                });
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
                                .AsQueryable()
                                .Where(
                                    i =>
                                        i.ReleaseId == releaseId
                                        && i.Email.ToLower() == email.ToLower()
                                        && i.Role == ReleaseRole.PrereleaseViewer
                                )
                        );
                        await _context.SaveChangesAsync();

                        // NOTE: UserInvites only stores whether a user has a particular role - not which release
                        // that role may be against. So we only wanted to remove the user's prerelease role from
                        // UserInvites if they no longer have any PrereleaseView roles.
                        var remainingReleaseInvites = await _context
                            .UserReleaseInvites
                            .AsQueryable()
                            .Where(
                                i =>
                                    i.Email.ToLower() == email.ToLower()
                                    && i.Role == ReleaseRole.PrereleaseViewer
                            )
                            .CountAsync();

                        if (remainingReleaseInvites == 0)
                        {
                            _usersAndRolesDbContext.UserInvites.RemoveRange(
                                _usersAndRolesDbContext.UserInvites
                                    .AsQueryable()
                                    .Where(
                                        i =>
                                            i.Email.ToLower() == email.ToLower()
                                            && i.RoleId == Role.PrereleaseUser.GetEnumValue()
                                            && !i.Accepted
                                    )
                            );

                            await _usersAndRolesDbContext.SaveChangesAsync();
                        }
                    }
                );
        }

        public async Task<Either<ActionResult, Unit>> SendPreReleaseUserInviteEmails(Guid releaseId)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess<ActionResult, Release, Unit>(async release =>
                {
                    var userReleaseInvites = await _context.UserReleaseInvites
                        .AsQueryable()
                        .Where(i =>
                            i.ReleaseId == releaseId
                            && i.Role == ReleaseRole.PrereleaseViewer
                            && !i.EmailSent)
                        .ToListAsync();

                    var results = await userReleaseInvites
                        .ToAsyncEnumerable()
                        .SelectAwait(async invite =>
                        {
                            var user = await _userRepository.FindByEmail(invite.Email);
                            return await SendPreReleaseInviteEmail(
                                    release,
                                    invite.Email.ToLower(),
                                    isNewUser: user == null)
                                .OnSuccessDo(async () => await MarkInviteEmailAsSent(invite));
                        })
                        .ToListAsync();

                    var failure = results.FirstOrDefault(sendResult => sendResult.IsLeft)?.Left;
                    if (failure != null)
                    {
                        return failure;
                    }

                    return Unit.Instance;
                });
        }

        private async Task<Either<ActionResult, PreReleaseUserViewModel>> InvitePreReleaseUser(Release release,
            string email)
        {
            var user = await _userRepository.FindByEmail(email);

            if (user == null)
            {
                await CreateUserInvite(email);
                return await CreateUserReleaseInvite(release, email)
                    .OnSuccess(_ => new PreReleaseUserViewModel(email));
            }

            return await CreateAcceptedUserReleaseInvite(release, email, user)
                .OnSuccess(_ => new PreReleaseUserViewModel(email));
        }

        private async Task<Either<ActionResult, Unit>> CreateUserReleaseInvite(Release release, string email)
        {
            if (!await UserHasPreReleaseInvite(release.Id, email))
            {
                var sendEmail = release.ApprovalStatus == ReleaseApprovalStatus.Approved;
                await _context.AddAsync(
                    new UserReleaseInvite
                    {
                        Email = email.ToLower(),
                        ReleaseId = release.Id,
                        Role = ReleaseRole.PrereleaseViewer,
                        EmailSent = sendEmail,
                        Created = UtcNow,
                        CreatedById = _userService.GetUserId()
                    }
                );
                await _context.SaveChangesAsync();

                if (sendEmail)
                {
                    return await SendPreReleaseInviteEmail(release, email, isNewUser: true);
                }
            }

            return Unit.Instance;
        }

        private async Task<Either<ActionResult, Unit>> CreateAcceptedUserReleaseInvite(Release release, string email,
            User user)
        {
            if (!await UserHasPreReleaseInvite(release.Id, email))
            {
                var sendEmail = release.ApprovalStatus == ReleaseApprovalStatus.Approved;

                // Create a new release invite
                // The invite is already accepted if the user exists
                await _context.AddAsync(
                    new UserReleaseInvite
                    {
                        Email = email.ToLower(),
                        ReleaseId = release.Id,
                        Role = ReleaseRole.PrereleaseViewer,
                        Accepted = true,
                        EmailSent = sendEmail,
                        Created = UtcNow,
                        CreatedById = _userService.GetUserId()
                    }
                );

                await _userReleaseRoleRepository.Create(
                    userId: user.Id,
                    releaseId: release.Id,
                    role: ReleaseRole.PrereleaseViewer);

                await _context.SaveChangesAsync();

                if (sendEmail)
                {
                    return await SendPreReleaseInviteEmail(release, email, isNewUser: false);
                }
            }

            return Unit.Instance;
        }

        private async Task MarkInviteEmailAsSent(UserReleaseInvite invite)
        {
            invite.EmailSent = true;
            _context.Update(invite);
            await _context.SaveChangesAsync();
        }

        private async Task<bool> UserHasPreReleaseInvite(Guid releaseId, string email)
        {
            return await _context
                .UserReleaseInvites
                .AsQueryable()
                .AnyAsync(i =>
                    i.ReleaseId == releaseId
                    && i.Email.ToLower() == email.ToLower()
                    && i.Role == ReleaseRole.PrereleaseViewer);
        }

        private async Task<bool> UserHasPreReleaseRole(Guid releaseId, string email)
        {
            return await _context
                .UserReleaseRoles
                .Include(r => r.User)
                .AnyAsync(r =>
                    r.ReleaseId == releaseId
                    && r.User.Email.ToLower() == email.ToLower()
                    && r.Role == ReleaseRole.PrereleaseViewer);
        }

        private async Task CreateUserInvite(string email)
        {
            var hasExistingSystemInvite = await _usersAndRolesDbContext
                .UserInvites
                .AsQueryable()
                .AnyAsync(i => i.Email.ToLower() == email.ToLower());

            if (!hasExistingSystemInvite)
            {
                _usersAndRolesDbContext.Add(
                    new UserInvite
                    {
                        Email = email.ToLower(),
                        RoleId = Role.PrereleaseUser.GetEnumValue(),
                        Created = UtcNow,
                        CreatedById = _userService.GetUserId().ToString()
                    }
                );

                await _usersAndRolesDbContext.SaveChangesAsync();
            }
        }

        private async Task<Either<ActionResult, Unit>> SendPreReleaseInviteEmail(
            Release release,
            string email,
            bool isNewUser)
        {
            await _context.Entry(release)
                .Reference(r => r.Publication)
                .LoadAsync();

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

            var emailValues = new Dictionary<string, dynamic?>
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

            return _emailService.SendEmail(email, template, emailValues);
        }

        private static string FormatTimeForEmail(DateTime dateTime)
        {
            return dateTime.ToString("HH:mm");
        }

        private static string FormatTimeForEmail(TimeSpan timeSpan)
        {
            return timeSpan.ToString(@"hh\:mm");
        }

        private static string? FormatDayForEmail(DateTime? dateTime)
        {
            return dateTime?.ToString("dddd dd MMMM yyyy");
        }

        private static Either<ActionResult, List<string>> ValidateEmailsAddresses(string input)
        {
            var emails = input.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Distinct()
                .ToList();

            var emailAddressAttribute = new EmailAddressAttribute();
            if (emails.Any(email => !emailAddressAttribute.IsValid(email)))
            {
                return ValidationActionResult(InvalidEmailAddress);
            }

            return emails;
        }
    }

    public class PreReleaseInvitePlan
    {
        public List<string> AlreadyInvited { get; } = new List<string>();

        public List<string> AlreadyAccepted { get; } = new List<string>();

        public List<string> Invitable { get; } = new List<string>();
    }

    public class PreReleaseUserViewModel
    {
        public PreReleaseUserViewModel(string email)
        {
            Email = email;
        }

        public string Email { get; }
    }
}
