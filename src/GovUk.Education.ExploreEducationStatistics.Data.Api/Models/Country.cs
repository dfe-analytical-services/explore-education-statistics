namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Country
    {
        public Country()
        {
        }

        public Country(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public string Name { get; set; }
        
        public string Code { get; set; }
    }
}