using System.Collections.Generic;

namespace DataApi.Models
{
    public class GeographicModel
    {
        public GeographicModel()
        {
        }

        public GeographicModel(int year, Level level, Country country, Region region, LocalAuthority localAuthority, School school, SchoolType schoolType, Dictionary<string, int> attributes)
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

        public Level Level { get; set; }

        public Country Country { get; set; }

        public Region Region { get; set; }

        public LocalAuthority LocalAuthority { get; set; }

        public School School { get; set; }

        public SchoolType SchoolType { get; set; }
        
        public Dictionary<string, int> Attributes { get; set; }
     }
}