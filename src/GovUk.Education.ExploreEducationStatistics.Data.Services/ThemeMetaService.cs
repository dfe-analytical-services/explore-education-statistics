using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class ThemeMetaService : IThemeMetaService
    {
        private readonly IReleaseService _releaseService;
        private readonly IMapper _mapper;

        public ThemeMetaService(
            IReleaseService releaseService,
            IMapper mapper)
        {
            _releaseService = releaseService;
            _mapper = mapper;
        }

        public IEnumerable<ThemeMetaViewModel> GetThemes()
        {
            return _mapper.Map<IEnumerable<ThemeMetaViewModel>>(_releaseService.All(
                    new List<Expression<Func<Release, object>>>
                    {
                        release => release.Publication.Topic.Theme
                    })
                .AsEnumerable()
                .GroupBy(release => release.Publication.Topic.Theme)
                .Select(grouping => grouping.Key)
            );
        }
    }
}