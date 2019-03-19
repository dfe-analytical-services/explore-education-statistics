using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class NationalCharacteristicCsvImporter : CsvImporter
    {
        public NationalCharacteristicCsvImporter(string path = "") : base(path)
        {
        }

        protected override TidyData TidyDataFromCsv(string csvLine,
            List<string> headers,
            Models.Release release)
        {
            var headerValues = new[]
            {
                "time_period", "time_identifier", "level", "country_code", "country_name", "school_type",
                "characteristic_breakdown", "characteristic_label"
            };
            var values = csvLine.Split(',');
            var model = new CharacteristicDataNational
            {
                PublicationId = release.PublicationId,
                Release = release,
                TimePeriod = int.Parse(values[headers.FindIndex(h => h.Equals("time_period"))]),
                TimeIdentifier = values[headers.FindIndex(h => h.Equals("time_identifier"))],
                Level = Levels.EnumFromStringForImport(values[headers.FindIndex(h => h.Equals("level"))]),
                Country = new Country
                {
                    Code = values[headers.FindIndex(h => h.Equals("country_code"))],
                    Name = values[headers.FindIndex(h => h.Equals("country_name"))]
                },
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

            return model;
        }
    }
}