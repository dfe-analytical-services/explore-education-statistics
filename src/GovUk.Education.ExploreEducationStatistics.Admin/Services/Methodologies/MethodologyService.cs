using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
        private readonly IUserService _userService;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;

        public MethodologyService(ContentDbContext context,
            IMapper mapper,
            IUserService userService,
            IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _persistenceHelper = persistenceHelper;
        }

        public async Task<Either<ActionResult, MethodologySummaryViewModel>> CreateMethodologyAsync(
            CreateMethodologyRequest request)
        {
            var slug = SlugFromTitle(request.Title);
            return await _userService.CheckCanCreateMethodology()
                .OnSuccess(() => ValidateMethodologySlugUnique(slug))
                .OnSuccess(async () =>
                {
                    var model = new Methodology
                    {
                        Title = request.Title,
                        Slug = slug,
                        PublishScheduled = request.PublishScheduled?.AsStartOfDayUtc()
                    };

                    var saved = await _context.Methodologies.AddAsync(model);
                    await _context.SaveChangesAsync();
                    return await GetSummaryAsync(saved.Entity.Id);
                });
        }

        public async Task<Either<ActionResult, MethodologySummaryViewModel>> GetSummaryAsync(Guid id)
        {
            return await _persistenceHelper.CheckEntityExists<Methodology>(id)
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
                });
        }

        public async Task<Either<ActionResult, MethodologySummaryViewModel>> UpdateMethodologyAsync(Guid id,
            UpdateMethodologyRequest request)
        {
            var slug = SlugFromTitle(request.Title);
            return await _persistenceHelper.CheckEntityExists<Methodology>(id)
                .OnSuccess(_userService.CheckCanUpdateMethodology)
                .OnSuccess(methodology => CheckCanUpdateMethodologyStatus(methodology, request.Status))
                .OnSuccessDo(() => ValidateMethodologySlugUniqueForUpdate(id, slug))
                .OnSuccess(async methodology =>
                {
                    _context.Methodologies.Update(methodology);
                    methodology.InternalReleaseNote = request.InternalReleaseNote ?? methodology.InternalReleaseNote;
                    methodology.PublishScheduled = request.PublishScheduled?.AsStartOfDayUtc();
                    methodology.Status = request.Status ?? methodology.Status;
                    methodology.Title = request.Title;
                    methodology.Slug = slug;

                    await _context.SaveChangesAsync();
                    return await GetSummaryAsync(id);
                });
        }

        private Task<Either<ActionResult, Methodology>> CheckCanUpdateMethodologyStatus(Methodology methodology,
            MethodologyStatus? status)
        {
            if (!status.HasValue || methodology.Status == status.Value)
            {
                // Status unchanged
                return Task.FromResult(new Either<ActionResult, Methodology>(methodology));
            }

            return _userService.CheckCanUpdateMethodologyStatus(methodology, status.Value);
        }

        private async Task<Either<ActionResult, bool>> ValidateMethodologySlugUnique(string slug)
        {
            if (await _context.Methodologies.AnyAsync(methodology => methodology.Slug == slug))
            {
                return ValidationActionResult(SlugNotUnique);
            }

            return true;
        }

        private async Task<Either<ActionResult, bool>> ValidateMethodologySlugUniqueForUpdate(Guid id, string slug)
        {
            if (await _context.Methodologies.AnyAsync(methodology => methodology.Slug == slug && methodology.Id != id))
            {
                return ValidationActionResult(SlugNotUnique);
            }

            return true;
        }
    }
}
