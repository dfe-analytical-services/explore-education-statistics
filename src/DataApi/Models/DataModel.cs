namespace DataApi.Models
{
    public class DataModel
    {
        protected DataModel()
        {
        }

        protected DataModel(int year, string level, Country country, string schoolType)
        {
            Year = year;
            Level = level;
            Country = country;
            SchoolType = schoolType;
        }

        public int Year { get; set; }

        public string Level { get; set; }

        public Country Country { get; set; }

        public string SchoolType { get; set; }    
    }
}