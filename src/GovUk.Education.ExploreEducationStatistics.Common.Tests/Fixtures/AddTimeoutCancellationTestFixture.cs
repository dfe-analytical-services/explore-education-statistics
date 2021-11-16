#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures
{
    /// <summary>
    /// Simple class fixture for enabling/disabling
    /// cancellation aspects before and after a test suite.
    /// </summary>
    public class AddTimeoutCancellationTestFixture : IDisposable
    {
        public const string CollectionName = "AddTimeoutCancellation tests";

        public AddTimeoutCancellationTestFixture()
        {
            AddTimeoutCancellationAspect.Enabled = true;
        }

        public void Dispose()
        {
            AddTimeoutCancellationAspect.Enabled = false;
        }
    }
}