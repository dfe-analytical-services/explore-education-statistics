#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Configuration;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class LegacyReleaseService : ILegacyReleaseService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IPublicationService _publicationService;
        private readonly IPublicationCacheService _publicationCacheService;
        private readonly IPublicationReleaseOrderService _publicationReleaseOrderService;
        private readonly EnvironmentOptions _options;

        public LegacyReleaseService(ContentDbContext context,
            IMapper mapper,
            IUserService userService,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IPublicationService publicationService,
            IPublicationCacheService publicationCacheService,
            IPublicationReleaseOrderService publicationReleaseOrderService,
            IOptions<EnvironmentOptions> options)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _persistenceHelper = persistenceHelper;
            _publicationService = publicationService;
            _publicationCacheService = publicationCacheService;
            _publicationReleaseOrderService = publicationReleaseOrderService;
            _options = options.Value;
        }

        public async Task<Either<ActionResult, LegacyReleaseViewModel>> GetLegacyRelease(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<LegacyRelease>(id)
                .OnSuccess(_userService.CheckCanViewLegacyRelease)
                .OnSuccess(release => _mapper.Map<LegacyReleaseViewModel>(release));
        }

        public async Task<Either<ActionResult, List<LegacyReleaseViewModel>>> ListLegacyReleases(Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId,
                    q => q.Include(p => p.LegacyReleases))
                .OnSuccess(publication => _userService.CheckCanViewPublication(publication))
                .OnSuccess(publication =>
                {
                    return publication.LegacyReleases
                        .Select(legacyRelease =>
                            new LegacyReleaseViewModel
                            {
                                Id = legacyRelease.Id,
                                Description = legacyRelease.Description,
                                Order = publication.ReleaseOrders.Find(ro => ro.ReleaseId == legacyRelease.Id)?.Order ?? 0,
                                Url = legacyRelease.Url
                            })
                        .OrderByDescending(lr => lr.Order)
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, List<CombinedReleaseViewModel>>> ListCombinedReleases(Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(publication => _userService.CheckCanViewPublication(publication))
                .OnSuccessCombineWith(async _ => await ListLegacyReleases(publicationId))
                .OnSuccessCombineWith(async _ => await _publicationService.ListLatestReleaseVersions(publicationId))
                .OnSuccess(publicationAndReleases =>
                {
                    var (publication, legacyReleases, eesReleases) = publicationAndReleases;
                    var combinedReleasesViewModels = new List<CombinedReleaseViewModel>();

                    foreach (var legacyRelease in legacyReleases)
                    {
                        combinedReleasesViewModels.Add(new()
                        {
                            Description = legacyRelease.Description,
                            Id = legacyRelease.Id,
                            Url = legacyRelease.Url,
                            Order = legacyRelease.Order,
                            IsLegacy = true
                        });
                    }

                    foreach (var eesRelease in eesReleases)
                    {
                        var eesReleaseId = eesRelease.Amendment && eesRelease.PreviousVersionId.HasValue
                            ? eesRelease.PreviousVersionId.Value
                            : eesRelease.Id;

                        combinedReleasesViewModels.Add(new()
                        {
                            Description = eesRelease.Title,
                            Id = eesRelease.Id,
                            Url = $"{_options.BaseUrl}/find-statistics/{publication.Slug}/{eesRelease.Slug}",
                            Order = eesRelease.Order,
                            IsDraft = eesRelease.IsDraft,
                            IsAmendment = eesRelease.Amendment,
                            IsLatest = publication.LatestPublishedReleaseId == eesReleaseId
                        });
                    }

                    return combinedReleasesViewModels
                        .OrderByDescending(cr => cr.Order)
                        .ToList();
                });
        }

        public async Task<Either<ActionResult, LegacyReleaseViewModel>> CreateLegacyRelease(
            LegacyReleaseCreateViewModel legacyRelease)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(
                    legacyRelease.PublicationId,
                    publication => publication.Include(p => p.LegacyReleases)
                )
                .OnSuccess(_userService.CheckCanManageLegacyReleases)
                .OnSuccess(async publication =>
                {
                    var saved = await _context.LegacyReleases.AddAsync(new LegacyRelease
                    {
                        Description = legacyRelease.Description,
                        Url = legacyRelease.Url,
                        PublicationId = legacyRelease.PublicationId
                    });

                    await _publicationReleaseOrderService.CreateForCreateLegacyRelease(
                        publication.Id,
                        saved.Entity.Id);

                    await _context.SaveChangesAsync();

                    await _publicationCacheService.UpdatePublication(publication.Slug);

                    return _mapper.Map<LegacyReleaseViewModel>(saved.Entity);
                });
        }

        public async Task<Either<ActionResult, LegacyReleaseViewModel>> UpdateLegacyRelease(
            Guid id,
            LegacyReleaseUpdateViewModel legacyReleaseUpdate)
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
                    legacyRelease.Description = legacyReleaseUpdate.Description;
                    legacyRelease.Url = legacyReleaseUpdate.Url;
                    legacyRelease.PublicationId = legacyReleaseUpdate.PublicationId;

                    var publication = legacyRelease.Publication;

                    _context.Update(legacyRelease);
                    _context.Update(publication);

                    await _context.SaveChangesAsync();

                    await _publicationCacheService.UpdatePublication(publication.Slug);

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
                    var publication = legacyRelease.Publication;
                    publication.LegacyReleases.Remove(legacyRelease);

                    _context.Remove(legacyRelease);
                    await _publicationReleaseOrderService.DeleteForDeleteLegacyRelease(publication.Id, id);
                    _context.Update(publication);

                    await _context.SaveChangesAsync();

                    await _publicationCacheService.UpdatePublication(publication.Slug);

                    return true;
                });
        }
    }
}
