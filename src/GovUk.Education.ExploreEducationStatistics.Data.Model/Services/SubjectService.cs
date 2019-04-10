using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class SubjectService : AbstractDataService<Subject, long>, ISubjectService
    {
        public SubjectService(ApplicationDbContext context,
            ILogger<SubjectService> logger) : base(context, logger)
        {
        }

        public Dictionary<string, IEnumerable<IndicatorMeta>> GetIndicatorMetaGroups(long subjectId)
        {
            return DbSet().Where(s => s.Id == subjectId)
                .SelectMany(s => s.Indicators)
                .GroupBy(indicatorMeta => indicatorMeta.Group)
                .ToDictionary(
                    metas => metas.Key,
                    metas => metas.AsEnumerable());
        }

        public Dictionary<string, IEnumerable<CharacteristicMeta>> GetCharacteristicMetaGroups(long subjectId)
        {
            return DbSet().Where(s => s.Id == subjectId)
                .SelectMany(s => s.Characteristics)
                .GroupBy(characteristicMeta => characteristicMeta.Group)
                .ToDictionary(
                    metas => metas.Key,
                    metas => metas.AsEnumerable());
        }
    }
}