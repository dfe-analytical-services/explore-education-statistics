#nullable enable
using System.ComponentModel.DataAnnotations;
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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class PreReleaseUserService(
    ContentDbContext context,
    IEmailService emailService,
    IOptions<AppOptions> appOptions,
    IOptions<NotifyOptions> notifyOptions,
    IPreReleaseService preReleaseService,
    IPersistenceHelper<ContentDbContext> persistenceHelper,
    IUserService userService,
    IUserRepository userRepository,
    IUserReleaseRoleRepository userReleaseRoleRepository,
    IUserReleaseInviteRepository userReleaseInviteRepository
) : IPreReleaseUserService
{
    public async Task<Either<ActionResult, List<PreReleaseUserViewModel>>> GetPreReleaseUsers(Guid releaseVersionId)
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanAssignPrereleaseContactsToReleaseVersion)
            .OnSuccess(async _ =>
            {
                var emailsFromRoles = await context
                    .UserReleaseRoles.Include(r => r.User)
                    .Where(r => r.Role == ReleaseRole.PrereleaseViewer && r.ReleaseVersionId == releaseVersionId)
                    .Select(r => r.User.Email.ToLower())
                    .Distinct()
                    .ToListAsync();

                var emailsFromInvites = await context
                    .UserReleaseInvites.Where(i =>
                        i.Role == ReleaseRole.PrereleaseViewer && i.ReleaseVersionId == releaseVersionId
                    )
                    .Select(i => i.Email.ToLower())
                    .Distinct()
                    .ToListAsync();

                return emailsFromRoles
                    .Concat(emailsFromInvites)
                    .Distinct()
                    .Select(email => new PreReleaseUserViewModel(email))
                    .OrderBy(model => model.Email)
                    .ToList();
            });
    }

    public async Task<Either<ActionResult, PreReleaseUserInvitePlan>> GetPreReleaseUsersInvitePlan(
        Guid releaseVersionId,
        List<string> emails
    )
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanAssignPrereleaseContactsToReleaseVersion)
            .OnSuccess(_ => EmailValidator.ValidateEmailAddresses(emails))
            .OnSuccess<ActionResult, List<string>, PreReleaseUserInvitePlan>(async validEmails =>
            {
                var plan = new PreReleaseUserInvitePlan();
                await validEmails
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(async email =>
                    {
                        if (
                            await userReleaseRoleRepository.HasUserReleaseRole(
                                email,
                                releaseVersionId,
                                ReleaseRole.PrereleaseViewer
                            )
                        )
                        {
                            plan.AlreadyAccepted.Add(email);
                        }
                        else
                        {
                            if (
                                await userReleaseInviteRepository.UserHasInvite(
                                    releaseVersionId,
                                    email,
                                    ReleaseRole.PrereleaseViewer
                                )
                            )
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
        List<string> emails
    )
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanAssignPrereleaseContactsToReleaseVersion)
            .OnSuccessCombineWith(_ => GetPreReleaseUsersInvitePlan(releaseVersionId, emails))
            .OnSuccess<ActionResult, Tuple<ReleaseVersion, PreReleaseUserInvitePlan>, List<PreReleaseUserViewModel>>(
                async releaseVersionAndPlan =>
                {
                    var (releaseVersion, plan) = releaseVersionAndPlan;

                    var results = await plan
                        .Invitable.ToAsyncEnumerable()
                        .SelectAwait(async email => await InvitePreReleaseUser(releaseVersion, email))
                        .ToListAsync();

                    var failure = results.FirstOrDefault(sendResult => sendResult.IsLeft)?.Left;
                    if (failure != null)
                    {
                        return failure;
                    }

                    return results.Select(sendResult => sendResult.Right).ToList();
                }
            );
    }

    public async Task<Either<ActionResult, Unit>> RemovePreReleaseUser(Guid releaseVersionId, string email)
    {
        if (!new EmailAddressAttribute().IsValid(email))
        {
            return ValidationActionResult(InvalidEmailAddress);
        }

        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanAssignPrereleaseContactsToReleaseVersion)
            .OnSuccess(async () => await FindUserByEmail(email))
            .OnSuccessVoid(async user =>
            {
                await userReleaseRoleRepository.RemoveForReleaseVersionAndUser(
                    userId: user.Id,
                    releaseVersionId: releaseVersionId,
                    rolesToInclude: ReleaseRole.PrereleaseViewer
                );

                await userReleaseInviteRepository.RemoveByReleaseVersionAndEmail(
                    email: email,
                    releaseVersionId: releaseVersionId,
                    rolesToInclude: ReleaseRole.PrereleaseViewer
                );
            });
    }

    private async Task<Either<ActionResult, User>> FindUserByEmail(string email)
    {
        var user = await userRepository.FindUserByEmail(email);

        return user is null ? new NotFoundResult() : user;
    }

    private async Task<Either<ActionResult, PreReleaseUserViewModel>> InvitePreReleaseUser(
        ReleaseVersion releaseVersion,
        string email
    )
    {
        var activeUser = await userRepository.FindActiveUserByEmail(email);

        if (activeUser is not null)
        {
            return await CreateActiveUserReleaseInvite(releaseVersion, email, activeUser)
                .OnSuccess(_ => new PreReleaseUserViewModel(email));
        }

        return await context.RequireTransaction(async () =>
        {
            var createdUser = await userRepository.CreateOrUpdate(
                email: email,
                role: Role.PrereleaseUser,
                createdById: userService.GetUserId()
            );

            return await CreateInactiveUserReleaseInvite(releaseVersion, createdUser)
                .OnSuccess(_ => new PreReleaseUserViewModel(email));
        });
    }

    private async Task<Either<ActionResult, Unit>> CreateInactiveUserReleaseInvite(
        ReleaseVersion releaseVersion,
        User user
    )
    {
        if (
            !await userReleaseInviteRepository.UserHasInvite(
                releaseVersion.Id,
                user.Email,
                ReleaseRole.PrereleaseViewer
            )
        )
        {
            var sendEmail = releaseVersion.ApprovalStatus == ReleaseApprovalStatus.Approved;
            if (sendEmail)
            {
                // TODO EES-4681 - we're not currently marking this email as having been sent using
                // MarkInviteEmailAsSent, but should we be doing so?
                var emailResult = await SendPreReleaseInviteEmail(releaseVersion.Id, user.Email, isNewUser: true);
                if (emailResult.IsLeft)
                {
                    return emailResult;
                }
            }

            await userReleaseInviteRepository.Create(
                releaseVersionId: releaseVersion.Id,
                email: user.Email,
                releaseRole: ReleaseRole.PrereleaseViewer,
                emailSent: sendEmail,
                createdById: userService.GetUserId()
            );
        }

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> CreateActiveUserReleaseInvite(
        ReleaseVersion releaseVersion,
        string email,
        User user
    )
    {
        if (!await userReleaseInviteRepository.UserHasInvite(releaseVersion.Id, email, ReleaseRole.PrereleaseViewer))
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
                await userReleaseInviteRepository.Create(
                    releaseVersionId: releaseVersion.Id,
                    email: email,
                    releaseRole: ReleaseRole.PrereleaseViewer,
                    emailSent: false,
                    createdById: userService.GetUserId()
                );
            }

            await userReleaseRoleRepository.CreateIfNotExists(
                userId: user.Id,
                releaseVersionId: releaseVersion.Id,
                role: ReleaseRole.PrereleaseViewer,
                createdById: userService.GetUserId()
            );
        }

        return Unit.Instance;
    }

    public async Task<Either<ActionResult, Unit>> SendPreReleaseInviteEmail(
        Guid releaseVersionId,
        string email,
        bool isNewUser
    )
    {
        return await context
            .ReleaseVersions.Include(rv => rv.Release)
            .ThenInclude(r => r.Publication)
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId)
            .OnSuccess(releaseVersion =>
            {
                var url = appOptions.Value.Url;
                var template = notifyOptions.Value.PreReleaseTemplateId;

                var prereleaseUrl =
                    $"{url}/publication/{releaseVersion.Release.Publication.Id}/release/{releaseVersion.Id}/prerelease/content";

                var preReleaseWindow = preReleaseService.GetPreReleaseWindow(releaseVersion);
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
                    { "publish time", publishTime },
                };

                return emailService.SendEmail(email, template, emailValues);
            });
    }

    public async Task MarkInviteEmailAsSent(UserReleaseInvite invite)
    {
        invite.EmailSent = true;
        context.Update(invite);
        await context.SaveChangesAsync();
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
