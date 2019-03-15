using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class LaCharacteristicCsvImporter : CsvImporter
    {
        public LaCharacteristicCsvImporter(string path = "") : base(path)
        {
        }

        protected override TidyData TidyDataFromCsv(string csvLine,
            List<string> headers,
            Models.Release release)
        {
            var headerValues = new[]
            {
                "term", "year", "level", "country_code", "country_name", "region_code", "region_name", "old_la_code",
                "new_la_code", "la_name", "school_type", "characteristic_desc", "characteristic"
            };
            var values = csvLine.Split(',');
            var model = new CharacteristicDataLa
            {
                PublicationId = release.PublicationId,
                Release = release,
                Year = int.Parse(values[headers.FindIndex(h => h.Equals("year"))]),
                Level = Levels.EnumFromStringForImport(values[headers.FindIndex(h => h.Equals("level"))]),
                Country = new Country
                {
                    Code = values[headers.FindIndex(h => h.Equals("country_code"))],
                    Name = values[headers.FindIndex(h => h.Equals("country_name"))]
                },
                Region = new Region
                {
                    Code = values[headers.FindIndex(h => h.Equals("region_code"))],
                    Name = values[headers.FindIndex(h => h.Equals("region_name"))]
                },
                LocalAuthority = new LocalAuthority
                {
                    Old_Code = values[headers.FindIndex(h => h.Equals("old_la_code"))],
                    Code = values[headers.FindIndex(h => h.Equals("new_la_code"))],
                    Name = values[headers.FindIndex(h => h.Equals("la_name"))]
                },
                SchoolType =
                    SchoolTypes.EnumFromStringForImport(values[headers.FindIndex(h => h.Equals("school_type"))]),
                Attributes = new Dictionary<string, string>(),
                Characteristic = new Characteristic
                {
                    Name = values[headers.FindIndex(h => h.Equals("characteristic"))],
                    Description = values[headers.FindIndex(h => h.Equals("characteristic_desc"))]
                }
            };

            if (headers.Contains("term"))
            {
                model.Term = values[headers.FindIndex(h => h.Equals("term"))];
            }

            for (var i = 0; i < values.Length; i++)
            {
                if (!headerValues.Contains(headers[i]))
                {
                    model.Attributes.Add(headers[i], values[i]);
                }
            }

            return model;
        }
    }
}