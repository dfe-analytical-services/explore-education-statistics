#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
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

        public class IsNullableTypeTests
        {
            [Fact]
            public void NullableValueTypes_ReturnsTrue()
            {
                var properties = typeof(NullableValueTypesClass)
                    .GetProperties();

                foreach (var property in properties)
                {
                    var isNullableType = property.PropertyType.IsNullableType();

                    Assert.True(isNullableType);
                }
            }

            [Fact]
            public void NonNullableValueTypes_ReturnsFalse()
            {
                var properties = typeof(NonNullableValueTypesClass)
                    .GetProperties();

                foreach (var property in properties)
                {
                    var isNullableType = property.PropertyType.IsNullableType();

                    Assert.False(isNullableType);
                }
            }

            [Fact]
            public void ReferenceType_ReturnsFalse()
            {
                var isNullableType = typeof(NullableReferenceTypeClass)
                    .GetProperty(nameof(NullableReferenceTypeClass.NonNullableReferenceType))!
                    .PropertyType
                    .IsNullableType();

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

            private class NullableValueTypesClass
            {
                public int? NullableInteger { get; set; }
                public double? NullableDouble { get; set; }
                public TestEnum? NullableEnum { get; set; }
                public bool? NullableBool { get; set; }
                public char? NullableChar { get; set; }
            };

            private class NonNullableValueTypesClass
            {
                public int NonNullableInteger { get; set; }
                public double NonNullableDouble { get; set; }
                public TestEnum NonNullableEnum { get; set; }
                public bool NonNullableBool { get; set; }
                public char NonNullableChar { get; set; }
            };

            private class NullableReferenceTypeClass
            {
                public NullableReferenceTypeClass NonNullableReferenceType { get; set; } = new();
                public NullableReferenceTypeClass? NullableReferenceType { get; set; }
            };

            private enum TestEnum
            {
            }
        }
    }
}
