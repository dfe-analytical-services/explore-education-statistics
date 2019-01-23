namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class LocalAuthority
    {
        public LocalAuthority()
        {
        }

        public LocalAuthority(string name, string code, string oldCode)
        {
            Name = name;
            Code = code;
            Old_Code = oldCode;
        }

        public string Name { get; set; }

        public string Code { get; set; }

        public string Old_Code { get; set; }
    }
}