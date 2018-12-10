using System.Collections.Generic;

namespace DataApi.Models
{
    public class GeographicModel : DataModel
    {
        public GeographicModel()
        {
        }

        public GeographicModel(int year, string level, Country country, string schoolType, Region region,
            LocalAuthority localAuthority, School school, Dictionary<string, string> attributes) :
            base(year, level, country, schoolType)
        {
            Region = region;
            LocalAuthority = localAuthority;
            School = school;
            Attributes = attributes;
        }

        public Region Region { get; set; }

        public LocalAuthority LocalAuthority { get; set; }

        public School School { get; set; }

        public Dictionary<string, string> Attributes { get; set; }
    }
}