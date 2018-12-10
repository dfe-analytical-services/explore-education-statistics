using System.Collections.Generic;

namespace DataApi.Models
{
    public class NationalCharacteristicModel : DataModel
    {
        public NationalCharacteristicModel()
        {
        }

        public NationalCharacteristicModel(int year, string level, Country country, string schoolType,
            Dictionary<string, string> attributes, Dictionary<string, string> characteristics) :
            base(year, level, country, schoolType, attributes)
        {
            Characteristics = characteristics;
        }

        public Dictionary<string, string> Characteristics { get; set; }
    }
}