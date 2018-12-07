using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DataApi.Models;

namespace DataApi
{
    public class CsvReader
    {
        public List<GeographicModel> GeoLevels()
        {
            var directory = Directory.GetCurrentDirectory() + "/data/";
            const string file = "schpupnum_geoglevels.csv";

            var data = File.ReadAllLines(directory + file)
                .Skip(1)
                .Select(FromCsv).ToList();

            return data;
        }

        private static GeographicModel FromCsv(string csvLine)
        {
            var values = csvLine.Split(',');
            var model = new GeographicModel
            {
                Year = int.Parse(values[0]),
                //Level = values[1],
                Country = new Country { Code = values[2], Name = values[3] },
                Region = new Region { Code = values[4], Name = values[5]},
                LocalAuthority = new LocalAuthority { Old_Code = values[6], Code = values[7], Name = values[8]},
                School = new School { estab = int.Parse(values[9]), laestab = int.Parse(values[10]), acad_type = values[11], acad_opend = values[12]},
                //SchoolType = values[13]
            };
            
            Console.WriteLine(model.Year.ToString());
            return model;
        }
    }
}