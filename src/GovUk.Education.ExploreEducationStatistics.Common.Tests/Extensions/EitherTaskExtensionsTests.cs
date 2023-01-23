#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class EitherTaskExtensionsTests
{
    public class OrNotFound
    {
        [Fact]
        public async Task ValueType_NotNull()
        {
            var result = await Task.FromResult<int?>(123).OrNotFound();

            var value = result.AssertRight();
            Assert.Equal(123, value);
        }

        [Fact]
        public async Task ValueType_Null()
        {
            var result = await Task.FromResult<int?>(null).OrNotFound();

            result.AssertNotFound();
        }

        [Fact]
        public async Task ReferenceType_NotNull()
        {
            var result = await Task.FromResult<Test?>(new Test()).OrNotFound();

            var value = result.AssertRight();
            Assert.Equal(new Test(), value);
        }

        [Fact]
        public async Task ReferenceTypeType_Null()
        {
            var result = await Task.FromResult<Test?>(null).OrNotFound();

            result.AssertNotFound();
        }

        private record Test;
    }
}
