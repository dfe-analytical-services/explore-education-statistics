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
            int? value = 123;
            var result = await value.OrNotFound();

            result.AssertRight(123);
        }

        [Fact]
        public async Task ValueType_Null()
        {
            int? value = null;
            var result = await value.OrNotFound();

            result.AssertNotFound();
        }

        [Fact]
        public async Task ReferenceType_NotNull()
        {
            var result = await new Test().OrNotFound();

            result.AssertRight(new Test());
        }

        [Fact]
        public async Task ReferenceTypeType_Null()
        {
            Test? value = null;
            var result = await value.OrNotFound();

            result.AssertNotFound();
        }

        [Fact]
        public async Task Task_ValueType_NotNull()
        {
            var result = await Task.FromResult<int?>(123).OrNotFound();

            result.AssertRight(123);
        }

        [Fact]
        public async Task Task_ValueType_Null()
        {
            var result = await Task.FromResult<int?>(null).OrNotFound();

            result.AssertNotFound();
        }

        [Fact]
        public async Task Task_ReferenceType_NotNull()
        {
            var result = await Task.FromResult<Test?>(new Test()).OrNotFound();

            result.AssertRight(new Test());
        }

        [Fact]
        public async Task Task_ReferenceTypeType_Null()
        {
            var result = await Task.FromResult<Test?>(null).OrNotFound();

            result.AssertNotFound();
        }

        private record Test;
    }
}
