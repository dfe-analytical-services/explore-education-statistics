namespace DataApi.Models
{
    public class GeographicModel
    {
        public int Year { get; set; }
        
        public string Level { get; set; }
        
        public Country Country { get; set; }

        public Region Region { get; set; }

        public LocalAuthority LocalAuthority { get; set; }

        public string laestab { get; set; }

        public SchoolType SchoolType { get; set; }
    }
}