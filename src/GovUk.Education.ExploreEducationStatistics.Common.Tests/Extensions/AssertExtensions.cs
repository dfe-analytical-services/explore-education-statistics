using Xunit;
using Xunit.Sdk;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public static class AssertExtensions
    {
        public static T AssertFail<T>(string message) 
        {
            throw new XunitException(message);
        }
    }
}