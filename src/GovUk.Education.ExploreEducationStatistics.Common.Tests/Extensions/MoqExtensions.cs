using System.Collections.Generic;
using System.Linq;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public static class ItIs
    {
        public static T DeepEqualTo<T>(T expected)
        {
            return It.Is<T>(actual => actual.IsDeepEqualTo(expected));
        }

        public static IList<T> ListSequenceEqualTo<T>(IEnumerable<T> expected)
        {
            return It.Is<IList<T>>(actual => actual.SequenceEqual(expected));
        }

        public static IEnumerable<T> EnumerableSequenceEqualTo<T>(IEnumerable<T> expected)
        {
            return It.Is<IEnumerable<T>>(actual => actual.SequenceEqual(expected));
        }

        public static IQueryable<T> QueryableSequenceEqualTo<T>(IEnumerable<T> expected)
        {
            return It.Is<IQueryable<T>>(actual => actual.SequenceEqual(expected));
        }
    } 
}