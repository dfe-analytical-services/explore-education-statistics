using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
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

        public Dictionary<string, List<AttributeMetaViewModel>> GetAttributeMetas(Guid publicationId, Type type)
        {
            var releaseId = GetLatestRelease(publicationId);

            var release = DbSet().Where(r => r.Id == releaseId)
                .Include(r => r.ReleaseAttributeMetas)
                .ThenInclude(meta => meta.AttributeMeta).First();

            return release.ReleaseAttributeMetas
                .Where(meta => meta.DataType == type.Name)
                .GroupBy(meta => meta.Group)
                .ToDictionary(
                    metas => metas.Key,
                    metas => metas.Select(meta => meta.AttributeMeta).Select(ToAttributeMetaViewModel).ToList());
        }

        public Dictionary<string, List<NameLabelViewModel>> GetCharacteristicMetas(Guid publicationId, Type type)
        {
            var releaseId = GetLatestRelease(publicationId);

            var release = DbSet().Where(r => r.Id == releaseId)
                .Include(r => r.ReleaseCharacteristicMetas)
                .ThenInclude(meta => meta.CharacteristicMeta).First();

            return release.ReleaseCharacteristicMetas
                .Where(meta => meta.DataType == type.Name)
                .Select(meta => meta.CharacteristicMeta)
                .GroupBy(o => o.Group)
                .ToDictionary(
                    metas => metas.Key,
                    metas => metas.Select(ToNameLabelViewModel).ToList());
        }

        private AttributeMetaViewModel ToAttributeMetaViewModel(AttributeMeta attributeMeta)
        {
            return _mapper.Map<AttributeMetaViewModel>(attributeMeta);
        }

        private NameLabelViewModel ToNameLabelViewModel(CharacteristicMeta characteristicMeta)
        {
            return _mapper.Map<NameLabelViewModel>(characteristicMeta);
        }
    }
}