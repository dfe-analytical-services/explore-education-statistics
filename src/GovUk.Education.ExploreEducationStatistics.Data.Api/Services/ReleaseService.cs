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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class ReleaseService : AbstractDataService<Release>, IReleaseService
    {
        private IMapper _mapper;

        public ReleaseService(ApplicationDbContext context, ILogger<ReleaseService> logger, IMapper mapper) :
            base(context, logger)
        {
            _mapper = mapper;
        }

        public long GetLatestRelease(Guid publicationId)
        {
            return TopWithPredicate(data => data.Id, data => data.PublicationId == publicationId);
        }

        public Dictionary<string, List<IndicatorMetaViewModel>> GetIndicatorMetas(Guid publicationId, string typeName)
        {
            var releaseId = GetLatestRelease(publicationId);

            var release = DbSet().Where(r => r.Id == releaseId)
                .Include(r => r.ReleaseIndicatorMetas)
                .ThenInclude(meta => meta.IndicatorMeta).First();

            return release.ReleaseIndicatorMetas
                .Where(meta => meta.DataType == typeName)
                .GroupBy(meta => meta.Group)
                .ToDictionary(
                    metas => metas.Key,
                    metas => metas.Select(meta => meta.IndicatorMeta).Select(ToIndicatorMetaViewModel).ToList());
        }

        public Dictionary<string, List<NameLabelViewModel>> GetCharacteristicMetas(Guid publicationId, string typeName)
        {
            var releaseId = GetLatestRelease(publicationId);

            var release = DbSet().Where(r => r.Id == releaseId)
                .Include(r => r.ReleaseCharacteristicMetas)
                .ThenInclude(meta => meta.CharacteristicMeta).First();

            return release.ReleaseCharacteristicMetas
                .Where(meta => meta.DataType == typeName)
                .Select(meta => meta.CharacteristicMeta)
                .GroupBy(o => o.Group)
                .ToDictionary(
                    metas => metas.Key,
                    metas => metas.Select(ToNameLabelViewModel).ToList());
        }

        private IndicatorMetaViewModel ToIndicatorMetaViewModel(IndicatorMeta indicatorMeta)
        {
            return _mapper.Map<IndicatorMetaViewModel>(indicatorMeta);
        }

        private NameLabelViewModel ToNameLabelViewModel(CharacteristicMeta characteristicMeta)
        {
            return _mapper.Map<NameLabelViewModel>(characteristicMeta);
        }
    }
}