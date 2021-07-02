using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class IntExtensions
    {
        /// <summary>
        /// Enumerate a series of integers up to the specified value
        /// </summary>
        /// <remarks>
        /// If the input integer is negative, a negative series
        /// of integers will be produced instead.
        /// </remarks>
        public static IEnumerable<int> ToEnumerable(this int value)
        {
            if (value == 0)
            {
                yield break;
            }

            if (value > 0)
            {
                for (var i = 1; i <= value; i++)
                {
                    yield return i;
                }
            }
            else
            {
                for (var i = -1; i >= value; i--)
                {
                    yield return i;
                }
            }
        }
    }
}