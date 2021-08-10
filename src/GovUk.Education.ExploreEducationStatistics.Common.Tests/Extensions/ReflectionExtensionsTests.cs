#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public static class ReflectionExtensionsTests
    {
        public class TryBoxToResultTests
        {
            private const string TestString = "test-value";

            [Fact]
            public void VoidBoxTarget()
            {
                Assert.False(TestString.TryBoxToResult(typeof(void), out var boxed));
                Assert.Equal(TestString, boxed);
            }

            [Fact]
            public void SameBoxTarget()
            {
                Assert.True(TestString.TryBoxToResult(typeof(string), out var boxed));
                Assert.Equal(TestString, boxed);
            }

            [Fact]
            public void ListBoxTarget()
            {
                Assert.False(TestString.TryBoxToResult(typeof(List<string>), out var boxed));
                Assert.Equal(TestString, boxed);
            }

            [Fact]
            public void Either()
            {
                Assert.True(TestString.TryBoxToResult(typeof(Either<Unit, string>), out var boxed));

                var value = Assert.IsType<Either<Unit, string>>(boxed);
                Assert.Equal(TestString, value.Right);
            }

            [Fact]
            public void Either_List()
            {
                var list = ListOf("test");

                Assert.True(list.TryBoxToResult(typeof(Either<Unit, List<string>>), out var boxed));

                var value = Assert.IsType<Either<Unit, List<string>>>(boxed);
                Assert.Equal(list, value.Right);
            }

            [Fact]
            public void Task()
            {
                Assert.True(TestString.TryBoxToResult(typeof(Task<string>), out var boxed));

                var value = Assert.IsType<Task<string>>(boxed);
                Assert.Equal(TestString, value.Result);
            }

            [Fact]
            public void Task_Void_DoesNotBox()
            {
                Assert.False(TestString.TryBoxToResult(typeof(Task), out var boxed));
                Assert.Equal(TestString, boxed);
            }

            [Fact]
            public void Task_List()
            {
                var list = ListOf("test");

                Assert.True(list.TryBoxToResult(typeof(Task<List<string>>), out var boxed));

                var value = Assert.IsType<Task<List<string>>>(boxed);
                Assert.Equal(list, value.Result);
            }

            [Fact]
            public void ActionResult()
            {
                Assert.True(TestString.TryBoxToResult(typeof(ActionResult<string>), out var boxed));

                var value = Assert.IsType<ActionResult<string>>(boxed);
                Assert.Equal(TestString, value.Value);
            }

            [Fact]
            public void ActionResult_Void_DoesNotBox()
            {
                Assert.False(TestString.TryBoxToResult(typeof(ActionResult), out var boxed));
                Assert.Equal(TestString, boxed);
            }

            [Fact]
            public void ActionResult_List()
            {
                var list = ListOf("test");

                Assert.True(list.TryBoxToResult(typeof(ActionResult<List<string>>), out var boxed));

                var value = Assert.IsType<ActionResult<List<string>>>(boxed);
                Assert.Equal(list, value.Value);
            }

            [Fact]
            public void Task_Either()
            {
                Assert.True(TestString.TryBoxToResult(typeof(Task<Either<Unit, string>>), out var boxed));

                var value = Assert.IsType<Task<Either<Unit, string>>>(boxed);
                Assert.Equal(TestString, value.Result.Right);
            }

            [Fact]
            public void Task_Either_ActionResult()
            {
                Assert.True(TestString.TryBoxToResult(typeof(Task<Either<Unit, ActionResult<string>>>), out var boxed));

                var value = Assert.IsType<Task<Either<Unit, ActionResult<string>>>>(boxed);
                Assert.Equal(TestString, value.Result.Right.Value);
            }

            [Fact]
            public void Task_Task()
            {
                Assert.True(TestString.TryBoxToResult(typeof(Task<Task<string>>), out var boxed));

                var value = Assert.IsType<Task<Task<string>>>(boxed);
                Assert.Equal(TestString, value.Result.Result);
            }

            [Fact]
            public void Task_Task_Void_DoesNotBox()
            {
                Assert.False(TestString.TryBoxToResult(typeof(Task<Task>), out var boxed));
                Assert.Equal(TestString, boxed);
            }

            [Fact]
            public void Task_ActionResult()
            {
                Assert.True(TestString.TryBoxToResult(typeof(Task<ActionResult<string>>), out var boxed));

                var value = Assert.IsType<Task<ActionResult<string>>>(boxed);
                Assert.Equal(TestString, value.Result.Value);
            }

            [Fact]
            public void Task_ActionResult_Void_DoesNotBox()
            {
                Assert.False(TestString.TryBoxToResult(typeof(Task<ActionResult>), out var boxed));
                Assert.Equal(TestString, boxed);
            }

            [Fact]
            public void Either_Task()
            {
                Assert.True(TestString.TryBoxToResult(typeof(Either<Unit, Task<string>>), out var boxed));

                var value = Assert.IsType<Either<Unit, Task<string>>>(boxed);
                Assert.Equal(TestString, value.Right.Result);
            }

            [Fact]
            public void Either_Task_ActionResult()
            {
                Assert.True(TestString.TryBoxToResult(typeof(Either<Unit, Task<ActionResult<string>>>), out var boxed));

                var value = Assert.IsType<Either<Unit, Task<ActionResult<string>>>>(boxed);
                Assert.Equal(TestString, value.Right.Result.Value);
            }

            [Fact]
            public void Either_Task_ActionResult_Void_DoesNotBox()
            {
                Assert.False(TestString.TryBoxToResult(typeof(Either<Unit, Task<ActionResult>>), out var boxed));
                Assert.Equal(TestString, boxed);
            }

            [Fact]
            public void Either_Either()
            {
                Assert.True(TestString.TryBoxToResult(typeof(Either<Unit, Either<Unit, string>>), out var boxed));

                var value = Assert.IsType<Either<Unit, Either<Unit, string>>>(boxed);
                Assert.Equal(TestString, value.Right.Right);
            }
        }

        public class TryUnboxResultTests
        {
            private const string TestString = "test-value";

            [Fact]
            public void NotBoxedTarget()
            {
                Assert.True(TestString.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }

            [Fact]
            public void Either()
            {
                var boxed = new Either<Unit, string>("test-value");

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }

            [Fact]
            public void Either_List()
            {
                var list = ListOf("test");
                var boxed = new Either<Unit, List<string>>(list);

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(list, unboxed);
            }

            [Fact]
            public void FromTask()
            {
                var boxed = Task.FromResult(TestString);

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }

            [Fact]
            public void Task_Void()
            {
                var boxed = Task.CompletedTask;

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(boxed, unboxed);
            }

            [Fact]
            public void Task_List()
            {
                var list = ListOf("test");
                var boxed = Task.FromResult(list);

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(list, unboxed);
            }

            [Fact]
            public void ActionResult()
            {
                var boxed = new ActionResult<string>(TestString);

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }

            [Fact]
            public void ActionResult_Void()
            {
                var boxed = new NotFoundResult();

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(boxed, unboxed);
            }

            [Fact]
            public void ActionResult_List()
            {
                var list = ListOf("test");
                var boxed = new ActionResult<List<string>>(list);

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(list, unboxed);
            }

            [Fact]
            public void Task_Either()
            {
                var boxed = Task.FromResult(new Either<Unit, string>(TestString));

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }

            [Fact]
            public void Task_Either_ActionResult()
            {
                var boxed = Task.FromResult(new Either<Unit, ActionResult<string>>(TestString));

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }

            [Fact]
            public void Task_Task()
            {
                var boxed = Task.FromResult(Task.FromResult(TestString));

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }

            [Fact]
            public void Either_Task()
            {
                var boxed = new Either<Unit, Task<string>>(Task.FromResult(TestString));

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }

            [Fact]
            public void Either_Task_Void()
            {
                var boxed = new Either<Unit, Task>(Task.CompletedTask);

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(Task.CompletedTask, unboxed);
            }

            [Fact]
            public void Either_Task_ActionResult()
            {
                var boxed = new Either<Unit, Task<ActionResult<string>>>(
                    Task.FromResult(new ActionResult<string>(TestString))
                );

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }

            [Fact]
            public void Either_Task_ActionResult_Void()
            {
                var result = new OkResult();
                var boxed = new Either<Unit, Task<ActionResult>>(
                    Task.FromResult<ActionResult>(result)
                );

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(result, unboxed);
            }

            [Fact]
            public void Either_Either()
            {
                var boxed = new Either<Unit, Either<Unit, string>>(
                    new Either<Unit, string>(TestString)
                );

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }
        }
    }
}