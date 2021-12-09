#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class MayoralCombinedAuthority : ObservationalUnit, ILocationAttribute
    {
        public MayoralCombinedAuthority(string? code, string? name) : base(code, name)
        {
        }

        public static MayoralCombinedAuthority Empty()
        {
            return new(null, null);
        }
    }
}
