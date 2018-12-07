using System.Collections.Generic;

namespace DataApi.Models
{
    public class GeographicModel
    {
        public GeographicModel()
        {
        }

        public GeographicModel(int year, string level, Country country, Region region, LocalAuthority localAuthority, School school, string schoolType, Dictionary<string, int> attributes)
        {
            Year = year;
            Level = level;
            Country = country;
            Region = region;
            LocalAuthority = localAuthority;
            School = school;
            SchoolType = schoolType;
            Attributes = attributes;
        }

        public int Year { get; set; }

        public string Level { get; set; }

        public Country Country { get; set; }

        public Region Region { get; set; }

        public LocalAuthority LocalAuthority { get; set; }

        public School School { get; set; }

        public string SchoolType { get; set; }
        
        public Dictionary<string, int> Attributes { get; set; }
     }
}