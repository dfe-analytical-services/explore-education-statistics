using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class ImporterSchoolService : BaseImporterService
    {
        private readonly ApplicationDbContext _context;

        public ImporterSchoolService(ImporterMemoryCache cache, ApplicationDbContext context) : base(cache)
        {
            _context = context;
        }

        public School Find(School source)
        {
            if (source == null)
            {
                return null;
            }

            var cacheKey = GetSchoolCacheKey(source.LaEstab);

            if (GetCache().TryGetValue(cacheKey, out School school))
            {
                return school;
            }

            school = LookupOrCreateSchool(source);
            GetCache().Set(cacheKey, source);

            return school;
        }

        private School LookupOrCreateSchool(School school)
        {
            var cacheKey = GetSchoolCacheKey(school.LaEstab);
            if (GetCache().TryGetValue(cacheKey, out School schoolOut))
            {
                return schoolOut;
            }
            
            school = _context.School
                     .FirstOrDefault(s => s.LaEstab == school.LaEstab) ?? _context.School.Add(school).Entity;
      
            GetCache().Set(cacheKey, school);
            
            return school;
        }

        private static string GetSchoolCacheKey(string laEstab)
        {
            return typeof(School).Name + "_" + laEstab;
        }
    }
}