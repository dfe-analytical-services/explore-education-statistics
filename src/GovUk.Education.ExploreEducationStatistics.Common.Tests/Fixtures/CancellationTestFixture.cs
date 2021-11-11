#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures
{
    /// <summary>
    /// Simple class fixture for enabling/disabling
    /// cancellation aspects before and after a test suite.
    /// </summary>
    public class CancellationTestFixture : IDisposable
    {
        public const string CollectionName = "Cancellation tests";

        public CancellationTestFixture()
        {
            CancellationAspects.Enabled = true;
        }

        public void Dispose()
        {
            CancellationAspects.Enabled = false;
        }
    }
}