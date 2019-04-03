using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class NationalCharacteristicImporter : Importer
    {
        public NationalCharacteristicImporter(IMemoryCache cache, ApplicationDbContext context, ILogger<NationalCharacteristicImporter> logger)
            : base(cache, context, logger)
        {
        }

        protected override TidyData TidyDataFromCsv(string csvLine,
            List<string> headers,
            Subject subject)
        {
            var headerValues = new[]
            {
                "time_period", "time_identifier", "level", "country_code", "country_name", "school_type",
                "characteristic_breakdown", "characteristic_label"
            };
            var values = csvLine.Split(',');
            var model = new CharacteristicData
            {
                Subject = subject,
                TimePeriod = int.Parse(values[headers.FindIndex(h => h.Equals("time_period"))]),
                TimeIdentifier = values[headers.FindIndex(h => h.Equals("time_identifier"))],
                SchoolType =
                    SchoolTypes.EnumFromStringForImport(values[headers.FindIndex(h => h.Equals("school_type"))]),
                Indicators = new Dictionary<string, string>(),
                Characteristic = new Characteristic
                {
                    Breakdown = values[headers.FindIndex(h => h.Equals("characteristic_breakdown"))],
                    Label = values[headers.FindIndex(h => h.Equals("characteristic_label"))]
                }
            };

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

            model.Level = LookupCachedLevelOrSet(level, country);

            return model;
        }
    }
}