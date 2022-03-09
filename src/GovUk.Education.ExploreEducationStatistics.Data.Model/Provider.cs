#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Provider : LocationAttribute
    {
        public Provider(string? ukprn, string? name) : base(ukprn, name)
        {
        }
    }
}
