using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class MethodologyService : IMethodologyService
    {
        private readonly IUserService _userService;
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;

        public MethodologyService(ContentDbContext context, IMapper mapper, IUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
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

        public async Task<MethodologyViewModel> GetAsync(Guid id)
        {
            var result = await _context.Methodologies.FirstOrDefaultAsync(m => m.Id == id);
            return _mapper.Map<MethodologyViewModel>(result);
        }

        public async Task<List<MethodologyViewModel>> ListAsync()
        {
            var result = await _context.Methodologies.ToListAsync();
            return _mapper.Map<List<MethodologyViewModel>>(result);
        }

        public async Task<List<MethodologyStatusViewModel>> ListStatusAsync()
        {
            var result = await _context.Methodologies
                .Include(m => m.Publications)
                .OrderBy(m => m.Title)
                .ToListAsync();

            return _mapper.Map<List<MethodologyStatusViewModel>>(result);
        }

        public async Task<List<MethodologyViewModel>> GetTopicMethodologiesAsync(Guid topicId)
        {
            var methodologies = await _context.Publications
                .Where(p => p.TopicId == topicId)
                .Include(p => p.Methodology)
                .Select(p => p.Methodology)
                .Distinct()
                .ToListAsync();
            return _mapper.Map<List<MethodologyViewModel>>(methodologies);
        }

        private async Task<Either<ValidationResult, bool>> ValidateMethodologySlugUnique(string slug)
        {
            if (await _context.Methodologies.AnyAsync(r => r.Slug == slug))
            {
                return new ValidationResult(ValidationErrorMessages.SlugNotUnique.ToString());
            }

            return true;
        }

        private async Task<Either<ValidationResult, bool>> ValidateAssignedPublication(Guid? publicationId)
        {
            if (publicationId != null)
            {
                var publication = await _context.Publications.FirstOrDefaultAsync(p => p.Id == publicationId);

                if (publication == null)
                {
                    return new ValidationResult(ValidationErrorMessages.PublicationDoesNotExist.ToString());
                }
                
                if (publication.MethodologyId != null)
                {
                    return new ValidationResult(ValidationErrorMessages.PublicationHasMethodologyAssigned.ToString());
                }
            }

            return true;
        }
    }
}