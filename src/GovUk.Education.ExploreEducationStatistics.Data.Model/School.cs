#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class School : ObservationalUnit, ILocationAttribute
    {
        public School(string? urn, string? name) : base(urn, name)
        {
        }

        public static School Empty()
        {
            return new(null, null);
        }
    }
}
