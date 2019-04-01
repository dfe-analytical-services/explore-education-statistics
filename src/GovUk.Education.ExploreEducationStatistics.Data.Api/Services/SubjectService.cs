using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class SubjectService : AbstractDataService<Subject>, ISubjectService
    {
        private readonly IReleaseService _releaseService;
        private readonly IMapper _mapper;

        public SubjectService(ApplicationDbContext context,
            ILogger<SubjectService> logger,
            IReleaseService releaseService,
            IMapper mapper) : base(context, logger)
        {
            _releaseService = releaseService;
            _mapper = mapper;
        }

        public IEnumerable<SubjectMetaViewModel> GetSubjectMetas(Guid publicationId)
        {
            var releaseId = _releaseService.GetLatestRelease(publicationId);

            return FindMany(subject => subject.Release.Id == releaseId)
                .Select(subject => _mapper.Map<SubjectMetaViewModel>(subject));
        }

        public Dictionary<string, IEnumerable<IndicatorMetaViewModel>> GetIndicatorMetas(Subject subject)
        {
            return DbSet().Where(s => s.Id == subject.Id)
                .SelectMany(s => s.Indicators)
                .GroupBy(indicatorMeta => indicatorMeta.Group)
                .ToDictionary(
                    metas => metas.Key,
                    metas => metas.Select(ToIndicatorMetaViewModel));
        }

        public Dictionary<string, IEnumerable<CharacteristicMetaViewModel>> GetCharacteristicMetas(Subject subject)
        {
            return DbSet().Where(s => s.Id == subject.Id)
                .SelectMany(s => s.Characteristics)
                .GroupBy(characteristicMeta => characteristicMeta.Group)
                .ToDictionary(
                    metas => metas.Key,
                    metas => metas.Select(ToCharacteristicMetaViewModel));
        }

        private IndicatorMetaViewModel ToIndicatorMetaViewModel(IndicatorMeta indicatorMeta)
        {
            return _mapper.Map<IndicatorMetaViewModel>(indicatorMeta);
        }

        private CharacteristicMetaViewModel ToCharacteristicMetaViewModel(CharacteristicMeta characteristicMeta)
        {
            return _mapper.Map<CharacteristicMetaViewModel>(characteristicMeta);
        }
    }
}