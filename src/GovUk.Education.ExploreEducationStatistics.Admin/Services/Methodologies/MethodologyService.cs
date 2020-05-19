using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

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

        public async Task<Either<ActionResult, MethodologyTitleViewModel>> CreateMethodologyAsync(
            CreateMethodologyRequest methodology)
        {
            return await _userService
                .CheckCanCreateMethodology()
                .OnSuccess(() => ValidateMethodologySlugUnique(methodology.Slug))
                .OnSuccess(async () =>
                {
                    var model = new Methodology
                    {
                        Title = methodology.Title,
                        Slug = methodology.Slug,
                        PublishScheduled = methodology.PublishScheduled?.AsStartOfDayUtc()
                    };

                    var saved = await _context.Methodologies.AddAsync(model);
                    await _context.SaveChangesAsync();
                    return await GetAsync(saved.Entity.Id);
                });
        }

        private async Task<Either<ActionResult, MethodologyTitleViewModel>> GetAsync(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Methodology>(id)
                .OnSuccess(_userService.CheckCanViewMethodology)
                .OnSuccess(_mapper.Map<MethodologyTitleViewModel>);
        }
        
        public async Task<Either<ActionResult, MethodologyStatusViewModel>> GetStatusAsync(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Methodology>(id)
                .OnSuccess(_userService.CheckCanViewMethodology)
                .OnSuccess(_mapper.Map<MethodologyStatusViewModel>);
        }

        public async Task<Either<ActionResult, MethodologySummaryViewModel>> GetSummaryAsync(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Methodology>(id)
                .OnSuccess(_userService.CheckCanViewMethodology)
                .OnSuccess(_mapper.Map<MethodologySummaryViewModel>);
        }

        public async Task<Either<ActionResult, List<MethodologyStatusViewModel>>> ListAsync()
        {
            return await _userService
                .CheckCanViewAllMethodologies()
                .OnSuccess(async () =>
                {
                    var result = await _context.Methodologies.ToListAsync();
                    return _mapper.Map<List<MethodologyStatusViewModel>>(result);
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

        public async Task<Either<ActionResult, MethodologyStatusViewModel>> UpdateMethodologyStatusAsync(Guid methodologyId,
            UpdateMethodologyStatusRequest request)
        {
            return await _persistenceHelper
                .CheckEntityExists<Methodology>(methodologyId)
                .OnSuccess(methodology => _userService.CheckCanUpdateMethodologyStatus(methodology, request.Status))
                .OnSuccess(async methodology =>
                {
                    methodology.Status = request.Status;
                    methodology.InternalReleaseNote = request.InternalReleaseNote;
                    
                    _context.Methodologies.Update(methodology);
                    await _context.SaveChangesAsync();

                    return await GetStatusAsync(methodologyId);
                });
        }

        private async Task<Either<ActionResult, bool>> ValidateMethodologySlugUnique(string slug)
        {
            if (await _context.Methodologies.AnyAsync(r => r.Slug == slug))
            {
                return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
            }

            return true;
        }
    }
}