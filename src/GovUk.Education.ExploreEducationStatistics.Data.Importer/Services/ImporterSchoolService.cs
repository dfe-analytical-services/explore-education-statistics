using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class ImporterSchoolService : BaseImporterService
    {
        public ImporterSchoolService(ImporterMemoryCache cache) : base(cache)
        {
        }

        public School Find(School source, StatisticsDbContext context)
        {
            if (source?.LaEstab == null)
            {
                return null;
            }

            var cacheKey = GetSchoolCacheKey(source.LaEstab);

            if (GetCache().TryGetValue(cacheKey, out School school))
            {
                return school;
            }

            school = LookupOrCreateSchool(source, context);
            GetCache().Set(cacheKey, source);

            return school;
        }

        private School LookupOrCreateSchool(School school, StatisticsDbContext context)
        {
            var cacheKey = GetSchoolCacheKey(school.LaEstab);
            if (GetCache().TryGetValue(cacheKey, out School schoolOut))
            {
                return schoolOut;
            }
            
            school = context.School.AsNoTracking()
                     .FirstOrDefault(s => s.LaEstab == school.LaEstab) ?? context.School.Add(school).Entity;
            
            return school;
        }

        private static string GetSchoolCacheKey(string laEstab)
        {
            return typeof(School).Name + "_" + laEstab;
        }
    }
}