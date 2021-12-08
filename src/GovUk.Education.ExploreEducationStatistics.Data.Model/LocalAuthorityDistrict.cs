#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class LocalAuthorityDistrict : ObservationalUnit, ILocationAttribute
    {
        public LocalAuthorityDistrict(string? code, string? name) : base(code, name)
        {
        }

        public static LocalAuthorityDistrict Empty()
        {
            return new(null, null);
        }
    }
}
