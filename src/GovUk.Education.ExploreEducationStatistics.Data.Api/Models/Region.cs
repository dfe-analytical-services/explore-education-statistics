namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Region
    {
        public Region()
        {
        }

        public Region(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public string Name { get; set; }
        
        public string Code { get; set; }
    }
}