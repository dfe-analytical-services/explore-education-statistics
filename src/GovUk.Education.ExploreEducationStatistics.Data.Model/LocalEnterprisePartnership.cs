namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class LocalEnterprisePartnership : ObservationalUnit
    {
        public LocalEnterprisePartnership()
        {
        }
        
        public LocalEnterprisePartnership(string code, string name) : base(code, name)
        {
        }
        
        public static LocalEnterprisePartnership Empty()
        {
            return new LocalEnterprisePartnership(null, null);
        }
    }
}