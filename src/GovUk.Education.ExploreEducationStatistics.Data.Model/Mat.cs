#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    /// <summary>
    /// Multi academy trust
    /// </summary>
    public class Mat : ObservationalUnit, ILocationAttribute
    {
        public Mat(string? code, string? name) : base(code, name)
        {
        }

        public static Mat Empty()
        {
            return new(null, null);
        }
    }
}
