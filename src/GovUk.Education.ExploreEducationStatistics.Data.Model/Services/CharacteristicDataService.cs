using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class CharacteristicDataService : AbstractDataService<CharacteristicData, long>, ICharacteristicDataService
    {
        private readonly ILevelService _levelService;

        public CharacteristicDataService(ApplicationDbContext context,
            ILevelService levelService,
            ILogger<CharacteristicDataService> logger) :
            base(context, logger)
        {
            _levelService = levelService;
        }

        public LevelMeta GetLevelMeta(long subjectId)
        {
            var levels = GetLevels(subjectId);
            return new LevelMeta
            {
                Country = levels.GroupBy(composite => composite.Country).Select(group => group.Key),
                LocalAuthority = levels.GroupBy(composite => composite.LocalAuthority).Select(group => group.Key),
                Region = levels.GroupBy(composite => composite.Region).Select(group => group.Key)
            };
        }

        private IEnumerable<LevelComposite> GetLevels(long subjectId)
        {
//            return (from l in _context.Set<LevelComposite>()
//                join
//                    d in _context.GeographicData.Where(data => data.SubjectId == subjectId)
//                        .GroupBy(data => data.LevelId) on l.Id equals d.Key
//                select l).ToList();

            var levelIds = DbSet().Where(data => data.SubjectId == subjectId).GroupBy(data => data.LevelId)
                .Select(group => group.Key);

            return levelIds.Any() ? _levelService.Find(levelIds.ToArray()) : new List<LevelComposite>();
        }
    }
}