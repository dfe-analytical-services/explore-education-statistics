namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Country : ObservationalUnit
    {
        public Country()
        {
        }
        
        public Country(string code, string name) : base(code, name)
        {
        }
    }
}