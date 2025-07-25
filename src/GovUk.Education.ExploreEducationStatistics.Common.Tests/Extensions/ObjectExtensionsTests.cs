#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class ObjectExtensionsTests
{
    public class ToDictionaryTests
    {
        [Fact]
        public void AnonymousObjectWithPrimitives()
        {
            var obj = new
            {
                String = "Test",
                Int = 123,
                Bool = true,
                Null = (object?)null
            };

            var dictionary = obj.ToDictionary();

            Assert.Equal(4, dictionary.Count);
            Assert.Equal("Test", dictionary["String"]);
            Assert.Equal(123, dictionary["Int"]);
            Assert.Equal(true, dictionary["Bool"]);
            Assert.Null(dictionary["Null"]);
        }

        [Fact]
        public void AnonymousObjectWithComplexTypes()
        {
            var anonymousObj = new
            {
                Field = "Test 1"
            };

            var classObj = new TestClassWithPrimitives();

            var list = new List<object>
            {
                "Test 2",
                new { Field = "Test 3" }
            };

            var obj = new
            {
                Anonymous = anonymousObj,
                Class = classObj,
                List = list
            };

            var dictionary = obj.ToDictionary();

            Assert.Equal(3, dictionary.Count);
            Assert.Equal(anonymousObj, dictionary["Anonymous"]);
            Assert.Equal(classObj, dictionary["Class"]);
            Assert.Equal(list, dictionary["List"]);
        }

        [Fact]
        public void TestClassWithPrimitivesObject()
        {
            var obj = new TestClassWithPrimitives
            {
                String = "Test",
                Int = 123,
                Bool = true,
                Null = null
            };

            var dictionary = obj.ToDictionary();

            Assert.Equal(4, dictionary.Count);
            Assert.Equal("Test", dictionary["String"]);
            Assert.Equal(123, dictionary["Int"]);
            Assert.Equal(true, dictionary["Bool"]);
            Assert.Null(dictionary["Null"]);
        }

        [Fact]
        public void TestClassWithHiddenPropertiesObject()
        {
            var obj = new TestClassWithHiddenProperties
            {
                Field = "Test",
            };

            var dictionary = obj.ToDictionary();

            Assert.Single(dictionary);
            Assert.Equal("Test", dictionary["Field"]);
        }

        [Fact]
        public void TestClassWithComplexTypesObject()
        {
            var anonymousObj = new
            {
                Field = "Test 1"
            };

            var classObj = new TestClassWithPrimitives();

            var list = new List<object>
            {
                "Test 2",
                new { Field = "Test 3" }
            };

            var obj = new TestClassWithComplexTypes
            {
                Anonymous = anonymousObj,
                Class = classObj,
                List = list
            };

            var dictionary = obj.ToDictionary();

            Assert.Equal(3, dictionary.Count);
            Assert.Equal(anonymousObj, dictionary["Anonymous"]);
            Assert.Equal(classObj, dictionary["Class"]);
            Assert.Equal(list, dictionary["List"]);
        }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class TestClassWithPrimitives
        {
            public string String { get; init; } = string.Empty;

            public int Int { get; init; }

            public bool Bool { get; init; }

            public object? Null { get; init; }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class TestClassWithHiddenProperties
        {
            public string Field { get; init; } = string.Empty;

            protected string? ProtectedHidden { get; init; } = null;

            private string? PrivateHidden { get; init; } = null;

            public string SomeMethod()
            {
                return "Test";
            }
        }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class TestClassWithComplexTypes
        {
            public object? Anonymous { get; init; }

            public TestClassWithPrimitives? Class { get; init; }

            public List<object>? List { get; init; }
        }
    }
}
