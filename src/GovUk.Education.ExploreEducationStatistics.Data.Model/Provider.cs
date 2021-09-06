namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Provider : ObservationalUnit
    {
        public Provider()
        {
        }
        
        public Provider(string ukprn, string name) : base(ukprn, name)
        {
        }
        
        public static Provider Empty()
        {
            return new Provider(null, null);
        }
    }
}
