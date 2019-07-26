using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.ModelMappers;
using TopicId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class MethodologyService : IMethodologyService
    {
        private readonly ApplicationDbContext _context;

        public MethodologyService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public List<MethodologyViewModel> List()
        {
            return MethodologyViewModelMapper.Map<List<MethodologyViewModel>>(_context.Methodologies);
            
        }
        
        public List<MethodologyViewModel> GetTopicMethodologies(TopicId topicId)
        {
            var methodologies = _context.Publications
                .Where(p => p.TopicId == topicId)
                .Include(p => p.Methodology)
                .Select(p => p.Methodology)
                .Distinct()
                .ToList();
            return MethodologyViewModelMapper.Map<List<MethodologyViewModel>>(methodologies);
        }
    }
}