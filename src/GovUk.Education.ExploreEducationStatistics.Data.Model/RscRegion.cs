#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    /// <summary>
    /// Regional School Commissioner Region
    /// </summary>
    public class RscRegion : ObservationalUnit, ILocationAttribute
    {
        public RscRegion(string? code) : base(code, code)
        {
        }

        public static RscRegion Empty()
        {
            return new(null);
        }
    }
}
