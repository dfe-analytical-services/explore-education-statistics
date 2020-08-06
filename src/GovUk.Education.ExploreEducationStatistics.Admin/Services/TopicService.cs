using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    /**
     * This service is currently used by the test scripts for the purposes of creating data
     */
    public class TopicService : ITopicService
    {
        private readonly ContentDbContext _context;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public TopicService(ContentDbContext context,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IMapper mapper,
            IUserService userService)
        {
            _context = context;
            _persistenceHelper = persistenceHelper;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<Either<ActionResult, TopicViewModel>> CreateTopic(Guid themeId,
            CreateTopicRequest request)
        {
            return await _persistenceHelper
                .CheckEntityExists<Theme>(themeId)
                .OnSuccess(_userService.CheckCanManageAllTaxonomy)
                .OnSuccess(async _ =>
                {
                    if (_context.Topics.Any(topic => topic.Slug == request.Slug))
                    {
                        return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
                    }

                    var saved = await _context.Topics.AddAsync(new Topic
                    {
                        Title = request.Title,
                        Slug = request.Slug,
                        Description = request.Description,
                        ThemeId = themeId,
                        Summary = request.Summary
                    });
                    await _context.SaveChangesAsync();
                    return await GetTopic(saved.Entity.Id);
                });
        }

        public async Task<Either<ActionResult, TopicViewModel>> GetTopic(Guid topicId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Topic>(topicId, HydrateTopicForTopicViewModel)
                .OnSuccess(_userService.CheckCanViewTopic)
                .OnSuccess(_mapper.Map<TopicViewModel>);   
        }

        private static IQueryable<Topic> HydrateTopicForTopicViewModel(IQueryable<Topic> values)
        {
            return values.Include(p => p.Publications);
        }
    }
}