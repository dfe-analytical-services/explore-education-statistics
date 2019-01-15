using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api
{
    public class CsvReader : ICsvReader
    {
        private readonly string _path;
        
        public CsvReader(string path = "")
        {
            _path = path;
        }
        
        public IEnumerable<GeographicModel> GeoLevels(string publication, List<string> attributes)
        {
            var file = publication + ".csv";
            var directory = Directory.GetCurrentDirectory();
            var newPath = Path.GetFullPath(Path.Combine(directory, _path));
            
            var path = newPath + "/wwwroot/data/" + file;
            
            Console.WriteLine("Reading data from:" + path);

            var headers = File.ReadLines(path).First().Split(',').ToList();
            
            var data = File.ReadAllLines(path)
                .Skip(1)
                .Select(x => GeographicFromCsv(x, headers)).ToList();
            
            if (attributes.Count > 0)
            {
                data = data.Select(x => x.WithFilteredAttributes(attributes)).ToList();
            }
            
            Console.WriteLine(data.Count() +" rows");
            return data;
        }
        
        public IEnumerable<LaCharacteristicModel> LaCharacteristics(string publication, List<string> attributes)
        {
            var file = publication + ".csv";
            var directory = Directory.GetCurrentDirectory();
            var newPath = Path.GetFullPath(Path.Combine(directory, _path));
            
            var path = newPath + "/wwwroot/data/" + file;
            
            Console.WriteLine("Reading data from:" + path);

            var headers = File.ReadLines(path).First().Split(',').ToList();
            
            var data = File.ReadAllLines(path)
                .Skip(1)
                .Select(x => LaCharacteristicFromCsv(x, headers)).ToList();


            if (attributes.Count > 0)
            {
                data = data.Select(x => x.WithFilteredAttributes(attributes)).ToList();
            }

            Console.WriteLine(data.Count() +" rows");
            return data;
        }
        
        public IEnumerable<NationalCharacteristicModel> NationalCharacteristics(string publication, List<string> attributes)
        {
            var file = publication + ".csv";
            var directory = Directory.GetCurrentDirectory();
            var newPath = Path.GetFullPath(Path.Combine(directory, _path));
            
            var path = newPath + "/wwwroot/data/" + file;

            var headers = File.ReadLines(path).First().Split(',').ToList();
            
            var data = File.ReadAllLines(path)
                .Skip(1)
                .Select(x => NationalCharacteristicFromCsv(x, headers)).ToList();

            if (attributes.Count > 0)
            {
                data = data.Select(x => x.WithFilteredAttributes(attributes)).ToList();
            }
            
            Console.WriteLine(data.Count() +" rows");
            return data;
        }

        private GeographicModel GeographicFromCsv(string csvLine, List<string> headers)
        {
            var headerValues = new string[] {"year","level","country_code","country_name","region_code","region_name","old_la_code","new_la_code","la_name","estab","laestab","acad_type","academy_type","acad_opendate","academy_open_date","school_type"};
            var values = csvLine.Split(',');
            var model = new GeographicModel
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
                    estab = values[headers.FindIndex(h => h.Equals("estab"))], 
                    laestab = values[headers.FindIndex(h => h.Equals("laestab"))],
                    acad_type = values[headers.FindIndex(h => h.Equals("acad_type") || h.Equals("academy_type"))], 
                    acad_opend = values[headers.FindIndex(h => h.Equals("acad_opendate") || h.Equals("academy_open_date"))],
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
        
        private LaCharacteristicModel LaCharacteristicFromCsv(string csvLine, List<string> headers)
        {
            var headerValues = new string[] {"year","level","country_code","country_name","region_code","region_name","old_la_code","new_la_code","la_name","school_type","characteristic_desc","characteristic"};
            var values = csvLine.Split(',');
            var model = new LaCharacteristicModel
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
                SchoolType = values[headers.FindIndex(h => h.Equals("school_type"))],
                Attributes = new Dictionary<string, string>(),
                Characteristic = new Characteristic
                {
                    Name = values[headers.FindIndex(h => h.Equals("characteristic"))],
                    Description = values[headers.FindIndex(h => h.Equals("characteristic_desc"))]
                }
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
        
        private NationalCharacteristicModel NationalCharacteristicFromCsv(string csvLine, List<string> headers)
        {
            var headerValues = new string[] {"year","level","country_code","country_name","school_type","characteristic_desc","characteristic_1","characteristic_2"};
            var values = csvLine.Split(',');
            var model = new NationalCharacteristicModel
            {
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