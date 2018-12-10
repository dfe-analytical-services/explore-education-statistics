using System.Collections.Generic;
using System.Linq;

namespace DataApi.Models
{
    public class GeographicModel : DataModel
    {
        public GeographicModel()
        {
        }

        public GeographicModel(int year, string level, Country country, string schoolType,
            Dictionary<string, string> attributes, Region region, LocalAuthority localAuthority, School school) :
            base(year, level, country, schoolType, attributes)
        {
            Region = region;
            LocalAuthority = localAuthority;
            School = school;
        }

        public Region Region { get; set; }

        public LocalAuthority LocalAuthority { get; set; }

        public School School { get; set; }

        public Dictionary<string, string> FilteredAttributes(List<string> attributes)
        {
            var filtered = from kvp in Attributes
                where attributes.Contains(kvp.Value)
                select kvp;

            return filtered.ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}