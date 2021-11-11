namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    public class CancellationAspects
    {
        /// <summary>
        /// Enables the Aspects AddTimeout, CaptureCancellationToken and UseCapturedCancellationToken.
        /// <para>
        /// This is set to false by default so that test code
        /// isn't affected by these aspects. It should be set to
        /// true in your application startup, or if your tests
        /// are concerned with testing cancellation.
        /// </para>
        /// </summary>
        public static bool Enabled { get; set; }
    }
}