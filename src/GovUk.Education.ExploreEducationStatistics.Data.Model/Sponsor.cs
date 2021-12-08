#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Sponsor : ObservationalUnit, ILocationAttribute
    {
        public Sponsor(string? code, string? name) : base(code, name)
        {
        }

        public static Sponsor Empty()
        {
            return new(null, null);
        }
    }
}
