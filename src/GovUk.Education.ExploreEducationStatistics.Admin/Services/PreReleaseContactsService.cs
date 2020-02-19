using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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
    public class PreReleaseContactsService : IPreReleaseContactsService
    {
        private readonly ContentDbContext _context;
        private readonly UsersAndRolesDbContext _usersAndRolesDbContext;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PreReleaseContactsService(ContentDbContext context, IUserService userService,
            IPersistenceHelper<ContentDbContext> persistenceHelper, UsersAndRolesDbContext usersAndRolesDbContext,
            IEmailService emailService, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userService = userService;
            _persistenceHelper = persistenceHelper;
            _usersAndRolesDbContext = usersAndRolesDbContext;
            _emailService = emailService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Either<ActionResult, List<PrereleaseCandidateViewModel>>>
            GetAvailablePreReleaseContactsAsync()
        {
            return await _userService
                .CheckCanViewPrereleaseContactsList()
                .OnSuccess(async _ =>
                {
                    var activePrereleaseContacts = await _context
                        .UserReleaseRoles
                        .Include(r => r.User)
                        .Where(r => r.Role == ReleaseRole.PrereleaseViewer)
                        .Select(r => r.User.Email.ToLower())
                        .Distinct()
                        .Select(email => new PrereleaseCandidateViewModel
                        {
                            Email = email,
                            Invited = false
                        })
                        .ToListAsync();

                    var invitedPrereleaseContacts = await _context
                        .UserReleaseInvites
                        .Where(r => r.Role == ReleaseRole.PrereleaseViewer && !r.Accepted)
                        .Select(i => i.Email.ToLower())
                        .Distinct()
                        .Select(email => new PrereleaseCandidateViewModel
                        {
                            Email = email,
                            Invited = true
                        })
                        .ToListAsync();

                    return activePrereleaseContacts
                        .Concat(invitedPrereleaseContacts)
                        .Distinct()
                        .OrderBy(c => c.Email)
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, List<PrereleaseCandidateViewModel>>>
            GetPreReleaseContactsForReleaseAsync(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanAssignPrereleaseContactsToRelease)
                .OnSuccess(async _ =>
                {
                    var activePrereleaseContacts = await _context
                        .UserReleaseRoles
                        .Include(r => r.User)
                        .Where(r => r.Role == ReleaseRole.PrereleaseViewer && r.ReleaseId == releaseId)
                        .Select(r => r.User.Email.ToLower())
                        .Distinct()
                        .Select(email => new PrereleaseCandidateViewModel
                        {
                            Email = email,
                            Invited = false
                        })
                        .ToListAsync();

                    var invitedPrereleaseContacts = await _context
                        .UserReleaseInvites
                        .Where(r => r.Role == ReleaseRole.PrereleaseViewer && !r.Accepted && r.ReleaseId == releaseId)
                        .Select(i => i.Email.ToLower())
                        .Distinct()
                        .Select(email => new PrereleaseCandidateViewModel
                        {
                            Email = email,
                            Invited = true
                        })
                        .ToListAsync();

                    return activePrereleaseContacts
                        .Concat(invitedPrereleaseContacts)
                        .Distinct()
                        .OrderBy(c => c.Email)
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, List<PrereleaseCandidateViewModel>>> AddPreReleaseContactToReleaseAsync(
            Guid releaseId, string email)
        {
            if (!new EmailAddressAttribute().IsValid(email))
            {
                return ValidationActionResult(InvalidEmailAddress);
            }
            
            var newUser = false;
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanAssignPrereleaseContactsToRelease)
                .OnSuccessDo(async () =>
                {
                    if (!await _context
                        .UserReleaseRoles
                        .Include(r => r.User)
                        .AnyAsync(r =>
                            r.ReleaseId == releaseId
                            && r.User.Email.ToLower() == email.ToLower()
                            && r.Role == ReleaseRole.PrereleaseViewer))
                    {
                        var existingUser = await _context
                            .Users
                            .Where(u => u.Email.ToLower() == email.ToLower())
                            .FirstOrDefaultAsync();

                        if (existingUser != null)
                        {
                            _context.Add(new UserReleaseRole
                            {
                                ReleaseId = releaseId,
                                Role = ReleaseRole.PrereleaseViewer,
                                UserId = existingUser.Id
                            });
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            var existingInvite = await _context
                                .UserReleaseInvites
                                .Where(i =>
                                    i.ReleaseId == releaseId
                                    && i.Email.ToLower() == email.ToLower()
                                    && i.Role == ReleaseRole.PrereleaseViewer)
                                .FirstOrDefaultAsync();

                            if (existingInvite == null)
                            {
                                newUser = true;

                                _context.Add(new UserReleaseInvite
                                {
                                    Email = email.ToLower(),
                                    ReleaseId = releaseId,
                                    Role = ReleaseRole.PrereleaseViewer,
                                    Created = UtcNow,
                                    CreatedById = _userService.GetUserId()
                                });
                                await _context.SaveChangesAsync();

                                var existingInviteToSystem = await _usersAndRolesDbContext
                                    .UserInvites
                                    .Where(i => i.Email.ToLower() == email.ToLower())
                                    .FirstOrDefaultAsync();

                                // TODO EES-1181 - allow multiple invites per email address to allow people to 
                                // be assigned multiple roles upon first login
                                if (existingInviteToSystem == null)
                                {
                                    var prereleaseRole = await _usersAndRolesDbContext
                                        .Roles
                                        // TODO represent Roles with an Enum
                                        .Where(r => r.Name == "Prerelease User")
                                        .FirstAsync();

                                    _usersAndRolesDbContext.Add(new UserInvite
                                    {
                                        Email = email.ToLower(),
                                        Role = prereleaseRole,
                                        Created = UtcNow,
                                        // TODO
                                        CreatedBy = ""
                                    });
                                    await _usersAndRolesDbContext.SaveChangesAsync();
                                }
                            }
                        }

                        await SendPreReleaseInviteEmail(releaseId, email, newUser);
                    }
                })
                .OnSuccess(_ => GetPreReleaseContactsForReleaseAsync(releaseId));
        }


        public async Task<Either<ActionResult, List<PrereleaseCandidateViewModel>>>
            RemovePreReleaseContactFromReleaseAsync(
                Guid releaseId, string email)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanAssignPrereleaseContactsToRelease)
                .OnSuccessDo(async () =>
                {
                    var existingUserRole = await _context
                        .UserReleaseRoles
                        .Include(r => r.User)
                        .Where(r =>
                            r.ReleaseId == releaseId
                            && r.User.Email.ToLower() == email.ToLower()
                            && r.Role == ReleaseRole.PrereleaseViewer)
                        .FirstOrDefaultAsync();

                    if (existingUserRole != null)
                    {
                        _context.Remove(existingUserRole);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        var existingInviteToRelease = await _context
                            .UserReleaseInvites
                            .Where(i =>
                                i.ReleaseId == releaseId
                                && i.Email.ToLower() == email.ToLower()
                                && i.Role == ReleaseRole.PrereleaseViewer)
                            .FirstOrDefaultAsync();

                        if (existingInviteToRelease != null)
                        {
                            // TODO - also need to remove their overall UserInvite record if they have no more 
                            // UserReleaseInvites available
                            _context.Remove(existingInviteToRelease);
                            await _context.SaveChangesAsync();
                        }
                    }
                })
                .OnSuccess(_ => GetPreReleaseContactsForReleaseAsync(releaseId));
        }

        private async Task SendPreReleaseInviteEmail(Guid releaseId, string email, bool newUser)
        {
            var template = _configuration.GetValue<string>("NotifyPreReleaseTemplateId");

            var release = await _context
                .Releases
                .Include(r => r.Publication)
                .FirstAsync(r => r.Id == releaseId);

            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var host = _httpContextAccessor.HttpContext.Request.Host;
            
            var prereleaseUrl = $"{scheme}://{host}/publication/{release.PublicationId}/release/{releaseId}/prerelease";
                
            var emailValues = new Dictionary<string, dynamic>
            {
                {"newUser", newUser ? "yes" : "no"},
                {"release name", release.ReleaseName},
                {"publication name", release.Publication.Title},
                {"prerelease link", prereleaseUrl},
                { "daybeforereleaseday", release.PublishScheduled.Value.AddDays(-1).ToString("dddd dd MMMM yyyy")},
                { "releaseday", release.PublishScheduled.Value.ToString("dddd dd MMMM yyyy")},
            };

            _emailService.SendEmail(email, template, emailValues);
        }
    }

    public class PrereleaseCandidateViewModel
    {
        public string Email { get; set; }

        public bool Invited { get; set; }
    }
}