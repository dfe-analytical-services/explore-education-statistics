#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class TypeExtensionsTests
{
    public class GetUnboxedResultTypePathTests
    {
        [Fact]
        public void NotBoxed()
        {
            var path = typeof(string).GetUnboxedResultTypePath();

            Assert.Single(path);
            Assert.Equal(typeof(string), path[0]);
        }

        [Fact]
        public void NotBoxed_List()
        {
            var path = typeof(List<string>).GetUnboxedResultTypePath();

            Assert.Single(path);
            Assert.Equal(typeof(List<string>), path[0]);
        }

        [Fact]
        public void Either()
        {
            var path = typeof(Either<Unit, string>).GetUnboxedResultTypePath();

            Assert.Equal(2, path.Count);
            Assert.Equal(typeof(Either<Unit, string>), path[0]);
            Assert.Equal(typeof(string), path[1]);
        }

        [Fact]
        public void Either_Task()
        {
            var path = typeof(Either<Unit, Task<string>>).GetUnboxedResultTypePath();

            Assert.Equal(3, path.Count);
            Assert.Equal(typeof(Either<Unit, Task<string>>), path[0]);
            Assert.Equal(typeof(Task<string>), path[1]);
            Assert.Equal(typeof(string), path[2]);
        }

        [Fact]
        public void Either_ActionResult()
        {
            var path = typeof(Either<Unit, ActionResult<string>>).GetUnboxedResultTypePath();

            Assert.Equal(3, path.Count);
            Assert.Equal(typeof(Either<Unit, ActionResult<string>>), path[0]);
            Assert.Equal(typeof(ActionResult<string>), path[1]);
            Assert.Equal(typeof(string), path[2]);
        }

        [Fact]
        public void Either_ActionResult_Void()
        {
            var path = typeof(Either<Unit, ActionResult>).GetUnboxedResultTypePath();

            Assert.Equal(2, path.Count);
            Assert.Equal(typeof(Either<Unit, ActionResult>), path[0]);
            Assert.Equal(typeof(ActionResult), path[1]);
        }

        [Fact]
        public void Task()
        {
            var path = typeof(Task<string>).GetUnboxedResultTypePath();

            Assert.Equal(2, path.Count);
            Assert.Equal(typeof(Task<string>), path[0]);
            Assert.Equal(typeof(string), path[1]);
        }

        [Fact]
        public void Task_Void()
        {
            var path = typeof(Task).GetUnboxedResultTypePath();

            Assert.Single(path);
            Assert.Equal(typeof(Task), path[0]);
        }

        [Fact]
        public void Task_Either()
        {
            var path = typeof(Task<Either<Unit, string>>).GetUnboxedResultTypePath();

            Assert.Equal(3, path.Count);
            Assert.Equal(typeof(Task<Either<Unit, string>>), path[0]);
            Assert.Equal(typeof(Either<Unit, string>), path[1]);
            Assert.Equal(typeof(string), path[2]);
        }

        [Fact]
        public void Task_ActionResult_Either()
        {
            var path = typeof(Task<ActionResult<Either<Unit, string>>>).GetUnboxedResultTypePath();

            Assert.Equal(4, path.Count);
            Assert.Equal(typeof(Task<ActionResult<Either<Unit, string>>>), path[0]);
            Assert.Equal(typeof(ActionResult<Either<Unit, string>>), path[1]);
            Assert.Equal(typeof(Either<Unit, string>), path[2]);
            Assert.Equal(typeof(string), path[3]);
        }

        [Fact]
        public void Task_ActionResult_Void()
        {
            var path = typeof(Task<ActionResult>).GetUnboxedResultTypePath();

            Assert.Equal(2, path.Count);
            Assert.Equal(typeof(Task<ActionResult>), path[0]);
            Assert.Equal(typeof(ActionResult), path[1]);
        }

        [Fact]
        public void ActionResult()
        {
            var path = typeof(ActionResult<string>).GetUnboxedResultTypePath();

            Assert.Equal(2, path.Count);
            Assert.Equal(typeof(ActionResult<string>), path[0]);
            Assert.Equal(typeof(string), path[1]);
        }

        [Fact]
        public void ActionResult_Void()
        {
            var path = typeof(ActionResult).GetUnboxedResultTypePath();

            Assert.Single(path);
            Assert.Equal(typeof(ActionResult), path[0]);
        }
    }

    public class GetSubclassesTests
    {
        [Fact]
        public void EmptyWhenNoSubclasses()
        {
            var subclasses = typeof(NotDerived)
                .GetSubclasses()
                .ToList();

            Assert.Empty(subclasses);
        }

        [Fact]
        public void ReturnsSubclasses()
        {
            var subclasses = typeof(Base)
                .GetSubclasses()
                .ToList();

            Assert.Equal(2, subclasses.Count);
            Assert.Equal(typeof(Derived1), subclasses[0]);
            Assert.Equal(typeof(Derived2), subclasses[1]);
        }

        private class Base;
        private class Derived1 : Base;
        private class Derived2 : Derived1;

        private class NotDerived;
    }

    public class IsSimpleTests
    {
        public static readonly TheoryData<Type> SimpleTypes = new()
        {
            typeof(int),
            typeof(int?),
            typeof(double),
            typeof(double?),
            typeof(bool),
            typeof(bool?),
            typeof(char),
            typeof(char?),
            typeof(string),
            typeof(decimal),
            typeof(decimal?),
            typeof(TestEnum),
            typeof(TestEnum?),
            typeof(DateTime),
            typeof(DateTime?),
            typeof(DateTimeOffset),
            typeof(DateTimeOffset?),
            typeof(TimeSpan),
            typeof(Uri),
            typeof(Guid),
            typeof(Guid?),
        };

        public static readonly TheoryData<Type> ComplexTypes = new()
        {
            typeof(List<string>),
            typeof(ISet<string>),
            typeof(TypeExtensionsTests),
            typeof(Type),
            typeof(TestStruct),
            typeof(TestStruct?),

        };

        [Theory]
        [MemberData(nameof(SimpleTypes))]
        public void SimpleType_ReturnsTrue(Type simpleType)
        {
            Assert.True(simpleType.IsSimple());
        }
            
        [Theory]
        [MemberData(nameof(ComplexTypes))]
        public void ComplexType_ReturnsFalse(Type complexType)
        {
            Assert.False(complexType.IsSimple());
        }
    }

    public class IsComplexTests
    {
        [Theory]
        [MemberData(nameof(IsSimpleTests.SimpleTypes), MemberType = typeof(IsSimpleTests))]
        public void SimpleType_ReturnsFalse(Type simpleType)
        {
            Assert.False(simpleType.IsComplex());
        }

        [Theory]
        [MemberData(nameof(IsSimpleTests.ComplexTypes), MemberType = typeof(IsSimpleTests))]
        public void ComplexType_ReturnsTrue(Type complexType)
        {
            Assert.True(complexType.IsComplex());
        }
    }

    public class IsNullableTypeTests
    {
        [Theory]
        [InlineData(typeof(int?))]
        [InlineData(typeof(double?))]
        [InlineData(typeof(TestEnum?))]
        [InlineData(typeof(bool?))]
        [InlineData(typeof(char?))]
        public void NullableValueTypes_ReturnsTrue(Type nullableType)
        {
            var isNullableType = nullableType.IsNullableType();

            Assert.True(isNullableType);
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(TestEnum))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(char))]
        public void NonNullableValueTypes_ReturnsFalse(Type nonNullableType)
        {
            var isNullableType = nonNullableType.IsNullableType();

            Assert.False(isNullableType);
        }

        [Fact]
        public void ReferenceType_ReturnsFalse()
        {
            var isNullableType = typeof(NullableReferenceTypeClass).IsNullableType();

            Assert.False(isNullableType);
        }

        [Fact]
        public void NullableReferenceType_ReturnsFalse()
        {
            var isNullableType = typeof(NullableReferenceTypeClass)
                .GetProperty(nameof(NullableReferenceTypeClass.NullableReferenceType))!
                .PropertyType
                .IsNullableType();

            Assert.False(isNullableType);
        }
    }

    public class GetUnderlyingTypeTests
    {
        [Theory]
        [InlineData(typeof(int?), typeof(int))]
        [InlineData(typeof(double?), typeof(double))]
        [InlineData(typeof(TestEnum?), typeof(TestEnum))]
        [InlineData(typeof(bool?), typeof(bool))]
        [InlineData(typeof(char?), typeof(char))]
        public void NullableValueTypes_ReturnsUnderlyingType(Type nullableType, Type expectedUnderlyingType)
        {
            var underlyingType = nullableType.GetUnderlyingType();

            Assert.Equal(expectedUnderlyingType, underlyingType);
        }

        [Theory]
        [InlineData(typeof(int), typeof(int))]
        [InlineData(typeof(double), typeof(double))]
        [InlineData(typeof(TestEnum), typeof(TestEnum))]
        [InlineData(typeof(bool), typeof(bool))]
        [InlineData(typeof(char), typeof(char))]
        public void NonNullableValueTypes_ReturnsUnderlyingType(Type nullableType, Type expectedUnderlyingType)
        {
            var underlyingType = nullableType.GetUnderlyingType();

            Assert.Equal(expectedUnderlyingType, underlyingType);
        }

        [Fact]
        public void ReferenceType_ReturnsUnderlyingType()
        {
            var underlyingType = typeof(NullableReferenceTypeClass).GetUnderlyingType();

            Assert.Equal(typeof(NullableReferenceTypeClass), underlyingType);
        }

        [Fact]
        public void NullableReferenceType_ReturnsUnderlyingType()
        {
            var underlyingType = typeof(NullableReferenceTypeClass)
                .GetProperty(nameof(NullableReferenceTypeClass.NullableReferenceType))!
                .PropertyType
                .GetUnderlyingType();

            Assert.Equal(typeof(NullableReferenceTypeClass), underlyingType);
        }
    }

    private class NullableReferenceTypeClass
    {
        public NullableReferenceTypeClass? NullableReferenceType { get; set; }
    };

    private enum TestEnum;

    private struct TestStruct;
}
