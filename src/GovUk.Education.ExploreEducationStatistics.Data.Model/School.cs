#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class School : LocationAttribute
    {
        public School(string? urn, string? name) : base(urn, name)
        {
        }
    }
}
