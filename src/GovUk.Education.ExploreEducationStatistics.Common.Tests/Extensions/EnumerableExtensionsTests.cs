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
                new TestClass(1),
                new TestClass(1),
                new TestClass(2),
            };

            var distinct = list.DistinctByProperty(x => x.Value).ToList();

            Assert.Equal(2, distinct.Count);
            Assert.Equal(1, distinct[0].Value);
            Assert.Equal(2, distinct[1].Value);
        }
        
        [Fact]
        public async void ForEachAsync()
        {
            List<int> results = new List<int>();
            
            await new List<int> {1, 2}.ForEachAsync(async value =>
            {
                var result = await GetIntTask(value);
                results.Add(result);
            });
            
            Assert.Equal(2, results.Count);
            Assert.Contains(1, results);
            Assert.Contains(2, results);
        }
        
        [Fact]
        public async void ForEachAsync_SuccessfulEitherList()
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
        public async void ForEachAsync_FailingEither()
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
            var results = ((List<int>) null).SelectNullSafe(value => value * 2).ToList();
            Assert.Equal(new List<int>(), results);            
        }
        
        private static async Task<int> GetIntTask(int value)
        {
            await Task.Delay(5);
            return value;
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