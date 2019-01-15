using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class MongoGeoLevelsCsvImporter : MongoCsvImporter<TidyDataGeographic>
    {
        public MongoGeoLevelsCsvImporter(string path = "") : base(path)
        {
        }

        protected override TidyDataGeographic TidyDataFromCsv(string csvLine, List<string> headers)
        {
            var headerValues = new[]
            {
                "year", "level", "country_code", "country_name", "region_code", "region_name", "old_la_code",
                "new_la_code", "la_name", "estab", "laestab", "acad_type", "acad_opendate", "school_type"
            };
            var values = csvLine.Split(',');
            var model = new TidyDataGeographic
            {
                Year = int.Parse(values[headers.FindIndex(h => h.Equals("year"))]),
                Level = values[headers.FindIndex(h => h.Equals("level"))],
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
                School = new School
                {
                    Estab = values[headers.FindIndex(h => h.Equals("estab"))],
                    LaEstab = values[headers.FindIndex(h => h.Equals("laestab"))],
                    AcademyOpenDate = values[headers.FindIndex(h => h.Equals("acad_opendate"))],
                    AcademyType = values[headers.FindIndex(h => h.Equals("acad_type"))]
                },
                SchoolType = values[headers.FindIndex(h => h.Equals("school_type"))],
                Attributes = new Dictionary<string, string>()
            };

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