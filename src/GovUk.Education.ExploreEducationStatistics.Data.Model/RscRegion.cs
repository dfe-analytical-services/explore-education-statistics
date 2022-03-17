#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    /// <summary>
    /// Regional School Commissioner Region
    /// </summary>
    public class RscRegion : LocationAttribute
    {
        public RscRegion(string? code) : base(code, code)
        {
        }
    }
}
