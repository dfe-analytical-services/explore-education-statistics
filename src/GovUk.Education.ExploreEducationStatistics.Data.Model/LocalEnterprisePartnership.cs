#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class LocalEnterprisePartnership : ObservationalUnit, ILocationAttribute
    {
        public LocalEnterprisePartnership(string? code, string? name) : base(code, name)
        {
        }

        public static LocalEnterprisePartnership Empty()
        {
            return new(null, null);
        }
    }
}
