#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Provider : ObservationalUnit, ILocationAttribute
    {
        public Provider(string? ukprn, string? name) : base(ukprn, name)
        {
        }

        public static Provider Empty()
        {
            return new(null, null);
        }
    }
}
