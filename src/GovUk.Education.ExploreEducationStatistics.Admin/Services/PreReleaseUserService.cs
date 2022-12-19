#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
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
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

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
        private readonly IUserInviteRepository _userInviteRepository;
        private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;
        private readonly IUserReleaseInviteRepository _userReleaseInviteRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PreReleaseUserService(ContentDbContext context,
            UsersAndRolesDbContext usersAndRolesDbContext,
            IConfiguration configuration,
            IEmailService emailService,
            IPreReleaseService preReleaseService,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IUserRepository userRepository,
            IUserInviteRepository userInviteRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository,
            IUserReleaseInviteRepository userReleaseInviteRepository,
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
            _userInviteRepository = userInviteRepository;
            _userReleaseRoleRepository = userReleaseRoleRepository;
            _userReleaseInviteRepository = userReleaseInviteRepository;
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
                        var emailsFromRoles = await _context
                            .UserReleaseRoles
                            .Include(r => r.User)
                            .Where(r => r.Role == PrereleaseViewer && r.ReleaseId == releaseId)
                            .Select(r => r.User.Email.ToLower())
                            .Distinct()
                            .ToListAsync();

                        var emailsFromInvites = await _context
                            .UserReleaseInvites
                            .Where(i => i.Role == PrereleaseViewer && i.ReleaseId == releaseId)
                            .Select(i => i.Email.ToLower())
                            .Distinct()
                            .ToListAsync();

                        return emailsFromRoles
                            .Concat(emailsFromInvites)
                            .Distinct()
                            .Select(email => new PreReleaseUserViewModel(email))
                            .OrderBy(model => model.Email)
                            .ToList();
                    }
                );
        }

        public async Task<Either<ActionResult, PreReleaseUserInvitePlan>> GetPreReleaseUsersInvitePlan(
            Guid releaseId,
            List<string> emails)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanAssignPrereleaseContactsToRelease)
                .OnSuccess(_ => EmailValidator.ValidateEmailAddresses(emails))
                .OnSuccess<ActionResult, List<string>, PreReleaseUserInvitePlan>(async validEmails =>
                {
                    var plan = new PreReleaseUserInvitePlan();
                    await validEmails
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(async email =>
                        {
                            if (await _userReleaseRoleRepository
                                    .HasUserReleaseRole(email, releaseId, PrereleaseViewer))
                            {
                                plan.AlreadyAccepted.Add(email);
                            }
                            else
                            {
                                if (await _userReleaseInviteRepository
                                        .UserHasInvite(releaseId, email, PrereleaseViewer))
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
            List<string> emails)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanAssignPrereleaseContactsToRelease)
                .OnSuccessCombineWith(_ => GetPreReleaseUsersInvitePlan(releaseId, emails))
                .OnSuccess<ActionResult, Tuple<Release, PreReleaseUserInvitePlan>, List<PreReleaseUserViewModel>>(
                    async releaseAndPlan =>
                    {
                        var (release, plan) = releaseAndPlan;

                        var results = await plan.Invitable
                            .ToAsyncEnumerable()
                            .SelectAwait(async email => await InvitePreReleaseUser(release, email))
                            .ToListAsync();

                        var failure = results.FirstOrDefault(sendResult => sendResult.IsLeft)?.Left;
                        if (failure != null)
                        {
                            return failure;
                        }

                        return results
                            .Select(sendResult => sendResult.Right)
                            .ToList();
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
                        var userReleaseRolesToRemove = await _context
                            .UserReleaseRoles
                            .Include(r => r.User)
                            .Where(
                                r =>
                                    r.ReleaseId == releaseId
                                    && r.User.Email.ToLower().Equals(email.ToLower())
                                    && r.Role == PrereleaseViewer
                            ).ToListAsync();
                        await _userReleaseRoleRepository.RemoveMany(userReleaseRolesToRemove, _userService.GetUserId());

                        _context.RemoveRange(
                            _context
                                .UserReleaseInvites
                                .AsQueryable()
                                .Where(
                                    i =>
                                        i.ReleaseId == releaseId
                                        && i.Email.ToLower().Equals(email.ToLower())
                                        && i.Role == PrereleaseViewer
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
                                    i.Email.ToLower().Equals(email.ToLower())
                                    && i.Role == PrereleaseViewer
                            )
                            .CountAsync();

                        if (remainingReleaseInvites == 0)
                        {
                            _usersAndRolesDbContext.UserInvites.RemoveRange(
                                _usersAndRolesDbContext.UserInvites
                                    .AsQueryable()
                                    .Where(
                                        i =>
                                            i.Email.ToLower().Equals(email.ToLower())
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
                            && i.Role == PrereleaseViewer
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
                                .OnSuccessDo(() => MarkInviteEmailAsSent(invite));
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
                return await CreateUserReleaseInvite(release, email)
                    .OnSuccessDo(() => _userInviteRepository.CreateOrUpdate(
                        email: email,
                        role: Role.PrereleaseUser,
                        createdById: _userService.GetUserId()))
                    .OnSuccess(_ => new PreReleaseUserViewModel(email));
            }

            return await CreateExistingUserReleaseInvite(release, email, user)
                .OnSuccess(_ => new PreReleaseUserViewModel(email));
        }

        private async Task<Either<ActionResult, Unit>> CreateUserReleaseInvite(Release release, string email)
        {
            if (!await _userReleaseInviteRepository.UserHasInvite(release.Id, email, PrereleaseViewer))
            {
                var sendEmail = release.ApprovalStatus == ReleaseApprovalStatus.Approved;
                if (sendEmail)
                {
                    var emailResult = await SendPreReleaseInviteEmail(release, email, isNewUser: true);
                    if (emailResult.IsLeft)
                    {
                        return emailResult;
                    }
                }

                await _userReleaseInviteRepository.Create(
                    releaseId: release.Id,
                    email: email,
                    releaseRole: PrereleaseViewer,
                    emailSent: sendEmail,
                    createdById: _userService.GetUserId());
            }

            return Unit.Instance;
        }

        private async Task<Either<ActionResult, Unit>> CreateExistingUserReleaseInvite(Release release, string email,
            User user)
        {
            if (!await _userReleaseInviteRepository.UserHasInvite(release.Id, email, PrereleaseViewer))
            {
                var sendEmail = release.ApprovalStatus == ReleaseApprovalStatus.Approved;

                if (sendEmail)
                {
                    var emailResult = await SendPreReleaseInviteEmail(release, email, isNewUser: false);
                    if (emailResult.IsLeft)
                    {
                        return emailResult;
                    }
                }
                else
                {
                    // Create an invite. The e-mail is sent if an invite exists when the release is approved
                    await _userReleaseInviteRepository.Create(
                        releaseId: release.Id,
                        email: email,
                        releaseRole: PrereleaseViewer,
                        emailSent: false,
                        createdById: _userService.GetUserId());
                }

                await _userReleaseRoleRepository.CreateIfNotExists(
                    userId: user.Id,
                    releaseId: release.Id,
                    role: PrereleaseViewer,
                    createdById: _userService.GetUserId());
            }

            return Unit.Instance;
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

            var scheme = _httpContextAccessor.HttpContext!.Request.Scheme;
            var host = _httpContextAccessor.HttpContext.Request.Host;

            var prereleaseUrl =
                $"{scheme}://{host}/publication/{release.PublicationId}/release/{release.Id}/prerelease/content";

            var preReleaseWindow = _preReleaseService.GetPreReleaseWindow(release);
            var preReleaseWindowStart = preReleaseWindow.Start.ConvertUtcToUkTimeZone();
            var publishScheduled = release.PublishScheduled!.Value.ConvertUtcToUkTimeZone();

            // TODO EES-828 This time should depend on the Publisher schedule
            var publishScheduledTime = new TimeSpan(9, 30, 0);

            var preReleaseDay = FormatDayForEmail(preReleaseWindowStart);
            var preReleaseTime = FormatTimeForEmail(preReleaseWindowStart);
            var publishDay = FormatDayForEmail(publishScheduled);
            var publishTime = FormatTimeForEmail(publishScheduledTime);

            var emailValues = new Dictionary<string, dynamic>
            {
                { "newUser", isNewUser ? "yes" : "no" },
                { "release name", release.Title },
                { "publication name", release.Publication.Title },
                { "prerelease link", prereleaseUrl },
                { "prerelease day", preReleaseDay },
                { "prerelease time", preReleaseTime },
                { "publish day", publishDay },
                { "publish time", publishTime }
            };

            return _emailService.SendEmail(email, template, emailValues);
        }

        private async Task MarkInviteEmailAsSent(UserReleaseInvite invite)
        {
            invite.EmailSent = true;
            _context.Update(invite);
            await _context.SaveChangesAsync();
        }

        private static string FormatTimeForEmail(DateTime dateTime)
        {
            return dateTime.ToString("HH:mm");
        }

        private static string FormatTimeForEmail(TimeSpan timeSpan)
        {
            return timeSpan.ToString(@"hh\:mm");
        }

        private static string FormatDayForEmail(DateTime dateTime)
        {
            return dateTime.ToString("dddd dd MMMM yyyy");
        }
    }
}
