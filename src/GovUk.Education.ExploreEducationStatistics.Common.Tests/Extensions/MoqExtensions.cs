using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public static class ItIs
    {
        public static T DeepEqualTo<T>(T expected)
        {
            return It.Is<T>(actual => actual.IsDeepEqualTo(expected));
        }
    } 
}