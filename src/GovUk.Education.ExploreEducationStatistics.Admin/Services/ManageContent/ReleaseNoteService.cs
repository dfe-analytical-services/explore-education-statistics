using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class ReleaseNoteService : IReleaseNoteService
    {
        private readonly IMapper _mapper;
        private readonly ContentDbContext _context;
        private readonly PersistenceHelper<Release, Guid> _releaseHelper;

        public ReleaseNoteService(IMapper mapper, ContentDbContext context)
        {
            _mapper = mapper;
            _context = context;
            _releaseHelper = new PersistenceHelper<Release, Guid>(
                _context, 
                context.Releases, 
                ValidationErrorMessages.ReleaseNotFound);
        }

        public Task<Either<ValidationResult, List<ReleaseNoteViewModel>>> AddReleaseNoteAsync(Guid releaseId, CreateReleaseNoteRequest request)
        {
            return _releaseHelper.CheckEntityExists(releaseId, async release =>
            {
                if (release.Updates == null)
                {
                    release.Updates = new List<Update>();
                }
                
                release.Updates.Add(new Update
                {
                    On = DateTime.Now,
                    Reason = request.ReleaseNote,
                });

                _context.Releases.Update(release);
                await _context.SaveChangesAsync();
                return _mapper.Map<List<ReleaseNoteViewModel>>(release.Updates.OrderBy(update => update.On));
            }, HydrateRelease);
        }
        
        private static IQueryable<Release> HydrateRelease(IQueryable<Release> values)
        {
            return values.Include(r => r.Updates);
        }
    }
}