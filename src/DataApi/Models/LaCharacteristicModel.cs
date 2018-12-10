using System.Collections.Generic;

namespace DataApi.Models
{
    public class LaCharacteristicModel : DataModel
    {
        public LaCharacteristicModel()
        {
        }

        public LaCharacteristicModel(int year, string level, Country country, string schoolType,
            Dictionary<string, string> attributes, Region region, LocalAuthority localAuthority,
            Dictionary<string, string> characteristics) :
            base(year, level, country, schoolType, attributes)
        {
            Region = region;
            LocalAuthority = localAuthority;
            Characteristics = characteristics;
        }

        public Region Region { get; set; }

        public LocalAuthority LocalAuthority { get; set; }

        public Dictionary<string, string> Characteristics { get; set; }
    }
}