namespace DataApi.Models
{
    public class GeographicModel
    {
        public GeographicModel(int year, string level, Country country, Region region, LocalAuthority localAuthority, string laestab, SchoolType schoolType)
        {
            Year = year;
            Level = level;
            Country = country;
            Region = region;
            LocalAuthority = localAuthority;
            this.laestab = laestab;
            SchoolType = schoolType;
        }

        public int Year { get; set; }
        
        public Level Level { get; set; }
        
        public Country Country { get; set; }

        public Region Region { get; set; }

        public LocalAuthority LocalAuthority { get; set; }

        public string laestab { get; set; }

        public SchoolType SchoolType { get; set; }
    }
}