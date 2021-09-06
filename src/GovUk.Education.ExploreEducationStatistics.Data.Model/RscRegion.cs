using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    /**
     * Regional School Commissioner Region
     */
    public class RscRegion : ObservationalUnit
    {
        public RscRegion()
        {
        }
        
        public RscRegion(string code) : base(code, code)
        {
        }
        
        public static RscRegion Empty()
        {
            return new RscRegion(null);
        }
    }
}