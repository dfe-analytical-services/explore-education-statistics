#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public interface ILocationAttribute
    {
        public string? Code { get; }

        public string? Name { get; }

        public string GetCacheKey()
        {
            return $"{GetType().Name}:{GetCodeOrFallback()}:{Name ?? string.Empty}";
        }

        public string GetCodeOrFallback()
        {
            return Code ?? string.Empty;
        }
    }
}
