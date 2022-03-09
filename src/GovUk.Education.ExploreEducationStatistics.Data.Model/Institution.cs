#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Institution : LocationAttribute
    {
        public Institution(string? code, string? name) : base(code, name)
        {
        }
    }
}
