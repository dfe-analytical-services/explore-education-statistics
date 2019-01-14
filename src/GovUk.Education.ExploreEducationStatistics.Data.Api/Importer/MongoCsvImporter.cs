using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class MongoCsvImporter
    {
        private readonly string _path;
        
        public MongoCsvImporter(string path = "")
        {
            _path = path;
        }
        
        public IEnumerable<TidyData> GeoLevels(string publication)
        {
            var file = publication + ".csv";
            var directory = Directory.GetCurrentDirectory();
            var newPath = Path.GetFullPath(Path.Combine(directory, _path));
            
            var path = newPath + "/wwwroot/data/" + file;
            
            Console.WriteLine("Reading data from:" + path);

            var headers = File.ReadLines(path).First().Split(',').ToList();
            
            var data = File.ReadAllLines(path)
                .Skip(1)
                .Select(x => TidyDataFromCsv(x, headers)).ToList();
            
            Console.WriteLine(data.Count() +" rows");
            return data;
        }
        
        private TidyData TidyDataFromCsv(string csvLine, List<string> headers)
        {
            var headerValues = new string[] {"year","level","country_code","country_name","region_code","region_name","old_la_code","new_la_code","la_name","estab","laestab","acad_type","academy_type","acad_opendate","academy_open_date","school_type"};
            var values = csvLine.Split(',');
            var model = new TidyData
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
                Estab = values[headers.FindIndex(h => h.Equals("estab"))], 
                LaEstab = values[headers.FindIndex(h => h.Equals("laestab"))],
                SchoolType = values[headers.FindIndex(h => h.Equals("school_type"))], 
            };

            return model;
        }
    }
}