#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures
{
    /// <summary>
    /// Simple class fixture for enabling/disabling
    /// caching before and after a test suite.
    /// </summary>
    public class CacheTestFixture : IDisposable
    {
        public const string CollectionName = "Cache tests";

        public CacheTestFixture()
        {
            CacheAspect.Enabled = true;
        }

        public void Dispose()
        {
            CacheAspect.Enabled = false;
        }
    }
}