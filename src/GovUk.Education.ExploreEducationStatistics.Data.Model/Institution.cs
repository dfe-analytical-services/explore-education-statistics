#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Institution : ObservationalUnit, ILocationAttribute
    {
        public Institution(string? code, string? name) : base(code, name)
        {
        }

        public static Institution Empty()
        {
            return new(null, null);
        }
    }
}
