namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class LocalAuthorityDistrict : ObservationalUnit
    {
        public LocalAuthorityDistrict()
        {
        }
        
        public LocalAuthorityDistrict(string code, string name) : base(code, name)
        {
        }
        
        public static LocalAuthorityDistrict Empty()
        {
            return new LocalAuthorityDistrict(null, null);
        }
    }
}