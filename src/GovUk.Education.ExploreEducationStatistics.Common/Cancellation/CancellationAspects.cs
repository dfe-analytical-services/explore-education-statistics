using System.Linq;
using System.Threading;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    public static class CancellationAspects
    {
        /// <summary>
        /// Enables the Aspects AddTimeout, CaptureCancellationToken and AddCapturedCancellation.
        /// <para>
        /// This is set to false by default so that test code
        /// isn't affected by these aspects. It should be set to
        /// true in your application startup, or if your tests
        /// are concerned with testing cancellation.
        /// </para>
        /// </summary>
        public static bool Enabled { get; set; }
        
        public static CancellationToken CombineTokens(params CancellationToken?[] tokens)
        {
            var nonNullTokens = tokens
                .Where(token => token != null)
                .Cast<CancellationToken>()
                .ToArray();

            if (nonNullTokens.Length == 0)
            {
                return new CancellationToken();
            }

            if (nonNullTokens.Length == 1)
            {
                return nonNullTokens[0];
            }

            return CancellationTokenSource
                .CreateLinkedTokenSource(nonNullTokens)
                .Token;
        }
    }
}