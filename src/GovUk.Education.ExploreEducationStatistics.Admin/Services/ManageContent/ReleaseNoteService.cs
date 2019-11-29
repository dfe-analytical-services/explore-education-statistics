using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class ReleaseNoteService : IReleaseNoteService
    {
        private readonly ContentDbContext _context;
        private readonly PersistenceHelper<Release, Guid> _releaseHelper;

        public ReleaseNoteService(ContentDbContext context)
        {
            _context = context;
            _releaseHelper = new PersistenceHelper<Release, Guid>(
                _context, 
                context.Releases, 
                ValidationErrorMessages.ReleaseNotFound);
        }

        public Task<Either<ValidationResult, List<Update>>> AddReleaseNoteAsync(Guid releaseId, CreateReleaseNoteRequest request)
        {
            return _releaseHelper.CheckEntityExists(releaseId, async release =>
            {
                if (release.Updates == null)
                {
                    release.Updates = new List<Update>();
                }
                
                release.Updates.Add(new Update
                {
                    Id = Guid.NewGuid(),
                    On = DateTime.Now,
                    Reason = request.ReleaseNote,
                    ReleaseId = release.Id
                });

                _context.Releases.Update(release);
                await _context.SaveChangesAsync();
                return release.Updates;
            }, HydrateRelease);
        }
        
        private static IQueryable<Release> HydrateRelease(IQueryable<Release> values)
        {
            return values.Include(r => r.Updates);
        }
    }
}