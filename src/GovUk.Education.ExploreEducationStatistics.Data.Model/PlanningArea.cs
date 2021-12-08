#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class PlanningArea : ObservationalUnit, ILocationAttribute
    {
        public PlanningArea(string? code, string? name) : base(code, name)
        {
        }

        public static PlanningArea Empty()
        {
            return new(null, null);
        }
    }
}
