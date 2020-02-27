using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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

        public MethodologyService(ContentDbContext context, IMapper mapper, IUserService userService, IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _persistenceHelper = persistenceHelper;
        }

        public async Task<Either<ActionResult, MethodologyViewModel>> CreateMethodologyAsync(
            CreateMethodologyViewModel methodology)
        {
            return await _userService
                .CheckCanCreateMethodology()
                .OnSuccess(() => ValidateMethodologySlugUnique(methodology.Slug))
                .OnSuccess(() => ValidateAssignedPublication(methodology.PublicationId))
                .OnSuccess(async () =>
                {
                    var model = new Methodology
                    {
                        Title = methodology.Title,
                        Slug = methodology.Slug,
                        PublishScheduled = methodology.PublishScheduled
                    };

                    var saved = _context.Methodologies.Add(model);
                    await _context.SaveChangesAsync();

                    if (methodology.PublicationId != null)
                    {
                        var publication =
                            await _context.Publications.FirstOrDefaultAsync(p => p.Id == methodology.PublicationId);

                        publication.MethodologyId = saved.Entity.Id;
                        _context.Publications.Update(publication);
                        await _context.SaveChangesAsync();
                    }

                    return await GetAsync(saved.Entity.Id);
                });
        }

        public async Task<Either<ActionResult, MethodologyViewModel>> GetAsync(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Methodology>(id)
                .OnSuccess(_userService.CheckCanViewMethodology)
                .OnSuccess(_mapper.Map<MethodologyViewModel>);
        }

        public async Task<Either<ActionResult, List<MethodologyViewModel>>> ListAsync()
        {
            return await _userService
                .CheckCanViewAllMethodologies()
                .OnSuccess(async () =>
                {
                    var result = await _context.Methodologies.ToListAsync();
                    return _mapper.Map<List<MethodologyViewModel>>(result);
                });
        }

        public async Task<Either<ActionResult, List<MethodologyStatusViewModel>>> ListStatusAsync()
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

                    return _mapper.Map<List<MethodologyStatusViewModel>>(result);
                });
        }

        public async Task<Either<ActionResult, List<MethodologyViewModel>>> GetTopicMethodologiesAsync(Guid topicId)
        {
            return await _userService
                .CheckCanViewAllMethodologies()
                .OnSuccess(async () =>
                {
                    var methodologies = await _context.Publications
                        .Where(p => p.TopicId == topicId)
                        .Include(p => p.Methodology)
                        .Select(p => p.Methodology)
                        .Distinct()
                        .ToListAsync();
                    
                    return _mapper.Map<List<MethodologyViewModel>>(methodologies);
                });
        }

        public async Task<Either<ActionResult, MethodologyViewModel>> UpdateMethodologyStatusAsync(Guid methodologyId,
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

                    return await GetAsync(methodologyId);
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

        private async Task<Either<ActionResult, bool>> ValidateAssignedPublication(Guid? publicationId)
        {
            if (publicationId != null)
            {
                var publication = await _context.Publications.FirstOrDefaultAsync(p => p.Id == publicationId);

                if (publication == null)
                {
                    return ValidationActionResult(ValidationErrorMessages.PublicationDoesNotExist);
                }
                
                if (publication.MethodologyId != null)
                {
                    return ValidationActionResult(ValidationErrorMessages.PublicationHasMethodologyAssigned);
                }
            }

            return true;
        }
    }
}