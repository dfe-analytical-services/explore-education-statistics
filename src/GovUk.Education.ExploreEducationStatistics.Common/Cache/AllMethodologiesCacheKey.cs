#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache
{
    public sealed class AllMethodologiesCacheKey : ICacheKey
    {
        public string Key => "627b1bfc-3436-474c-9d10-7b0b98b6dee5";

        private AllMethodologiesCacheKey()
        {
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is AllMethodologiesCacheKey other && Key == other.Key;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public static AllMethodologiesCacheKey Instance { get; } = new AllMethodologiesCacheKey();
    }
}
