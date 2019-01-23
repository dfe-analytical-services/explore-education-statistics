using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class MongoNationalCharacteristicCsvImporter : MongoCsvImporter
    {
        public MongoNationalCharacteristicCsvImporter(string path = "") : base(path)
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
                "term", "year", "level", "country_code", "country_name", "school_type", "characteristic_desc",
                "characteristic_1", "characteristic_2"
            };
            var values = csvLine.Split(',');
            var model = new TidyDataNationalCharacteristic
            {
                PublicationId = publicationId,
                ReleaseId = releaseId,
                ReleaseDate = releaseDate,
                Year = int.Parse(values[headers.FindIndex(h => h.Equals("year"))]),
                Level = values[headers.FindIndex(h => h.Equals("level"))],
                Country = new Country
                {
                    Code = values[headers.FindIndex(h => h.Equals("country_code"))],
                    Name = values[headers.FindIndex(h => h.Equals("country_name"))]
                },
                SchoolType = values[headers.FindIndex(h => h.Equals("school_type"))],
                Attributes = new Dictionary<string, string>(),
                Characteristic = new Characteristic
                {
                    Name = values[headers.FindIndex(h => h.Equals("characteristic_1"))],
                    Name2 = values[headers.FindIndex(h => h.Equals("characteristic_2"))],
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