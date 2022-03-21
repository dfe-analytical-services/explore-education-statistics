#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Region : LocationAttribute
    {
        public Region(string? code, string? name) : base(code, name)
        {
        }
    }
}
