#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces
{
    public interface IBlobCacheKey : ICacheKey
    {
        IBlobContainer Container { get; }
    }
}