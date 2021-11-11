using System.Threading;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cancellation
{
    public static class CancellationContext
    {
        private static readonly AsyncLocal<CancellationToken?> CurrentToken = new();

        public static void SetCurrent(CancellationToken? token)
        {
            CurrentToken.Value = token;
        }

        public static CancellationToken? GetCurrent()
        {
            return CurrentToken.Value;
        }
    }
}