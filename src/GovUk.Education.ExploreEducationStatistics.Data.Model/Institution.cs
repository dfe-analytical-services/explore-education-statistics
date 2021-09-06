namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Institution : ObservationalUnit
    {
        public Institution()
        {
        }
        
        public Institution(string code, string name) : base(code, name)
        {
        }
    }
}