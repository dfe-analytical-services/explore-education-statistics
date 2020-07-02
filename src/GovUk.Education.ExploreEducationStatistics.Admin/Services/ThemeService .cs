using System;
using System.Collections.Generic;
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
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ThemeService : IThemeService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IThemeRepository _repository;

        public ThemeService(
            ContentDbContext context,
            IMapper mapper,
            IUserService userService,
            IThemeRepository repository,
            IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _repository = repository;
            _persistenceHelper = persistenceHelper;
        }

        public async Task<Either<ActionResult, ThemeViewModel>> CreateTheme(CreateThemeRequest request)
        {
            return await _userService.CheckCanManageAllTaxonomy()
                .OnSuccess(async _ =>
                {
                    if (_context.Themes.Any(theme => theme.Slug == request.Slug))
                    {
                        return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
                    }

                    var saved = await _context.Themes.AddAsync(new Theme
                    {
                        Slug = request.Slug,
                        Summary = request.Summary,
                        Title = request.Title,
                    });

                    await _context.SaveChangesAsync();
                    return await Get(saved.Entity.Id);
                });
        }

        public async Task<Either<ActionResult, List<Theme>>> GetMyThemes()
        {
            return await _userService
                .CheckCanAccessSystem()
                .OnSuccess(_ =>
                {
                    return _userService
                        .CheckCanViewAllTopics()
                        .OnSuccess(() => _repository.GetAllThemesAsync())
                        .OrElse(() => _repository.GetThemesRelatedToUserAsync(_userService.GetUserId()));
                });
        }

        public async Task<Either<ActionResult, ThemeViewModel>> Get(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Theme>(id)
                .OnSuccess(_userService.CheckCanViewTheme)
                .OnSuccess(_mapper.Map<ThemeViewModel>);
        }

        public async Task<Either<ActionResult, TitleAndIdViewModel>> GetSummary(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Theme>(id)
                .OnSuccess(_userService.CheckCanViewTheme)
                .OnSuccess(theme => new TitleAndIdViewModel
                {
                    Id = theme.Id,
                    Title = theme.Title
                });
        }
    }
}