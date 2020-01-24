using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class PreReleaseService : IPreReleaseService
    {
        private readonly ContentDbContext _context;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        
        public PreReleaseService(ContentDbContext context, IUserService userService, 
            IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _context = context;
            _userService = userService;
            _persistenceHelper = persistenceHelper;
        }

        public async Task<Either<ActionResult, List<PrereleaseCandidateViewModel>>> GetAvailablePreReleaseContactsAsync()
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
        
        public async Task<Either<ActionResult, List<PrereleaseCandidateViewModel>>> GetPreReleaseContactsForReleaseAsync(Guid releaseId)
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
                                _context.Add(new UserReleaseInvite
                                {
                                    Email = email.ToLower(),
                                    ReleaseId = releaseId,
                                    Role = ReleaseRole.PrereleaseViewer,
                                    CreatedById = _userService.GetUserId()
                                });
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                })
                .OnSuccess(_ => GetPreReleaseContactsForReleaseAsync(releaseId));
        }
        
        public async Task<Either<ActionResult, List<PrereleaseCandidateViewModel>>> RemovePreReleaseContactFromReleaseAsync(
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
                        var existingInvite = await _context
                            .UserReleaseInvites
                            .Where(i => 
                                i.ReleaseId == releaseId 
                                    && i.Email.ToLower() == email.ToLower() 
                                    && i.Role == ReleaseRole.PrereleaseViewer)
                            .FirstOrDefaultAsync();

                        if (existingInvite != null)
                        {
                            _context.Remove(existingInvite);
                            await _context.SaveChangesAsync();
                        }
                    }
                })
                .OnSuccess(_ => GetPreReleaseContactsForReleaseAsync(releaseId));
        }
    }

    public class PrereleaseCandidateViewModel
    {
        public string Email { get; set; }
            
        public bool Invited { get; set; }
    }
}