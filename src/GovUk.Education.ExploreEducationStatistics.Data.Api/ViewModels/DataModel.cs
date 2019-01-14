using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class DataModel<T> where T : DataModel<T>
    {
        protected DataModel()
        {
        }

        protected DataModel(int year, string level, Country country, string schoolType,
            Dictionary<string, string> attributes)
        {
            Year = year;
            Level = level;
            Country = country;
            SchoolType = schoolType;
            Attributes = attributes;
        }

        public int Year { get; set; }

        public string Level { get; set; }

        public Country Country { get; set; }

        public string SchoolType { get; set; }

        public Dictionary<string, string> Attributes { get; set; }

        public T WithFilteredAttributes(List<string> attributes)
        {
            var copy = ShallowCopy();
            copy.Attributes = (
                from kvp in Attributes
                where attributes.Contains(kvp.Key)
                select kvp
            ).ToDictionary(pair => pair.Key, pair => pair.Value);
            return copy;
        }
        
        private T ShallowCopy()
        {
            return (T) MemberwiseClone();
        }
    }
}