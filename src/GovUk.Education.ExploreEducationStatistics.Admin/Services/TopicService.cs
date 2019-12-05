using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class TopicService : ITopicService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;

        public TopicService(ContentDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Topic> GetTopicAsync(Guid topicId)
        {
            var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Id == topicId);

            return topic;
        }
        
        public async Task<Either<ValidationResult, TopicViewModel>> CreateTopicAsync(Guid themeId, CreateTopicRequest topic)
        {
            if (_context.Topics.Any(t => t.Slug == topic.Slug))
            {
                return ValidationResult(SlugNotUnique);
            }
            
            var saved = _context.Topics.Add(new Topic
            {
                Id = Guid.NewGuid(),
                Title = topic.Title,
                Slug = topic.Slug,
                Description = topic.Description,
                ThemeId = themeId,
                Summary = topic.Summary,
            });
            await _context.SaveChangesAsync();
            return await GetViewModelAsync(saved.Entity.Id);
        }

        public async Task<TopicViewModel> GetViewModelAsync(Guid topicId)
        {
            var topic = await _context.Topics
                .Where(p => p.Id == topicId)
                .HydrateTopicForTopicViewModel()
                .FirstOrDefaultAsync();
            return _mapper.Map<TopicViewModel>(topic);
        }
    }

    public static class TopicLinqExtensions
    {
        public static IQueryable<Topic> HydrateTopicForTopicViewModel(this
        IQueryable<Topic> values)
        {
            return values.Include(p => p.Publications);
        }
    }
}