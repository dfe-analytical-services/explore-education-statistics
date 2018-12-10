using System.Collections.Generic;

namespace DataApi.Models
{
    public class LaCharacteristicModel
    {
        public LaCharacteristicModel()
        {
        }

        public int Year { get; set; }

        public string Level { get; set; }

        public Country Country { get; set; }

        public Region Region { get; set; }

        public LocalAuthority LocalAuthority { get; set; }

        public string SchoolType { get; set; }
        
        public Dictionary<string, string> Characteristics { get; set; }
     }
}