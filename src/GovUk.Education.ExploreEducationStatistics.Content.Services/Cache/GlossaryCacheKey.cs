#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache
{
    public record GlossaryCacheKey : IBlobCacheKey
    {
        public string Key => "glossary.json";

        public IBlobContainer Container => BlobContainers.PublicContent;
    }
}
