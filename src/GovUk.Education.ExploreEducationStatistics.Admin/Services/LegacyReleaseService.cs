using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class LegacyReleaseService : ILegacyReleaseService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;

        public LegacyReleaseService(
            ContentDbContext context, 
            IMapper mapper, 
            IUserService userService, 
            IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _persistenceHelper = persistenceHelper;
        }

        public async Task<Either<ActionResult, LegacyReleaseViewModel>> GetLegacyRelease(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<LegacyRelease>(id)
                .OnSuccess(_userService.CheckCanViewLegacyRelease)
                .OnSuccess(release => _mapper.Map<LegacyReleaseViewModel>(release));
        }

        public async Task<Either<ActionResult, LegacyReleaseViewModel>> CreateLegacyRelease(
            CreateLegacyReleaseViewModel legacyRelease)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(
                    legacyRelease.PublicationId, 
                    publication => publication.Include(p => p.LegacyReleases)
                )
                .OnSuccess(_userService.CheckCanCreateLegacyRelease)
                .OnSuccess(async publication =>
                {
                    var saved = await _context.LegacyReleases.AddAsync(new LegacyRelease
                    {
                        Description = legacyRelease.Description,
                        Url = legacyRelease.Url,
                        Order = publication.LegacyReleases.Count > 0
                            ? publication.LegacyReleases.Max(release => release.Order) + 1
                            : 1,
                        PublicationId = legacyRelease.PublicationId
                    });

                    await _context.SaveChangesAsync();
                    
                    return _mapper.Map<LegacyReleaseViewModel>(saved.Entity);
                });
        }

        public async Task<Either<ActionResult, LegacyReleaseViewModel>> UpdateLegacyRelease(
            Guid id,
            UpdateLegacyReleaseViewModel updateLegacyRelease)
        {
            return await _persistenceHelper
                .CheckEntityExists<LegacyRelease>(
                    id, 
                    release => release.Include(r => r.Publication)
                        .ThenInclude(p => p.LegacyReleases)
                )
                .OnSuccess(_userService.CheckCanUpdateLegacyRelease)
                .OnSuccess(async legacyRelease =>
                {
                    legacyRelease.Description = updateLegacyRelease.Description;
                    legacyRelease.Url = updateLegacyRelease.Url;
                    legacyRelease.PublicationId = updateLegacyRelease.PublicationId;

                    var publication = legacyRelease.Publication;

                    if (updateLegacyRelease.Order != legacyRelease.Order)
                    {
                        legacyRelease.Order = updateLegacyRelease.Order;

                        // Shift up orders of existing legacy releases
                        // to make space for updated legacy release.
                        publication.LegacyReleases
                            .FindAll(release => release.Order >= updateLegacyRelease.Order && release.Id != id)
                            .ForEach(release => release.Order++);

                        var currentOrder = 0;

                        // Re-order again to make sure orders are
                        // increased incrementally with no gaps.
                        publication.LegacyReleases
                            .OrderBy(release => release.Order)
                            .ToList()
                            .ForEach(release =>
                            {
                                currentOrder++;
                                release.Order = currentOrder;
                            });
                    }

                    _context.Update(legacyRelease);
                    _context.Update(publication);

                    await _context.SaveChangesAsync();

                    return _mapper.Map<LegacyReleaseViewModel>(legacyRelease);
                });
        }

        public async Task<Either<ActionResult, bool>> DeleteLegacyRelease(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<LegacyRelease>(
                    id,
                    release => release.Include(r => r.Publication)
                        .ThenInclude(p => p.LegacyReleases)
                )
                .OnSuccess(_userService.CheckCanDeleteLegacyRelease)
                .OnSuccess(async legacyRelease =>
                {
                    var previousOrder = legacyRelease.Order;
                    var publication = legacyRelease.Publication;
                    
                    // Shift down the orders of existing legacy releases
                    // to fill the space from removed legacy release.
                    publication.LegacyReleases
                        .FindAll(release => release.Order > previousOrder)
                        .ForEach(release => release.Order--);

                    publication.LegacyReleases.Remove(legacyRelease);

                    _context.Remove(legacyRelease);
                    _context.Update(publication);

                    await _context.SaveChangesAsync();

                    return true;
                });
        }
    }
}
