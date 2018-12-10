using System.Collections.Generic;

namespace DataApi.Models
{
    public class NationalCharacteristicModel
    {
        public NationalCharacteristicModel()
        {
        }

        public int Year { get; set; }

        public string Level { get; set; }

        public Country Country { get; set; }

        public string SchoolType { get; set; }
        
        public Dictionary<string, string> Characteristics { get; set; }
     }
}