using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class MethodologyService : IMethodologyService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPublishingService _publishingService;
        private readonly IUserService _userService;

        public MethodologyService(ContentDbContext context,
            IMapper mapper,
            IPublishingService publishingService,
            IUserService userService,
            IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _context = context;
            _mapper = mapper;
            _publishingService = publishingService;
            _userService = userService;
            _persistenceHelper = persistenceHelper;
        }

        public async Task<Either<ActionResult, MethodologySummaryViewModel>> CreateMethodologyAsync(
            MethodologyCreateRequest request)
        {
            var slug = SlugFromTitle(request.Title);
            return await _userService.CheckCanCreateMethodology()
                .OnSuccess(() => ValidateMethodologySlugUnique(slug))
                .OnSuccess(async () =>
                {
                    var model = new Methodology
                    {
                        Title = request.Title,
                        Slug = slug
                    };

                    var saved = await _context.Methodologies.AddAsync(model);
                    await _context.SaveChangesAsync();
                    return await GetSummaryAsync(saved.Entity.Id);
                });
        }

        public async Task<Either<ActionResult, MethodologySummaryViewModel>> GetSummaryAsync(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Methodology>(id)
                .OnSuccess(_userService.CheckCanViewMethodology)
                .OnSuccess(_mapper.Map<MethodologySummaryViewModel>);
        }

        public async Task<Either<ActionResult, List<MethodologySummaryViewModel>>> ListAsync()
        {
            return await _userService.CheckCanViewAllMethodologies()
                .OnSuccess(async () =>
                {
                    var result = await _context.Methodologies.ToListAsync();
                    return _mapper.Map<List<MethodologySummaryViewModel>>(result);
                });
        }

        public async Task<Either<ActionResult, List<MethodologyPublicationsViewModel>>> ListWithPublicationsAsync()
        {
            return await _userService
                .CheckCanViewAllMethodologies()
                .OnSuccess(async () =>
                {
                    var result = await _context
                        .Methodologies
                        .Include(m => m.Publications)
                        .OrderBy(m => m.Title)
                        .ToListAsync();

                    return _mapper.Map<List<MethodologyPublicationsViewModel>>(result);
                })
                .OrElse(async () =>
                {
                    var userId = _userService.GetUserId();
                    var viewablePublications = await _context.UserReleaseRoles
                        .Include(urr => urr.Release)
                        .ThenInclude(r => r.Publication)
                        .Where(urr =>
                            urr.UserId == userId
                            && urr.Role != ReleaseRole.PrereleaseViewer)
                        .Select(urr => urr.Release.Publication)
                        .Distinct()
                        .ToListAsync();

                    var allMethodologies = await _context.Methodologies
                        .Include(m => m.Publications)
                        .ToListAsync();

                    var viewableMethodologies = allMethodologies
                        .Where(m => viewablePublications
                            .Any(p1 => m.Publications
                                .Select(p2 => p2.Id)
                                .Contains(p1.Id)))
                        .OrderBy(m => m.Title)
                        .ToList();

                    return _mapper.Map<List<MethodologyPublicationsViewModel>>(viewableMethodologies);
                });
        }

        public async Task<Either<ActionResult, MethodologySummaryViewModel>> UpdateMethodologyAsync(Guid id,
            MethodologyUpdateRequest request)
        {
            return await _persistenceHelper.CheckEntityExists<Methodology>(id)
                .OnSuccess(methodology => CheckCanUpdateMethodologyStatus(methodology, request.Status))
                .OnSuccess(_userService.CheckCanUpdateMethodology)
                .OnSuccess(async methodology =>
                {
                    if (methodology.Live)
                    {
                        // Leave slug
                        return methodology;
                    }
                    var slug = SlugFromTitle(request.Title);
                    return (await ValidateMethodologySlugUniqueForUpdate(methodology.Id, slug)).Map(_ =>
                    {
                        methodology.Slug = slug;
                        return methodology;
                    });
                })
                .OnSuccess(async methodology =>
                {
                    methodology.InternalReleaseNote = request.InternalReleaseNote ?? methodology.InternalReleaseNote;
                    methodology.Status = request.Status;
                    methodology.Title = request.Title;
                    methodology.Updated = DateTime.UtcNow;

                    _context.Methodologies.Update(methodology);
                    await _context.SaveChangesAsync();

                    if (methodology.Status == MethodologyStatus.Approved && methodology.Live)
                    {
                        await _publishingService.MethodologyChanged(methodology.Id);
                    }

                    return await GetSummaryAsync(id);
                });
        }

        private Task<Either<ActionResult, Methodology>> CheckCanUpdateMethodologyStatus(Methodology methodology,
            MethodologyStatus status)
        {
            if (methodology.Status == status)
            {
                // Status unchanged
                return Task.FromResult(new Either<ActionResult, Methodology>(methodology));
            }

            return status switch
            {
                MethodologyStatus.Draft => _userService.CheckCanMarkMethodologyAsDraft(methodology),
                MethodologyStatus.Approved => _userService.CheckCanApproveMethodology(methodology),
                _ => throw new ArgumentOutOfRangeException(nameof(status), "Unexpected status")
            };
        }

        private async Task<Either<ActionResult, Unit>> ValidateMethodologySlugUnique(string slug)
        {
            if (await _context.Methodologies.AnyAsync(methodology => methodology.Slug == slug))
            {
                return ValidationActionResult(SlugNotUnique);
            }

            return Unit.Instance;
        }

        private async Task<Either<ActionResult, Unit>> ValidateMethodologySlugUniqueForUpdate(Guid id, string slug)
        {
            if (await _context.Methodologies.AnyAsync(methodology => methodology.Slug == slug && methodology.Id != id))
            {
                return ValidationActionResult(SlugNotUnique);
            }

            return Unit.Instance;
        }
    }
}
