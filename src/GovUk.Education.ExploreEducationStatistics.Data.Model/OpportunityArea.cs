#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class OpportunityArea : ObservationalUnit, ILocationAttribute
    {
        public OpportunityArea(string? code, string? name) : base(code, name)
        {
        }

        public static OpportunityArea Empty()
        {
            return new(null, null);
        }
    }
}
