#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class ParliamentaryConstituency : ObservationalUnit, ILocationAttribute
    {
        public ParliamentaryConstituency(string? code, string? name) : base(code, name)
        {
        }

        public static ParliamentaryConstituency Empty()
        {
            return new(null, null);
        }
    }
}
