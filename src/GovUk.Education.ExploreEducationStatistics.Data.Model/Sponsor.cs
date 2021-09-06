namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Sponsor : ObservationalUnit
    {
        public Sponsor()
        {
        }
        
        public Sponsor(string code, string name) : base(code, name)
        {
        }
        
        public static Sponsor Empty()
        {
            return new Sponsor(null, null);
        }
    }
}