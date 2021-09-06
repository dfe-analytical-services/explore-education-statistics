using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class School : ObservationalUnit
    {
        public School()
        {
        }
        
        public School(string urn, string name) : base(urn, name)
        {
        }
        
        public static School Empty()
        {
            return new School(null, null);
        }
    }
}
