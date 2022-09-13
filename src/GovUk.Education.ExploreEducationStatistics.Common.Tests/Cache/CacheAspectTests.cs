#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Cache
{
    [Collection(CacheTestFixture.CollectionName)]
    public class CacheAspectTests : IClassFixture<CacheTestFixture>
    {
        private static readonly Mock<ICacheService<ICacheKey>> CacheService = new(MockBehavior.Strict);

        public CacheAspectTests()
        {
            CacheService.Reset();
        }

        private record TestCacheKey : ICacheKey
        {
            public string Key => "test-key";
        }

        private record TestCacheKeyWithString : ICacheKey
        {
            public string Key { get; }

            public TestCacheKeyWithString(string key)
            {
                Key = key;
            }
        }

        private record TestCacheKeyWithTwoStrings : ICacheKey
        {
            public string Key { get; }

            public TestCacheKeyWithTwoStrings(string key1, string key2)
            {
                Key = $"{key1}-{key2}";
            }
        }

        private record TestCacheKeyWithInt : ICacheKey
        {
            public string Key { get; }

            public TestCacheKeyWithInt(int key)
            {
                Key = key.ToString();
            }
        }

        private record TestCacheKeyWithStringAndInt : ICacheKey
        {
            public string Key { get; }

            public TestCacheKeyWithStringAndInt(string key1, int key2)
            {
                Key = $"{key1}-{key2}";
            }
        }

        private record TestCacheKeyWithDefaultRecordConstructor(string Key1, string Key2) : ICacheKey
        {
            public string Key => $"{Key1}-{Key2}";
        }

        private record TestCacheKeyWithTestParam : ICacheKey
        {
            public string Key { get; }

            public TestCacheKeyWithTestParam(ITestParam param)
            {
                Key = param.Key;
            }
        }

        private record TestCacheKeyWithMultipleConstructors : ICacheKey
        {
            public string Key { get; }

            public TestCacheKeyWithMultipleConstructors(string key1)
            {
                Key = key1;
            }

            public TestCacheKeyWithMultipleConstructors(string key1, string key2)
            {
                Key = $"{key1}-{key2}";
            }

            public TestCacheKeyWithMultipleConstructors(string key1, int key2)
            {
                Key = $"{key1}-{key2}";
            }
        }

        private record TestCacheKeyWithDefaultConstructors : ICacheKey
        {
            public string Key { get; }

            public TestCacheKeyWithDefaultConstructors(string key = "test-default")
            {
                Key = key;
            }

            public TestCacheKeyWithDefaultConstructors(int key1, string key2 = "test-default")
            {
                Key = $"{key1}-{key2}";
            }
        }

        private record TestCacheKeyWithNullableConstructors : ICacheKey
        {
            public string Key { get; }

            public TestCacheKeyWithNullableConstructors(string? key)
            {
                Key = key ?? "test-default";
            }

            public TestCacheKeyWithNullableConstructors(int key1, string? key2)
            {
                var key2WithDefault = key2 ?? "test-default";
                Key = $"{key1}-{key2WithDefault}";
            }
        }

        private record TestCacheKeyWithNullableAndDefaultConstructors : ICacheKey
        {
            public string Key { get; }

            public TestCacheKeyWithNullableAndDefaultConstructors(int? key = 10)
            {
                Key = $"{key}";
            }

            public TestCacheKeyWithNullableAndDefaultConstructors(int key1, string? key2)
            {
                var key2WithString = key2 ?? "test-default";
                Key = $"{key1}-{key2WithString}";
            }

            public TestCacheKeyWithNullableAndDefaultConstructors(string key1, int? key2 = null)
            {
                var key2String = key2 ?? 10;
                Key = $"{key1}-{key2String}";
            }
        }

        private interface ITestParam
        {
            string Key { get; }
        }

        private record TestParam(string Key) : ITestParam;

        private record TestValue
        {
            public Guid Value => Guid.NewGuid();
        }

        private class TestCacheAttribute : CacheAttribute
        {
            public TestCacheAttribute(Type key, bool forceUpdate = false) : base(key, forceUpdate)
            {
            }

            public override async Task<object?> Get(ICacheKey cacheKey, Type returnType)
            {
                return await CacheService.Object.GetItem(cacheKey, returnType);
            }

            public override async Task Set(ICacheKey cacheKey, object value)
            {
                await CacheService.Object.SetItem(cacheKey, value);
            }
        }

        // ReSharper disable UnusedParameter.Local
        private static class TestMethods
        {
            [TestCache(typeof(TestCacheKey))]
            public static void ReturnVoid()
            {
            }

            [TestCache(typeof(TestCacheKey))]
            public static TestValue NoParams()
            {
                return new();
            }

            [TestCache(typeof(TestCacheKey))]
            public static Either<Unit, TestValue> NoParams_Either()
            {
                return new TestValue();
            }

            public static Either<Unit, TestValue> NoParams_Either_Left()
            {
                return Unit.Instance;
            }

            [TestCache(typeof(TestCacheKey))]
            public static ActionResult<TestValue> NoParams_ActionResult()
            {
                return new(new TestValue());
            }

            [TestCache(typeof(TestCacheKey))]
            public static ActionResult<TestValue> NoParams_ActionResult_NotFound()
            {
                return new NotFoundResult();
            }

            [TestCache(typeof(TestCacheKey))]
            public static Task<TestValue> NoParams_Task()
            {
                return Task.FromResult(new TestValue());
            }

            [TestCache(typeof(TestCacheKey))]
            public static Task<ActionResult<TestValue>> NoParams_TaskActionResult()
            {
                return Task.FromResult(new ActionResult<TestValue>(new TestValue()));
            }

            [TestCache(typeof(TestCacheKey))]
            public static Task<ActionResult<TestValue>> NoParams_TaskActionResult_NotFound()
            {
                return Task.FromResult<ActionResult<TestValue>>(new NotFoundResult());
            }

            [TestCache(typeof(TestCacheKey))]
            public static Task<TestValue> NoParams_Task_Failure()
            {
                return Task.FromException<TestValue>(new Exception("Something went wrong"));
            }

            [TestCache(typeof(TestCacheKey))]
            public static Task<Either<Unit, TestValue>> NoParams_TaskEither()
            {
                return Task.FromResult(new Either<Unit, TestValue>(new TestValue()));
            }

            [TestCache(typeof(TestCacheKey))]
            public static Task<Either<Unit, TestValue>> NoParams_TaskEither_Failure()
            {
                return Task.FromException<Either<Unit, TestValue>>(new Exception("Something went wrong"));
            }

            [TestCache(typeof(TestCacheKey))]
            public static Task<Either<Unit, TestValue>> NoParams_TaskEither_Left()
            {
                return Task.FromResult(new Either<Unit, TestValue>(Unit.Instance));
            }

            [TestCache(typeof(TestCacheKey))]
            public static Task<Either<Unit, Task<TestValue>>> NoParams_NestedTaskEither()
            {
                return Task.FromResult(
                    new Either<Unit, Task<TestValue>>(
                        Task.FromResult(new TestValue())
                    )
                );
            }

            [TestCache(typeof(TestCacheKeyWithString))]
            public static TestValue NoParams_NoMatch()
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithString))]
            public static TestValue SingleParam(string param)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithInt))]
            public static TestValue SingleParam_NoMatch(string param)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithTestParam))]
            public static TestValue SingleParam_IsAssignable(ITestParam param)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithTwoStrings))]
            public static TestValue SingleParam_Ambiguous(string param)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithString))]
            public static TestValue TwoParamsOfSameType_Ambiguous(string param1, string param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithString))]
            public static TestValue TwoParamsOfSameType_MatchOnName(string param1, string key)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithInt))]
            public static TestValue TwoParamsOfSameType_NoMatch(string param1, string param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithString))]
            public static TestValue TwoParamsOfSameType_ParamAttribute(
                string param1,
                [CacheKeyParam] string param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithString))]
            public static TestValue TwoParamsOfSameType_BothParamAttributes_Ambiguous(
                [CacheKeyParam] string param1,
                [CacheKeyParam] string param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithString))]
            public static TestValue TwoParamsOfSameType_BothParamAttributes_MatchOnParamName(
                [CacheKeyParam] string param1,
                [CacheKeyParam] string key)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithString))]
            public static TestValue TwoParamsOfSameType_BothParamAttributes_MatchOnAttributeName(
                [CacheKeyParam] string param1,
                [CacheKeyParam("key")] string param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithTwoStrings))]
            public static TestValue TwoParamsOfSameType_KeyWithTwoParamsOfSameType_Ambiguous(
                string param1,
                string param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithTwoStrings))]
            public static TestValue TwoParamsOfSameType_KeyWithTwoParamsOfSameType_MatchOnNames(string key2, string key1)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithTwoStrings))]
            public static TestValue TwoParamsOfSameType_KeyWithTwoParamsOfSameType_BothParamAttributes_SingleAmbiguous(
                    [CacheKeyParam] string param1,
                    [CacheKeyParam] string key2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithTwoStrings))]
            public static TestValue TwoParamsOfSameType_KeyWithTwoParamsOfSameType_BothParamAttributes_BothAmbiguous(
                [CacheKeyParam] string param1,
                [CacheKeyParam] string param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithTwoStrings))]
            public static TestValue
                TwoParamsOfSameType_KeyWithTwoParamsOfSameType_BothParamAttributes_MatchOnParamNames(
                    [CacheKeyParam] string key2,
                    [CacheKeyParam] string key1)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithTwoStrings))]
            public static TestValue
                TwoParamsOfSameType_KeyWithTwoParamsOfSameType_BothParamAttributes_MatchOnAttributeNames(
                    [CacheKeyParam("key2")] string param1,
                    [CacheKeyParam("key1")] string param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithTwoStrings))]
            public static TestValue
                TwoParamsOfSameType_KeyWithTwoParamsOfSameType_BothParamAttributes_MatchOnAttributeAndParamNames(
                    [CacheKeyParam] string key2,
                    [CacheKeyParam("key1")] string param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithString))]
            public static TestValue TwoParamsOfDifferentType(int param1, string param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithStringAndInt))]
            public static TestValue TwoParamsOfDifferentType_KeyWithTwoParamsOfDifferentType(
                int param1,
                string param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithTestParam))]
            public static TestValue TwoParamsOfDifferentType_NoMatch(int param1, string param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithDefaultRecordConstructor))]
            public static TestValue TwoParamsOfSameType_DefaultRecordConstructor(
                string key1,
                string key2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithMultipleConstructors))]
            public static TestValue MultipleConstructors_SingleParam(string param1)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithMultipleConstructors))]
            public static TestValue MultipleConstructors_TwoParamsOfSameType_NoMatch(int param1, int param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithMultipleConstructors))]
            public static TestValue MultipleConstructors_TwoParamsOfSameType_MatchOnName(string key1)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithMultipleConstructors))]
            public static TestValue MultipleConstructors_TwoParamsOfSameType_MatchOnNames(string key2, string key1)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithMultipleConstructors))]
            public static TestValue MultipleConstructors_TwoParamsOfSameType_MatchOnAttributesName(
                string param1,
                [CacheKeyParam("key1")] string param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithMultipleConstructors))]
            public static TestValue MultipleConstructors_TwoParamsOfSameType_MatchOnAttributesNames(
                [CacheKeyParam("key2")] string param1,
                [CacheKeyParam("key1")] string param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithMultipleConstructors))]
            public static TestValue MultipleConstructors_TwoParamsOfDifferentType(int param1, string param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithMultipleConstructors))]
            public static TestValue MultipleConstructors_TwoParamsOfDifferentType_MatchOnAttributesName(
                int param1,
                [CacheKeyParam("key1")] string param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithDefaultConstructors))]
            public static TestValue DefaultConstructors_NoParams()
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithDefaultConstructors))]
            public static TestValue DefaultConstructors_SingleParam(int param1)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithNullableConstructors))]
            public static TestValue NullableConstructors_NoParams()
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithNullableConstructors))]
            public static TestValue NullableConstructors_SingleParam(int param1)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithNullableAndDefaultConstructors))]
            public static TestValue NullableAndDefaultConstructors_NoParams()
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithNullableAndDefaultConstructors))]
            public static TestValue NullableAndDefaultConstructors_SingleParam(int param1)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithNullableAndDefaultConstructors))]
            public static TestValue NullableAndDefaultConstructors_TwoParams(int param1, string key2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithNullableAndDefaultConstructors))]
            public static TestValue NullableAndDefaultConstructors_TwoParams_Nullable(string key1, int? param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithString))]
            [TestCache(typeof(TestCacheKeyWithInt))]
            public static TestValue TwoCaches(string param1, int param2)
            {
                return new();
            }

            [TestCache(typeof(TestCacheKeyWithString))]
            [TestCache(typeof(TestCacheKeyWithInt), Priority = 100)]
            public static TestValue TwoCaches_CustomOrder(string param1, int param2)
            {
                return new();
            }

            [TestCache(null!)]
            public static TestValue NullKeyType()
            {
                return new();
            }

            [TestCache(typeof(object))]
            public static TestValue InvalidKeyType()
            {
                return new();
            }

            [TestCache(typeof(TestCacheKey), forceUpdate: true)]
            public static TestValue ForceUpdate()
            {
                return new();
            }
        }
        // ReSharper enable UnusedParameter.Local

        [Fact]
        public void ReturnVoid_Throws()
        {
            var exception = Assert.Throws<ArgumentException>(TestMethods.ReturnVoid);

            Assert.Equal("Method return type cannot be void", exception.Message);
        }

        [Fact]
        public void NoParams_CacheHit()
        {
            var cacheKey = new TestCacheKey();
            var expected = new TestValue();

            AssertCacheHit(cacheKey, expected, TestMethods.NoParams);
        }

        [Fact]
        public void NoParams_CacheMiss()
        {
            var cacheKey = new TestCacheKey();

            AssertCacheMiss(cacheKey, TestMethods.NoParams);
        }

        [Fact]
        public void NoParams_Either_CacheMiss()
        {
            var cacheKey = new TestCacheKey();

            AssertCacheMiss(cacheKey, () => TestMethods.NoParams_Either().Right);
        }

        [Fact]
        public void NoParams_Either_Left()
        {
            var result = TestMethods.NoParams_Either_Left();

            Assert.True(result.IsLeft);

            VerifyAllMocks(CacheService);
        }

        [Fact]
        public void NoParams_ActionResult()
        {
            var cacheKey = new TestCacheKey();

            AssertCacheMiss(cacheKey, () => TestMethods.NoParams_ActionResult().Value);
        }

        [Fact]
        public void NoParams_ActionResult_NotFound()
        {
            var cacheKey = new TestCacheKey();

            CacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
                .ReturnsAsync(null);

            TestMethods.NoParams_ActionResult_NotFound().AssertNotFoundResult();

            VerifyAllMocks(CacheService);
        }

        [Fact]
        public void NoParams_Task_CacheMiss()
        {
            var cacheKey = new TestCacheKey();

            AssertCacheMiss(cacheKey, () => TestMethods.NoParams_Task().Result);
        }

        [Fact]
        public async Task NoParams_Task_Failure()
        {
            var cacheKey = new TestCacheKey();

            CacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
                .ReturnsAsync(null);

            var exception = await Assert.ThrowsAsync<Exception>(TestMethods.NoParams_Task_Failure);

            Assert.Equal("Something went wrong", exception.Message);

            VerifyAllMocks(CacheService);
        }

        [Fact]
        public void NoParams_TaskEither_CacheHit()
        {
            var cacheKey = new TestCacheKey();
            var expected = new TestValue();

            AssertCacheHit(cacheKey, expected, () => TestMethods.NoParams_TaskEither().Result.Right);
        }

        [Fact]
        public void NoParams_TaskEither_CacheMiss()
        {
            var cacheKey = new TestCacheKey();

            AssertCacheMiss(cacheKey, () => TestMethods.NoParams_TaskEither().Result.Right);
        }

        [Fact]
        public async Task NoParams_TaskEither_Failure()
        {
            var cacheKey = new TestCacheKey();

            CacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
                .ReturnsAsync(null);

            var exception = await Assert.ThrowsAsync<Exception>(TestMethods.NoParams_TaskEither_Failure);

            Assert.Equal("Something went wrong", exception.Message);

            VerifyAllMocks(CacheService);
        }

        [Fact]
        public async void NoParams_TaskEither_Left()
        {
            var cacheKey = new TestCacheKey();

            CacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
                .ReturnsAsync(null);

            var result = await TestMethods.NoParams_TaskEither_Left();

            Assert.True(result.IsLeft);

            VerifyAllMocks(CacheService);
        }

        [Fact]
        public void NoParams_TaskActionResult()
        {
            var cacheKey = new TestCacheKey();

            AssertCacheMiss(cacheKey, () => TestMethods.NoParams_TaskActionResult().Result.Value);
        }

        [Fact]
        public async Task NoParams_TaskActionResult_NotFound()
        {
            var cacheKey = new TestCacheKey();

            CacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
                .ReturnsAsync(null);

            var result = await TestMethods.NoParams_TaskActionResult_NotFound();

            result.AssertNotFoundResult();

            VerifyAllMocks(CacheService);
        }

        [Fact]
        private void NoParams_NestedTaskEither()
        {
            var cacheKey = new TestCacheKey();

            AssertCacheMiss(cacheKey, () => TestMethods.NoParams_NestedTaskEither().Result.Right.Result);
        }

        [Fact]
        public void NoParams_NoMatch()
        {
            AssertNoMatchingConstructorException(typeof(TestCacheKeyWithString), TestMethods.NoParams_NoMatch);
        }

        [Fact]
        public void SingleParam_CacheHit()
        {
            var cacheKey = new TestCacheKeyWithString("test");
            var expected = new TestValue();

            AssertCacheHit(cacheKey, expected, () => TestMethods.SingleParam("test"));
        }

        [Fact]
        public void SingleParam_CacheMiss()
        {
            var cacheKey = new TestCacheKeyWithString("test");

            AssertCacheMiss(cacheKey, () => TestMethods.SingleParam("test"));
        }

        [Fact]
        public void SingleParam_IsAssignable()
        {
            var param = new TestParam("test");
            var cacheKey = new TestCacheKeyWithTestParam(param);

            AssertCacheMiss(cacheKey, () => TestMethods.SingleParam_IsAssignable(param));
        }

        [Fact]
        public void SingleParam_Ambiguous()
        {
            var exception = AssertNoUnambiguousMatchException(
                typeof(TestCacheKeyWithTwoStrings),
                () => TestMethods.SingleParam_Ambiguous("test")
            );

            Assert.Contains(
                "Constructor (String key1, String key2) has candidate parameters (String param) that could not be unambiguously matched",
                exception.Message
            );
        }

        [Fact]
        public void SingleParam_NoMatch()
        {
            AssertNoMatchingConstructorException(
                typeof(TestCacheKeyWithInt),
                () => TestMethods.SingleParam_NoMatch("test")
            );
        }

        [Fact]
        public void TwoParamsOfSameType_Ambiguous()
        {
            var exception = AssertNoUnambiguousMatchException(
                typeof(TestCacheKeyWithString),
                () => TestMethods.TwoParamsOfSameType_Ambiguous("test1", "test2")
            );

            Assert.Contains(
                "Constructor (String key) has multiple matches at position(s): 0",
                exception.Message
            );
        }

        [Fact]
        public void TwoParamsOfSameType_MatchOnName()
        {
            // Second parameter is chosen for the cache key
            // as types are same and parameter names match.
            var cacheKey = new TestCacheKeyWithString("test2");

            AssertCacheMiss(
                cacheKey,
                () =>
                    TestMethods.TwoParamsOfSameType_MatchOnName("test1", "test2")
            );
        }

        [Fact]
        public void TwoParamsOfSameType_ParamAttribute()
        {
            var cacheKey = new TestCacheKeyWithString("test2");

            AssertCacheMiss(
                cacheKey,
                () =>
                    TestMethods.TwoParamsOfSameType_ParamAttribute("test1", "test2")
            );
        }

        [Fact]
        public void TwoParamsOfSameType_BothParamAttributes_Ambiguous()
        {
            var exception = AssertNoUnambiguousMatchException(
                typeof(TestCacheKeyWithString),
                () => TestMethods.TwoParamsOfSameType_BothParamAttributes_Ambiguous("test1", "test2")
            );

            Assert.Contains("Constructor (String key) has multiple matches at position(s): 0", exception.Message);
        }

        [Fact]
        public void TwoParamsOfSameType_BothParamAttributes_MatchOnParamName()
        {
            var cacheKey = new TestCacheKeyWithString("test2");

            AssertCacheMiss(
                cacheKey,
                () => TestMethods.TwoParamsOfSameType_BothParamAttributes_MatchOnParamName("test1", "test2")
            );
        }

        [Fact]
        public void TwoParamsOfSameType_BothParamAttributes_MatchOnAttributeName()
        {
            // Match based on the parameter name specified by the CacheKeyParam attribute
            var cacheKey = new TestCacheKeyWithString("test2");

            AssertCacheMiss(
                cacheKey,
                () => TestMethods.TwoParamsOfSameType_BothParamAttributes_MatchOnAttributeName("test1", "test2")
            );
        }

        [Fact]
        public void TwoParamsOfSameType_KeyWithTwoParamsOfSameType_Ambiguous()
        {
            var exception = AssertNoUnambiguousMatchException(
                typeof(TestCacheKeyWithTwoStrings),
                () => TestMethods.TwoParamsOfSameType_KeyWithTwoParamsOfSameType_Ambiguous("test1", "test2")
            );

            Assert.Contains(
                $"Constructor (String key1, String key2) has multiple matches at position(s): 0, 1",
                exception.Message
            );
        }


        [Fact]
        public void TwoParamsOfSameType_KeyWithTwoParamsOfSameType_MatchOnNames()
        {
            // Order is reversed as we match based on parameter name
            var cacheKey = new TestCacheKeyWithTwoStrings("test2", "test1");

            AssertCacheMiss(
                cacheKey,
                () =>
                    TestMethods.TwoParamsOfSameType_KeyWithTwoParamsOfSameType_MatchOnNames("test1", "test2")
            );
        }

        [Fact]
        public void TwoParamsOfSameType_KeyWithTwoParamsOfSameType_BothParamAttributes_SingleAmbiguous()
        {
            var exception = AssertNoUnambiguousMatchException(
                typeof(TestCacheKeyWithTwoStrings),
                () => TestMethods.TwoParamsOfSameType_KeyWithTwoParamsOfSameType_BothParamAttributes_SingleAmbiguous(
                    "test1",
                    "test2"
                )
            );

            Assert.Contains(
                "Constructor (String key1, String key2) has multiple matches at position(s): 0",
                exception.Message
            );
        }

        [Fact]
        public void TwoParamsOfSameType_KeyWithTwoParamsOfSameType_BothParamAttributes_BothAmbiguous()
        {
            var exception = AssertNoUnambiguousMatchException(
                typeof(TestCacheKeyWithTwoStrings),
                () => TestMethods.TwoParamsOfSameType_KeyWithTwoParamsOfSameType_BothParamAttributes_BothAmbiguous(
                    "test1",
                    "test2"
                )
            );

            Assert.Contains(
                "Constructor (String key1, String key2) has multiple matches at position(s): 0, 1",
                exception.Message
            );
        }

        [Fact]
        public void TwoParamsOfSameType_KeyWithTwoParamsOfSameType_BothParamAttributes_MatchOnParamNames()
        {
            // Order is reversed as we match based on parameter name
            var cacheKey = new TestCacheKeyWithTwoStrings("test2", "test1");

            AssertCacheMiss(
                cacheKey,
                () =>
                    TestMethods.TwoParamsOfSameType_KeyWithTwoParamsOfSameType_BothParamAttributes_MatchOnParamNames(
                        "test1",
                        "test2"
                    )
            );
        }

        [Fact]
        public void TwoParamsOfSameType_KeyWithTwoParamsOfSameType_BothParamAttributes_MatchOnAttributeNames()
        {
            // Order is reversed as we match based on the
            // attribute's specified parameter name to use.
            var cacheKey = new TestCacheKeyWithTwoStrings("test2", "test1");

            AssertCacheMiss(
                cacheKey,
                () =>
                    TestMethods
                        .TwoParamsOfSameType_KeyWithTwoParamsOfSameType_BothParamAttributes_MatchOnAttributeNames(
                            "test1",
                            "test2"
                        )
            );
        }

        [Fact]
        public void TwoParamsOfSameType_KeyWithTwoParamsOfSameType_BothParamAttributes_MatchOnAttributeAndParamNames()
        {
            // Order is reversed as we match based on a combination of
            // the parameter name and attribute names used.
            var cacheKey = new TestCacheKeyWithTwoStrings("test2", "test1");

            AssertCacheMiss(
                cacheKey,
                () =>
                    TestMethods
                        .TwoParamsOfSameType_KeyWithTwoParamsOfSameType_BothParamAttributes_MatchOnAttributeAndParamNames(
                            "test1",
                            "test2"
                        )
            );
        }

        [Fact]
        public void TwoParamsOfSameType_NoMatch()
        {
            AssertNoMatchingConstructorException(
                typeof(TestCacheKeyWithInt),
                () => TestMethods.TwoParamsOfSameType_NoMatch("test1", "test2")
            );
        }

        [Fact]
        public void TwoParamsOfDifferentType_CacheHit()
        {
            var cacheKey = new TestCacheKeyWithString("test");
            var expected = new TestValue();

            AssertCacheHit(
                cacheKey,
                expected,
                () => TestMethods.TwoParamsOfDifferentType(10, "test")
            );
        }

        [Fact]
        public void TwoParamsOfDifferentType_CacheMiss()
        {
            var cacheKey = new TestCacheKeyWithString("test");

            AssertCacheMiss(
                cacheKey,
                () => TestMethods.TwoParamsOfDifferentType(10, "test")
            );
        }

        [Fact]
        public void TwoParamsOfDifferentType_KeyWithTwoParamsOfDifferentType()
        {
            var cacheKey = new TestCacheKeyWithStringAndInt("test", 10);

            AssertCacheMiss(
                cacheKey,
                () => TestMethods.TwoParamsOfDifferentType_KeyWithTwoParamsOfDifferentType(10, "test")
            );
        }

        [Fact]
        public void TwoParamsOfDifferentType_NoMatch()
        {
            AssertNoMatchingConstructorException(
                typeof(TestCacheKeyWithTestParam),
                () => TestMethods.TwoParamsOfDifferentType_NoMatch(10, "test")
            );
        }

        [Fact]
        public void TwoParamsOfSameType_DefaultRecordConstructor()
        {
            var cacheKey = new TestCacheKeyWithDefaultRecordConstructor("Key1Value", "Key2Value");

            AssertCacheMiss(
                cacheKey,
                () => TestMethods.TwoParamsOfSameType_DefaultRecordConstructor("Key1Value", "Key2Value")
            );
        }

        [Fact]
        public void MultipleConstructors_SingleParam()
        {
            var cacheKey = new TestCacheKeyWithMultipleConstructors("test");

            AssertCacheMiss(
                cacheKey,
                () => TestMethods.MultipleConstructors_SingleParam("test")
            );
        }

        [Fact]
        public void MultipleConstructors_TwoParamsOfSameType_NoMatch()
        {
            AssertNoMatchingConstructorException(
                typeof(TestCacheKeyWithMultipleConstructors),
                () => TestMethods.MultipleConstructors_TwoParamsOfSameType_NoMatch(10, 20)
            );
        }

        [Fact]
        public void MultipleConstructors_TwoParamsOfSameType_MatchOnName()
        {
            var cacheKey = new TestCacheKeyWithMultipleConstructors("test");

            AssertCacheMiss(
                cacheKey,
                () => TestMethods.MultipleConstructors_TwoParamsOfSameType_MatchOnName("test")
            );
        }

        [Fact]
        public void MultipleConstructors_TwoParamsOfSameType_MatchOnNames()
        {
            var cacheKey = new TestCacheKeyWithMultipleConstructors("test2", "test1");

            AssertCacheMiss(
                cacheKey,
                () => TestMethods.MultipleConstructors_TwoParamsOfSameType_MatchOnNames("test1", "test2")
            );
        }

        [Fact]
        public void MultipleConstructors_TwoParamsOfSameType_MatchOnAttributesName()
        {
            var cacheKey = new TestCacheKeyWithMultipleConstructors("test2");

            AssertCacheMiss(
                cacheKey,
                () => TestMethods.MultipleConstructors_TwoParamsOfSameType_MatchOnAttributesName("test1", "test2")
            );
        }

        [Fact]
        public void MultipleConstructors_TwoParamsOfSameType_MatchOnAttributesNames()
        {
            var cacheKey = new TestCacheKeyWithMultipleConstructors("test2", "test1");

            AssertCacheMiss(
                cacheKey,
                () => TestMethods.MultipleConstructors_TwoParamsOfSameType_MatchOnAttributesNames("test1", "test2")
            );
        }

        [Fact]
        public void MultipleConstructors_TwoParamsOfDifferentType()
        {
            var cacheKey = new TestCacheKeyWithMultipleConstructors("test", 10);

            AssertCacheMiss(
                cacheKey,
                () => TestMethods.MultipleConstructors_TwoParamsOfDifferentType(10, "test")
            );
        }

        [Fact]
        public void MultipleConstructors_TwoParamsOfDifferentType_MatchOnAttributeName()
        {
            var cacheKey = new TestCacheKeyWithMultipleConstructors("test");

            AssertCacheMiss(
                cacheKey,
                () => TestMethods.MultipleConstructors_TwoParamsOfDifferentType_MatchOnAttributesName(10, "test")
            );
        }

        [Fact]
        public void DefaultConstructors_NoParams()
        {
            var cacheKey = new TestCacheKeyWithDefaultConstructors();

            AssertCacheMiss(cacheKey, TestMethods.DefaultConstructors_NoParams);
        }

        [Fact]
        public void DefaultConstructors_SingleParam()
        {
            var cacheKey = new TestCacheKeyWithDefaultConstructors(10);

            AssertCacheMiss(cacheKey, () => TestMethods.DefaultConstructors_SingleParam(10));
        }

        [Fact]
        public void NullableConstructors_NoParams()
        {
            var cacheKey = new TestCacheKeyWithNullableConstructors(null);

            AssertCacheMiss(cacheKey, TestMethods.NullableConstructors_NoParams);
        }

        [Fact]
        public void NullableConstructors_SingleParam()
        {
            var cacheKey = new TestCacheKeyWithNullableConstructors(10, null);

            AssertCacheMiss(cacheKey, () => TestMethods.NullableConstructors_SingleParam(10));
        }

        [Fact]
        public void NullableAndDefaultConstructors_NoParams()
        {
            var cacheKey = new TestCacheKeyWithNullableAndDefaultConstructors();

            AssertCacheMiss(cacheKey, TestMethods.NullableAndDefaultConstructors_NoParams);
        }

        [Fact]
        public void NullableAndDefaultConstructors_SingleParam()
        {
            var cacheKey = new TestCacheKeyWithNullableAndDefaultConstructors(10, null);

            AssertCacheMiss(cacheKey, () => TestMethods.NullableAndDefaultConstructors_SingleParam(10));
        }


        [Fact]
        public void NullableAndDefaultConstructors_TwoParams()
        {
            var cacheKey = new TestCacheKeyWithNullableAndDefaultConstructors(10, "test1");

            AssertCacheMiss(cacheKey, () => TestMethods.NullableAndDefaultConstructors_TwoParams(10, "test1"));
        }

        [Fact]
        public void NullableAndDefaultConstructors_TwoParams_Nullable()
        {
            var cacheKey = new TestCacheKeyWithNullableAndDefaultConstructors("test", null);

            AssertCacheMiss(cacheKey, () => TestMethods.NullableAndDefaultConstructors_TwoParams_Nullable("test", null));
        }

        [Fact]
        public void TwoCaches_CacheHitOnFirst()
        {
            var cacheKey1 = new TestCacheKeyWithString("test");

            var expected = new TestValue();

            CacheService
                .Setup(s => s.GetItem(cacheKey1, typeof(TestValue)))
                .ReturnsAsync(expected);

            var result = TestMethods.TwoCaches("test", 10);

            Assert.Equal(expected, result);

            VerifyAllMocks(CacheService);
        }

        [Fact]
        public void TwoCaches_CacheHitOnSecond()
        {
            var cacheKey1 = new TestCacheKeyWithString("test");
            var cacheKey2 = new TestCacheKeyWithInt(10);

            var expected = new TestValue();

            CacheService
                .Setup(s => s.GetItem(cacheKey1, typeof(TestValue)))
                .ReturnsAsync(null);
            CacheService
                .Setup(s => s.GetItem(cacheKey2, typeof(TestValue)))
                .ReturnsAsync(expected);

            var result = TestMethods.TwoCaches("test", 10);

            Assert.Equal(expected, result);

            VerifyAllMocks(CacheService);
        }

        [Fact]
        public void TwoCaches_CacheMissAll()
        {
            var cacheKey1 = new TestCacheKeyWithString("test");
            var cacheKey2 = new TestCacheKeyWithInt(10);

            CacheService
                .Setup(s => s.GetItem(cacheKey1, typeof(TestValue)))
                .ReturnsAsync(null);
            CacheService
                .Setup(s => s.GetItem(cacheKey2, typeof(TestValue)))
                .ReturnsAsync(null);

            var args = new List<object>();

            CacheService
                .Setup(s => s.SetItem(cacheKey1, Capture.In(args)))
                .Returns(Task.CompletedTask);
            CacheService
                .Setup(s => s.SetItem(cacheKey2, Capture.In(args)))
                .Returns(Task.CompletedTask);

            var result = TestMethods.TwoCaches("test", 10);

            Assert.Equal(args[0], result);
            Assert.Equal(args[1], result);

            VerifyAllMocks(CacheService);
        }

        [Fact]
        public void TwoCaches_CustomOrder_CacheHitOnFirst()
        {
            // Due to the custom cache order, we are
            // using the second cache first.
            var cacheKey = new TestCacheKeyWithInt(10);

            var expected = new TestValue();

            CacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
                .ReturnsAsync(expected);

            var result = TestMethods.TwoCaches_CustomOrder("test", 10);

            Assert.Equal(expected, result);

            VerifyAllMocks(CacheService);
        }

        [Fact]
        public void TwoCaches_CustomOrder_CacheHitOnSecond()
        {
            // Due to the custom cache order, we are
            // using the first cache last.
            var cacheKey1 = new TestCacheKeyWithInt(10);
            var cacheKey2 = new TestCacheKeyWithString("test");

            var expected = new TestValue();

            CacheService
                .Setup(s => s.GetItem(cacheKey1, typeof(TestValue)))
                .ReturnsAsync(null);
            CacheService
                .Setup(s => s.GetItem(cacheKey2, typeof(TestValue)))
                .ReturnsAsync(expected);

            var result = TestMethods.TwoCaches_CustomOrder("test", 10);

            Assert.Equal(expected, result);

            VerifyAllMocks(CacheService);
        }

        [Fact]
        public void TwoCaches_CustomOrder_CacheMissAll()
        {
            var cacheKey1 = new TestCacheKeyWithString("test");
            var cacheKey2 = new TestCacheKeyWithInt(10);

            CacheService
                .Setup(s => s.GetItem(cacheKey1, typeof(TestValue)))
                .ReturnsAsync(null);
            CacheService
                .Setup(s => s.GetItem(cacheKey2, typeof(TestValue)))
                .ReturnsAsync(null);

            var args = new List<object>();

            CacheService
                .Setup(s => s.SetItem(cacheKey1, Capture.In(args)))
                .Returns(Task.CompletedTask);
            CacheService
                .Setup(s => s.SetItem(cacheKey2, Capture.In(args)))
                .Returns(Task.CompletedTask);

            var result = TestMethods.TwoCaches_CustomOrder("test", 10);

            Assert.Equal(args[0], result);
            Assert.Equal(args[1], result);

            VerifyAllMocks(CacheService);
        }

        [Fact]
        public void NullCacheKeyType()
        {
            var exception = Assert.Throws<ArgumentException>(TestMethods.NullKeyType);

            Assert.Equal($"Cache key type cannot be null", exception.Message);
        }

        [Fact]
        public void InvalidKeyType()
        {
            var exception = Assert.Throws<ArgumentException>(TestMethods.InvalidKeyType);

            Assert.Equal(
                $"Cache key type {typeof(object).FullName} must be assignable to {typeof(ICacheKey).GetPrettyFullName()}",
                exception.Message
            );
        }

        [Fact]
        public void ForceUpdate()
        {
            var cacheKey = new TestCacheKey();

            var args = new List<object>();

            // Verify that there is no attempt to "get" a currently-cached value when the "ForceUpdate" flag is set.
            // This means that we're only ever getting fresh values for the item and then setting or updating the cached
            // entry with that new value rather than ever attempting to retrieve it.
            CacheService
                .Setup(s => s.SetItem(cacheKey, Capture.In(args)))
                .Returns(Task.CompletedTask);

            var returnedItem = TestMethods.ForceUpdate();

            var cachedItem = Assert.Single(args);
            Assert.Equal(cachedItem, returnedItem);

            VerifyAllMocks(CacheService);
        }

        private static void AssertCacheHit(ICacheKey cacheKey, object expectedResult, Func<TestValue> run)
        {
            CacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TestValue)))
                .ReturnsAsync(expectedResult);

            var result = run();

            Assert.Equal(expectedResult, result);

            VerifyAllMocks(CacheService);
        }

        private static void AssertCacheMiss(ICacheKey cacheKey, Func<object> run)
        {
            AssertCacheMiss(cacheKey, typeof(TestValue), run);
        }

        private static void AssertCacheMiss(ICacheKey cacheKey, Type returnType, Func<object> run)
        {
            CacheService
                .Setup(s => s.GetItem(cacheKey, returnType))
                .ReturnsAsync(null);

            var args = new List<object>();

            CacheService
                .Setup(s => s.SetItem(cacheKey, Capture.In(args)))
                .Returns(Task.CompletedTask);

            var result = run();

            Assert.Equal(args[0], result);

            VerifyAllMocks(CacheService);
        }

        private static MissingMemberException AssertNoMatchingConstructorException(
            Type cacheKeyType,
            Func<object> run)
        {
            var exception = Assert.Throws<MissingMemberException>(run);

            Assert.Contains(
                $"No matching constructor for cache key {cacheKeyType.GetPrettyFullName()}",
                exception.Message
            );

            return exception;
        }

        private static AmbiguousMatchException AssertNoUnambiguousMatchException(Type cacheKeyType, Func<TestValue> run)
        {
            var exception = Assert.Throws<AmbiguousMatchException>(run);

            Assert.Contains(
                $"No constructor for cache key {cacheKeyType.GetPrettyFullName()} could be unambiguously matched",
                exception.Message
            );

            return exception;
        }
    }
}
