#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void DistinctByProperty()
        {
            var list = new List<TestClass>
            {
                new(1),
                new(1),
                new(2),
            };

            var distinct = list.DistinctByProperty(x => x.Value).ToList();

            Assert.Equal(2, distinct.Count);
            Assert.Equal(1, distinct[0].Value);
            Assert.Equal(2, distinct[1].Value);
        }

        [Fact]
        public void IndexOfFirst()
        {
            var result = new List<int> { 1, 2, 3 }
                .IndexOfFirst(value => value == 2);

            Assert.Equal(1, result);
        }

        [Fact]
        public void IndexOfFirst_NoMatch()
        {
            var result = new List<int> { 1, 2, 3 }
                .IndexOfFirst(value => value == 4);

            Assert.Equal(-1, result);
        }

        [Fact]
        public async Task ForEachAsync_SuccessfulEitherList()
        {
            Either<Unit, List<int>> results =
                await new List<int> {1, 2}
                    .ForEachAsync(async value =>
                        await GetSuccessfulEither(value));

            Assert.True(results.IsRight);

            var resultsList = results.Right;
            Assert.Equal(2, resultsList.Count);
            Assert.Contains(1, resultsList);
            Assert.Contains(2, resultsList);
        }

        [Fact]
        public async Task ForEachAsync_FailingEither()
        {
            Either<Unit, List<int>> results =
                await new List<int> {1, -1, 2}
                    .ForEachAsync(async value =>
                    {
                        if (value == -1)
                        {
                            return await GetFailingEither();
                        }

                        return await GetSuccessfulEither(value);
                    });

            Assert.True(results.IsLeft);
        }

        [Fact]
        public void SelectNullSafe_NotNull()
        {
            var list = new List<int> {1, 2, 3};
            var results = list.SelectNullSafe(value => value * 2).ToList();
            Assert.Equal(new List<int> {2, 4, 6}, results);
        }

        [Fact]
        public void SelectNullSafe_Null()
        {
            var results = ((List<int>?) null).SelectNullSafe(value => value * 2).ToList();
            Assert.Equal(new List<int>(), results);
        }

        [Fact]
        public void IsNullOrEmpty_Null()
        {
            Assert.True(((List<string>?) null).IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_Empty()
        {
            Assert.True(new List<string>().IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_NeitherNullNorEmpty()
        {
            var list = new List<string> {"foo", "bar"};
            Assert.False(list.IsNullOrEmpty());
        }

        [Fact]
        public void JoinToString()
        {
            var list = new List<string> {"foo", "bar", "baz"};

            Assert.Equal("foo-bar-baz", list.JoinToString('-'));
            Assert.Equal("foo - bar - baz", list.JoinToString(" - "));
            Assert.Equal("foo, bar, baz", list.JoinToString(", "));
        }

        private static async Task<Either<Unit, int>> GetSuccessfulEither(int value)
        {
            await Task.Delay(5);
            return value;
        }

        private static async Task<Either<Unit, int>> GetFailingEither()
        {
            await Task.Delay(5);
            return Unit.Instance;
        }

        private class TestClass
        {
            public readonly int Value;

            public TestClass(int value)
            {
                Value = value;
            }
        }
    }
}
