#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public interface ILocationAttribute
    {
        public string? Code { get; }

        public string? Name { get; }

        /// <summary>
        /// Produces a key string that represents this location attribute uniquely.
        /// </summary>
        /// <remarks>
        /// Used when adding Location cache entries to an in-memory cache while importing statistical data.
        /// </remarks>
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
