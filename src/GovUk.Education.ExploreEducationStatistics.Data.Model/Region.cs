#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Region : ObservationalUnit, ILocationAttribute
    {
        public Region(string? code, string? name) : base(code, name)
        {
        }

        public static Region Empty()
        {
            return new(null, null);
        }
    }
}
