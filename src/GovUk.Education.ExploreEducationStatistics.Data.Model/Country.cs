#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Country : LocationAttribute
    {
        public Country(string? code, string? name) : base(code, name)
        {
        }
    }
}
