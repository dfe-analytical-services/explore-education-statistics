#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class EnglishDevolvedArea : ObservationalUnit, ILocationAttribute
    {
        public EnglishDevolvedArea(string? code, string? name) : base(code, name)
        {
        }

        public static EnglishDevolvedArea Empty()
        {
            return new(null, null);
        }
    }
}
