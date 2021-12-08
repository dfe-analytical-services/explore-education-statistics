#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Ward : ObservationalUnit, ILocationAttribute
    {
        public Ward(string? code, string? name) : base(code, name)
        {
        }

        public static Ward Empty()
        {
            return new(null, null);
        }
    }
}
