#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    /// <summary>
    /// Multi academy trust
    /// </summary>
    public class Mat : LocationAttribute
    {
        public Mat(string? code, string? name) : base(code, name)
        {
        }
    }
}
