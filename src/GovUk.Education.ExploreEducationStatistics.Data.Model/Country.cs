namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Country : ObservationalUnit
    {
        public Country(string code, string name) : base(code, name)
        {
        }
        
        public static Country Empty()
        {
            return new Country(null, null);
        }
    }
}