#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class CollectionExtensionTests
{
    public class AddRange
    {
        [Fact]
        public void Set()
        {
            var set = new HashSet<string>();

            set.AddRange(ListOf("a", "b"));

            Assert.Equal(2, set.Count);
            Assert.Contains("a", set);
            Assert.Contains("b", set);
        }

        [Fact]
        public void Set_Varargs()
        {
            var set = new HashSet<string>();

            set.AddRange("a", "b");

            Assert.Equal(2, set.Count);
            Assert.Contains("a", set);
            Assert.Contains("b", set);
        }

        [Fact]
        public void Set_Duplicates()
        {
            var set = new HashSet<string> { "a" };

            set.AddRange(ListOf("a", "b"));

            Assert.Equal(2, set.Count);
            Assert.Contains("a", set);
            Assert.Contains("b", set);
        }

        [Fact]
        public void List()
        {
            var list = new List<string>();

            list.AddRange(ListOf("a", "b"));

            Assert.Equal(2, list.Count);
            Assert.Equal("a", list[0]);
            Assert.Equal("b", list[1]);
        }

        [Fact]
        public void List_Duplicates()
        {
            var list = new List<string> { "a" };

            list.AddRange(ListOf("a", "b"));

            Assert.Equal(3, list.Count);
            Assert.Equal("a", list[0]);
            Assert.Equal("a", list[1]);
            Assert.Equal("b", list[2]);
        }

        [Fact]
        public void List_Varargs()
        {
            var list = new List<string> { "a" };

            list.AddRange("a", "b");

            Assert.Equal(3, list.Count);
            Assert.Equal("a", list[0]);
            Assert.Equal("a", list[1]);
            Assert.Equal("b", list[2]);
        }
    }
}
