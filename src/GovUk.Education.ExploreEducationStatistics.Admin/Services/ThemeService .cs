using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ThemeService : IThemeService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IThemeRepository _repository;

        public ThemeService(IUserService userService, IThemeRepository repository, 
            IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _userService = userService;
            _repository = repository;
            _persistenceHelper = persistenceHelper;
        }

        public async Task<Either<ActionResult, List<Theme>>> GetMyThemesAsync()
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

        public async Task<Either<ActionResult, TitleAndIdViewModel>> GetSummaryAsync(Guid id)
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