namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    /// <summary>
    /// Multi academy trust
    /// </summary>
    public class Mat : ObservationalUnit
    {
        public Mat()
        {
        }
        
        public Mat(string code, string name) : base(code, name)
        {
        }
    }
}