#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class PreReleaseUserService : IPreReleaseUserService
{
    private readonly ContentDbContext _context;
    private readonly UsersAndRolesDbContext _usersAndRolesDbContext;
    private readonly IEmailService _emailService;
    private readonly IOptions<AppOptions> _appOptions;
    private readonly IOptions<NotifyOptions> _notifyOptions;
    private readonly IPreReleaseService _preReleaseService;
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;
    private readonly IUserInviteRepository _userInviteRepository;
    private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;
    private readonly IUserReleaseInviteRepository _userReleaseInviteRepository;

    public PreReleaseUserService(ContentDbContext context,
        UsersAndRolesDbContext usersAndRolesDbContext,
        IEmailService emailService,
        IOptions<AppOptions> appOptions,
        IOptions<NotifyOptions> notifyOptions,
        IPreReleaseService preReleaseService,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IUserService userService,
        IUserRepository userRepository,
        IUserInviteRepository userInviteRepository,
        IUserReleaseRoleRepository userReleaseRoleRepository,
        IUserReleaseInviteRepository userReleaseInviteRepository)
    {
        _context = context;
        _usersAndRolesDbContext = usersAndRolesDbContext;
        _emailService = emailService;
        _appOptions = appOptions;
        _notifyOptions = notifyOptions;
        _preReleaseService = preReleaseService;
        _persistenceHelper = persistenceHelper;
        _userService = userService;
        _userRepository = userRepository;
        _userInviteRepository = userInviteRepository;
        _userReleaseRoleRepository = userReleaseRoleRepository;
        _userReleaseInviteRepository = userReleaseInviteRepository;
    }

    public async Task<Either<ActionResult, List<PreReleaseUserViewModel>>> GetPreReleaseUsers(Guid releaseVersionId)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanAssignPrereleaseContactsToReleaseVersion)
            .OnSuccess(
                async _ =>
                {
                    var emailsFromRoles = await _context
                        .UserReleaseRoles
                        .Include(r => r.User)
                        .Where(r => r.Role == PrereleaseViewer && r.ReleaseVersionId == releaseVersionId)
                        .Select(r => r.User.Email.ToLower())
                        .Distinct()
                        .ToListAsync();

                    var emailsFromInvites = await _context
                        .UserReleaseInvites
                        .Where(i => i.Role == PrereleaseViewer && i.ReleaseVersionId == releaseVersionId)
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
        Guid releaseVersionId,
        List<string> emails)
    {
        return await _persistenceHelper.CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanAssignPrereleaseContactsToReleaseVersion)
            .OnSuccess(_ => EmailValidator.ValidateEmailAddresses(emails))
            .OnSuccess<ActionResult, List<string>, PreReleaseUserInvitePlan>(async validEmails =>
            {
                var plan = new PreReleaseUserInvitePlan();
                await validEmails
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(async email =>
                    {
                        if (await _userReleaseRoleRepository
                                .HasUserReleaseRole(email, releaseVersionId, PrereleaseViewer))
                        {
                            plan.AlreadyAccepted.Add(email);
                        }
                        else
                        {
                            if (await _userReleaseInviteRepository
                                    .UserHasInvite(releaseVersionId, email, PrereleaseViewer))
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
        Guid releaseVersionId,
        List<string> emails)
    {
        return await _persistenceHelper.CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanAssignPrereleaseContactsToReleaseVersion)
            .OnSuccessCombineWith(_ => GetPreReleaseUsersInvitePlan(releaseVersionId, emails))
            .OnSuccess<ActionResult, Tuple<ReleaseVersion, PreReleaseUserInvitePlan>, List<PreReleaseUserViewModel>>(
                async releaseVersionAndPlan =>
                {
                    var (releaseVersion, plan) = releaseVersionAndPlan;

                    var results = await plan.Invitable
                        .ToAsyncEnumerable()
                        .SelectAwait(async email => await InvitePreReleaseUser(releaseVersion, email))
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

    public async Task<Either<ActionResult, Unit>> RemovePreReleaseUser(Guid releaseVersionId, string email)
    {
        if (!new EmailAddressAttribute().IsValid(email))
        {
            return ValidationActionResult(InvalidEmailAddress);
        }

        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanAssignPrereleaseContactsToReleaseVersion)
            .OnSuccessVoid(
                async () =>
                {
                    var userReleaseRolesToRemove = await _context
                        .UserReleaseRoles
                        .Include(r => r.User)
                        .Where(
                            r =>
                                r.ReleaseVersionId == releaseVersionId
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
                                    i.ReleaseVersionId == releaseVersionId
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

    private async Task<Either<ActionResult, PreReleaseUserViewModel>> InvitePreReleaseUser(
        ReleaseVersion releaseVersion,
        string email)
    {
        var user = await _userRepository.FindByEmail(email);

        if (user == null)
        {
            return await CreateUserReleaseInvite(releaseVersion, email)
                .OnSuccessDo(() => _userInviteRepository.CreateOrUpdate(
                    email: email,
                    role: Role.PrereleaseUser,
                    createdById: _userService.GetUserId()))
                .OnSuccess(_ => new PreReleaseUserViewModel(email));
        }

        return await CreateExistingUserReleaseInvite(releaseVersion, email, user)
            .OnSuccess(_ => new PreReleaseUserViewModel(email));
    }

    private async Task<Either<ActionResult, Unit>> CreateUserReleaseInvite(ReleaseVersion releaseVersion,
        string email)
    {
        if (!await _userReleaseInviteRepository.UserHasInvite(releaseVersion.Id, email, PrereleaseViewer))
        {
            var sendEmail = releaseVersion.ApprovalStatus == ReleaseApprovalStatus.Approved;
            if (sendEmail)
            {
                // TODO EES-4681 - we're not currently marking this email as having been sent using
                // MarkInviteEmailAsSent, but should we be doing so?
                var emailResult = await SendPreReleaseInviteEmail(releaseVersion.Id, email, isNewUser: true);
                if (emailResult.IsLeft)
                {
                    return emailResult;
                }
            }

            await _userReleaseInviteRepository.Create(
                releaseVersionId: releaseVersion.Id,
                email: email,
                releaseRole: PrereleaseViewer,
                emailSent: sendEmail,
                createdById: _userService.GetUserId());
        }

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> CreateExistingUserReleaseInvite(ReleaseVersion releaseVersion,
        string email,
        User user)
    {
        if (!await _userReleaseInviteRepository.UserHasInvite(releaseVersion.Id, email, PrereleaseViewer))
        {
            var sendEmail = releaseVersion.ApprovalStatus == ReleaseApprovalStatus.Approved;

            if (sendEmail)
            {
                // TODO EES-4681 - we're not currently marking this email as having been sent using
                // MarkInviteEmailAsSent, but should we be doing so?
                var emailResult = await SendPreReleaseInviteEmail(releaseVersion.Id, email, isNewUser: false);
                if (emailResult.IsLeft)
                {
                    return emailResult;
                }
            }
            else
            {
                // Create an invite. The e-mail is sent if an invite exists when the release is approved
                await _userReleaseInviteRepository.Create(
                    releaseVersionId: releaseVersion.Id,
                    email: email,
                    releaseRole: PrereleaseViewer,
                    emailSent: false,
                    createdById: _userService.GetUserId());
            }

            await _userReleaseRoleRepository.CreateIfNotExists(
                userId: user.Id,
                releaseVersionId: releaseVersion.Id,
                role: PrereleaseViewer,
                createdById: _userService.GetUserId());
        }

        return Unit.Instance;
    }

    public async Task<Either<ActionResult, Unit>> SendPreReleaseInviteEmail(
        Guid releaseVersionId,
        string email,
        bool isNewUser)
    {
        return await _context.ReleaseVersions
            .Include(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId)
            .OnSuccess(releaseVersion =>
            {
                var url = _appOptions.Value.Url;
                var template = _notifyOptions.Value.PreReleaseTemplateId;

                var prereleaseUrl =
                    $"{url}/publication/{releaseVersion.Release.Publication.Id}/release/{releaseVersion.Id}/prerelease/content";

                var preReleaseWindow = _preReleaseService.GetPreReleaseWindow(releaseVersion);
                var preReleaseWindowStart = preReleaseWindow.Start.ConvertUtcToUkTimeZone();
                var publishScheduled = releaseVersion.PublishScheduled!.Value.ConvertUtcToUkTimeZone();

                // TODO EES-828 This time should depend on the Publisher schedule
                var publishScheduledTime = new TimeSpan(9, 30, 0);

                var preReleaseDay = FormatDayForEmail(preReleaseWindowStart);
                var preReleaseTime = FormatTimeForEmail(preReleaseWindowStart);
                var publishDay = FormatDayForEmail(publishScheduled);
                var publishTime = FormatTimeForEmail(publishScheduledTime);

                var emailValues = new Dictionary<string, dynamic>
                {
                    { "newUser", isNewUser ? "yes" : "no" },
                    { "release name", releaseVersion.Release.Title },
                    { "publication name", releaseVersion.Release.Publication.Title },
                    { "prerelease link", prereleaseUrl },
                    { "prerelease day", preReleaseDay },
                    { "prerelease time", preReleaseTime },
                    { "publish day", publishDay },
                    { "publish time", publishTime }
                };

                return _emailService.SendEmail(email, template, emailValues);
            });
    }

    public async Task MarkInviteEmailAsSent(UserReleaseInvite invite)
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
