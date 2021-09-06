namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Ward : ObservationalUnit
    {
        public Ward()
        {
        }
        
        public Ward(string code, string name) : base(code, name)
        {
        }
        
        public static Ward Empty()
        {
            return new Ward(null, null);
        }
    }
}