using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using TopicId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class MethodologyService : IMethodologyService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;

        public MethodologyService(ContentDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Either<ValidationResult, MethodologyViewModel>> CreateMethodologyAsync(
            CreateMethodologyViewModel methodology)
        {
            var model = new Methodology
            {
                Title = methodology.Title,
                Slug = methodology.Slug,
                // PublishScheduled = methodology.PublishScheduled
            };

            var saved = _context.Methodologies.Add(model);
            await _context.SaveChangesAsync();

            return await GetAsync(saved.Entity.Id);
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
                .Include(m => m.Publications.OrderBy(p => p.Title))
                .OrderBy(m => m.Title)
                .ToListAsync();

            return _mapper.Map<List<MethodologyStatusViewModel>>(result);
        }

        public async Task<List<MethodologyViewModel>> GetTopicMethodologiesAsync(TopicId topicId)
        {
            var methodologies = await _context.Publications
                .Where(p => p.TopicId == topicId)
                .Include(p => p.Methodology)
                .Select(p => p.Methodology)
                .Distinct()
                .ToListAsync();
            return _mapper.Map<List<MethodologyViewModel>>(methodologies);
        }
    }
}