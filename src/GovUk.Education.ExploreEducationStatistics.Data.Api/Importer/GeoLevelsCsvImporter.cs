using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class GeoLevelsCsvImporter : CsvImporter
    {
        public GeoLevelsCsvImporter(string path = "") : base(path)
        {
        }

        protected override TidyData TidyDataFromCsv(string csvLine,
            List<string> headers,
            Guid publicationId,
            Guid releaseId,
            DateTime releaseDate)
        {
            var headerValues = new[]
            {
                "term", "year", "level", "country_code", "country_name", "region_code", "region_name", "old_la_code",
                "new_la_code", "la_name", "estab", "laestab", "urn", "academy_type", "academy_open_date", "school_type"
            };
            var values = csvLine.Split(',');

            var model = new GeographicData
            {
                PublicationId = publicationId,
                ReleaseId = releaseId,
                ReleaseDate = releaseDate,
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
                School = new School
                {
                    Estab = values[headers.FindIndex(h => h.Equals("estab"))],
                    LaEstab = values[headers.FindIndex(h => h.Equals("laestab"))],
                    AcademyOpenDate = values[headers.FindIndex(h => h.Equals("academy_open_date"))],
                    AcademyType = values[headers.FindIndex(h => h.Equals("academy_type"))]
                },
                SchoolType =
                    SchoolTypes.EnumFromStringForImport(values[headers.FindIndex(h => h.Equals("school_type"))]),
                Attributes = new Dictionary<string, string>()
            };

            if (headers.Contains("urn"))
            {
                model.School.Urn = values[headers.FindIndex(h => h.Equals("urn"))];
            }

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