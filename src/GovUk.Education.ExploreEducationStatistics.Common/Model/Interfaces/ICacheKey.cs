#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Interfaces
{
    public interface ICacheKey<TEntity>
    {
        public string Key { get; }
    }
}
