using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class GeographicImporter : Importer
    {
        public GeographicImporter(IMemoryCache cache, ApplicationDbContext context, ILogger<GeographicImporter> logger) :
            base(cache, context, logger)
        {
        }

        protected override TidyData TidyDataFromCsv(string csvLine, List<string> headers, Subject subject)
        {
            var headerValues = new[]
            {
                "time_period", "time_identifier", "level", "country_code", "country_name", "region_code", "region_name",
                "old_la_code", "new_la_code", "la_name", "estab", "laestab", "urn", "academy_type", "academy_open_date",
                "school_type"
            };
            var values = csvLine.Split(',');
            
            var model = new GeographicData
            {
                Subject = subject,
                TimePeriod = int.Parse(values[headers.FindIndex(h => h.Equals("time_period"))]),
                TimeIdentifier = values[headers.FindIndex(h => h.Equals("time_identifier"))],
                School = new School
                {
                    Estab = values[headers.FindIndex(h => h.Equals("estab"))],
                    LaEstab = values[headers.FindIndex(h => h.Equals("laestab"))],
                    AcademyOpenDate = values[headers.FindIndex(h => h.Equals("academy_open_date"))],
                    AcademyType = values[headers.FindIndex(h => h.Equals("academy_type"))]
                },
                SchoolType =
                    SchoolTypes.EnumFromStringForImport(values[headers.FindIndex(h => h.Equals("school_type"))]),
                Indicators = new Dictionary<string, string>()
            };

            if (headers.Contains("urn"))
            {
                model.School.Urn = values[headers.FindIndex(h => h.Equals("urn"))];
            }

            for (var i = 0; i < values.Length; i++)
            {
                if (!headerValues.Contains(headers[i]))
                {
                    model.Indicators.Add(headers[i], values[i]);
                }
            }
            
            var level = Levels.EnumFromStringForImport(values[headers.FindIndex(h => h.Equals("level"))]);

            var country = new Country
            {
                Code = values[headers.FindIndex(h => h.Equals("country_code"))],
                Name = values[headers.FindIndex(h => h.Equals("country_name"))]
            };

            var region = new Region
            {
                Code = values[headers.FindIndex(h => h.Equals("region_code"))],
                Name = values[headers.FindIndex(h => h.Equals("region_name"))]
            };

            var localAuthority = new LocalAuthority
            {
                Old_Code = values[headers.FindIndex(h => h.Equals("old_la_code"))],
                Code = values[headers.FindIndex(h => h.Equals("new_la_code"))],
                Name = values[headers.FindIndex(h => h.Equals("la_name"))]
            };

            model.Level = LookupCachedLevelOrSet(level, country, region, localAuthority);

            return model;
        }
    }
}