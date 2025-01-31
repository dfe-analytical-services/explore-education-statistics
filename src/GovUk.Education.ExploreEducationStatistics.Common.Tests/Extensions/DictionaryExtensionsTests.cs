using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public static class DictionaryExtensionsTests
    {
        public class GetOrSetTests
        {
            [Fact]
            public void Exists()
            {
                var dictionary = new Dictionary<string, int>
                {
                    ["test"] = 20
                };

                var value = dictionary.GetOrSet("test", 10);

                Assert.Equal(20, value);
                Assert.Equal(20, dictionary["test"]);
            }

            [Fact]
            public void DoesNotExist()
            {
                var dictionary = new Dictionary<string, int>();

                var value = dictionary.GetOrSet("test", 10);

                Assert.Equal(10, value);
                Assert.Equal(10, dictionary["test"]);
            }

            [Fact]
            public void WithSupplier_Exists()
            {
                var dictionary = new Dictionary<string, int>
                {
                    ["test"] = 20
                };

                var value = dictionary.GetOrSet("test", () => 10);

                Assert.Equal(20, value);
                Assert.Equal(20, dictionary["test"]);
            }

            [Fact]
            public void WithSupplier_DoesNotExist()
            {
                var dictionary = new Dictionary<string, int>();

                var value = dictionary.GetOrSet("test", () => 10);

                Assert.Equal(10, value);
                Assert.Equal(10, dictionary["test"]);
            }
        }
    }
}