#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static System.Threading.Tasks.Task;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public static class ReflectionExtensionsTests
    {
        private const string TestString = "test-value";

        private abstract record BaseClass;

        private record TestClass : BaseClass;

        public class TryBoxToResultTests
        {
            [Fact]
            public void Void()
            {
                Assert.False(TestString.TryBoxToResult(typeof(void), out var boxed));
                Assert.Equal(TestString, boxed);
            }

            [Fact]
            public void ValueType_Itself()
            {
                Assert.True(TestString.TryBoxToResult(typeof(string), out var boxed));
                Assert.Equal(TestString, boxed);
            }

            [Fact]
            public void ValueType_Incompatible()
            {
                Assert.False(TestString.TryBoxToResult(typeof(int), out var boxed));
                Assert.Equal(TestString, boxed);
            }

            [Fact]
            public void ReferenceType_Itself()
            {
                var testClass = new TestClass();
                Assert.True(testClass.TryBoxToResult(typeof(BaseClass), out var boxed));
                Assert.Equal(testClass, boxed);
            }

            [Fact]
            public void ReferenceType_SubType()
            {
                var testClass = new TestClass();
                Assert.True(testClass.TryBoxToResult(typeof(BaseClass), out var boxed));
                Assert.Equal(testClass, boxed);
            }

            [Fact]
            public void ReferenceType_Incompatible()
            {
                var testClass = new TestClass();
                Assert.False(testClass.TryBoxToResult(typeof(string), out var boxed));
                Assert.Equal(testClass, boxed);
            }

            [Fact]
            public void List_Itself()
            {
                var list = ListOf("test 1", "test 2");

                Assert.True(list.TryBoxToResult(typeof(List<string>), out var boxed));
                Assert.Equal(list, boxed);
            }

            [Fact]
            public void List_Interface()
            {
                var list = ListOf("test 1", "test 2");

                Assert.True(list.TryBoxToResult(typeof(IList<string>), out var boxed));
                Assert.Equal(list, boxed);
            }

            [Fact]
            public void List_Collection()
            {
                var list = ListOf("test 1", "test 2");

                Assert.True(list.TryBoxToResult(typeof(ICollection<string>), out var boxed));
                Assert.Equal(list, boxed);
            }

            [Fact]
            public void List_Enumerable()
            {
                var list = ListOf("test 1", "test 2");

                Assert.True(list.TryBoxToResult(typeof(IEnumerable<string>), out var boxed));
                Assert.Equal(list, boxed);
            }

            [Fact]
            public void List_Tasks()
            {
                var list = ListOf(FromResult("test1"), FromResult("test2"));

                Assert.True(list.TryBoxToResult(typeof(List<Task<string>>), out var boxed));
                Assert.Equal(list, boxed);
            }

            [Fact]
            public void List_Tasks_Void()
            {
                var list = ListOf("test1", "test2");

                Assert.False(list.TryBoxToResult(typeof(List<Task>), out var boxed));
                Assert.Equal(list, boxed);
            }

            [Fact]
            public void List_ActionResults()
            {
                var list = ListOf(
                    new ActionResult<string>("test1"), new ActionResult<string>("test2")
                );

                Assert.True(list.TryBoxToResult(typeof(List<ActionResult<string>>), out var boxed));
                Assert.Equal(list, boxed);
            }

            [Fact]
            public void List_ActionResults_Void()
            {
                var list = ListOf(new OkResult(), new OkResult());

                Assert.False(list.TryBoxToResult(typeof(List<ActionResult>), out var boxed));
                Assert.Equal(list, boxed);
            }

            [Fact]
            public void List_Eithers()
            {
                var list = ListOf(
                    new Either<int, string>("test1"), new Either<int, string>("test2")
                );

                Assert.True(list.TryBoxToResult(typeof(List<Either<int, string>>), out var boxed));
                Assert.Equal(list, boxed);
            }

            [Fact]
            public void List_DoesNotBox()
            {
                Assert.False(TestString.TryBoxToResult(typeof(List<string>), out var boxed));
                Assert.Equal(TestString, boxed);
            }

            [Fact]
            public void Dictionary_Itself()
            {
                var dictionary = new Dictionary<string, string>
                {
                    {"key1", "value1"},
                    {"key2", "value2"}
                };

                Assert.True(dictionary.TryBoxToResult(typeof(Dictionary<string, string>), out var boxed));
                Assert.Equal(dictionary, boxed);
            }

            [Fact]
            public void Dictionary_Interface()
            {
                var dictionary = new Dictionary<string, string>
                {
                    {"key1", "value1"},
                    {"key2", "value2"}
                };

                Assert.True(dictionary.TryBoxToResult(typeof(IDictionary<string, string>), out var boxed));
                Assert.Equal(dictionary, boxed);
            }

            [Fact]
            public void Dictionary_Collection()
            {
                var dictionary = new Dictionary<string, string>
                {
                    {"key1", "value1"},
                    {"key2", "value2"}
                };

                Assert.True(dictionary.TryBoxToResult(typeof(ICollection<KeyValuePair<string, string>>), out var boxed));
                Assert.Equal(dictionary, boxed);
            }

            [Fact]
            public void Dictionary_Enumerable()
            {
                var dictionary = new Dictionary<string, string>
                {
                    {"key1", "value1"},
                    {"key2", "value2"}
                };

                Assert.True(dictionary.TryBoxToResult(typeof(IEnumerable<KeyValuePair<string, string>>), out var boxed));
                Assert.Equal(dictionary, boxed);
            }

            [Fact]
            public void Dictionary_Tasks()
            {
                var dictionary = new Dictionary<string, Task<string>>
                {
                    {"key1", FromResult("test1")},
                    {"key2", FromResult("test2")}
                };

                Assert.True(dictionary.TryBoxToResult(typeof(Dictionary<string, Task<string>>), out var boxed));
                Assert.Equal(dictionary, boxed);
            }

            [Fact]
            public void Dictionary_Tasks_Void()
            {
                var dictionary = new Dictionary<string, Task>
                {
                    {"key1", CompletedTask},
                    {"key2", CompletedTask}
                };

                Assert.True(dictionary.TryBoxToResult(typeof(Dictionary<string, Task>), out var boxed));
                Assert.Equal(dictionary, boxed);
            }

            [Fact]
            public void Dictionary_ActionResults()
            {
                var dictionary = new Dictionary<string, ActionResult<string>>
                {
                    {"key1", new ActionResult<string>("value1")},
                    {"key2", new ActionResult<string>("value2")}
                };

                Assert.True(dictionary.TryBoxToResult(typeof(Dictionary<string, ActionResult<string>>), out var boxed));
                Assert.Equal(dictionary, boxed);
            }

            [Fact]
            public void Dictionary_ActionResults_Void()
            {
                var dictionary = new Dictionary<string, string>
                {
                    {"key1", "value1"},
                    {"key2", "value2"}
                };

                Assert.False(dictionary.TryBoxToResult(typeof(List<ActionResult>), out var boxed));
                Assert.Equal(dictionary, boxed);
            }

            [Fact]
            public void Dictionary_Eithers()
            {
                var dictionary = new Dictionary<string, Either<int, string>>
                {
                    {"key1", new Either<int, string>("value1")},
                    {"key2", new Either<int, string>("value2")}
                };

                Assert.True(dictionary.TryBoxToResult(typeof(Dictionary<string, Either<int, string>>), out var boxed));
                Assert.Equal(dictionary, boxed);
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
            public void Either_List_DoesNotBox()
            {
                Assert.False(TestString.TryBoxToResult(typeof(Either<Unit, List<string>>), out var boxed));
                Assert.Equal(TestString, boxed);
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
            public void Task_List_Interface()
            {
                var list = ListOf("test");

                Assert.True(list.TryBoxToResult(typeof(Task<IList<string>>), out var boxed));

                var value = Assert.IsType<Task<IList<string>>>(boxed);
                Assert.Equal(list, value.Result);
            }

            [Fact]
            public void Task_List_Collection()
            {
                var list = ListOf("test");

                Assert.True(list.TryBoxToResult(typeof(Task<ICollection<string>>), out var boxed));

                var value = Assert.IsType<Task<ICollection<string>>>(boxed);
                Assert.Equal(list, value.Result);
            }

            [Fact]
            public void Task_List_Enumerable()
            {
                var list = ListOf("test");

                Assert.True(list.TryBoxToResult(typeof(Task<IEnumerable<string>>), out var boxed));

                var value = Assert.IsType<Task<IEnumerable<string>>>(boxed);
                Assert.Equal(list, value.Result);
            }

            [Fact]
            public void Task_List_Task()
            {
                var list = ListOf(FromResult("test"));

                Assert.True(list.TryBoxToResult(typeof(Task<List<Task<string>>>), out var boxed));

                var value = Assert.IsType<Task<List<Task<string>>>>(boxed);
                Assert.Equal(list, value.Result);
            }

            [Fact]
            public void Task_List_Task_Void()
            {
                var list = ListOf("test");

                Assert.False(list.TryBoxToResult(typeof(Task<List<Task>>), out var boxed));
                Assert.Equal(list, boxed);
            }

            [Fact]
            public void Task_List_DoesNotBox()
            {
                Assert.False(TestString.TryBoxToResult(typeof(Task<List<string>>), out var boxed));
                Assert.Equal(TestString, boxed);
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
            public void ActionResult_List_Interface()
            {
                var list = ListOf("test");

                Assert.True(list.TryBoxToResult(typeof(ActionResult<IList<string>>), out var boxed));

                var value = Assert.IsType<ActionResult<IList<string>>>(boxed);
                Assert.Equal(list, value.Value);
            }

            [Fact]
            public void ActionResult_List_Collection()
            {
                var list = ListOf("test");

                Assert.True(list.TryBoxToResult(typeof(ActionResult<ICollection<string>>), out var boxed));

                var value = Assert.IsType<ActionResult<ICollection<string>>>(boxed);
                Assert.Equal(list, value.Value);
            }

            [Fact]
            public void ActionResult_List_Enumerable()
            {
                var list = ListOf("test");

                Assert.True(list.TryBoxToResult(typeof(ActionResult<IEnumerable<string>>), out var boxed));

                var value = Assert.IsType<ActionResult<IEnumerable<string>>>(boxed);
                Assert.Equal(list, value.Value);
            }

            [Fact]
            public void ActionResult_List_DoesNotBox()
            {
                Assert.False(TestString.TryBoxToResult(typeof(ActionResult<List<string>>), out var boxed));
                Assert.Equal(TestString, boxed);
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
            public void Either_Task_ActionResult_List()
            {
                var list = ListOf("test");

                Assert.True(list.TryBoxToResult(typeof(Either<Unit, Task<ActionResult<List<string>>>>), out var boxed));

                var value = Assert.IsType<Either<Unit, Task<ActionResult<List<string>>>>>(boxed);
                Assert.Equal(list, value.Right.Result.Value);
            }

            [Fact]
            public void Either_Task_ActionResult_List_Interface()
            {
                var list = ListOf("test");

                Assert.True(list.TryBoxToResult(typeof(Either<Unit, Task<ActionResult<IList<string>>>>), out var boxed));

                var value = Assert.IsType<Either<Unit, Task<ActionResult<IList<string>>>>>(boxed);
                Assert.Equal(list, value.Right.Result.Value);
            }

            [Fact]
            public void Either_Task_ActionResult_List_Collection()
            {
                var list = ListOf("test");

                Assert.True(list.TryBoxToResult(typeof(Either<Unit, Task<ActionResult<ICollection<string>>>>), out var boxed));

                var value = Assert.IsType<Either<Unit, Task<ActionResult<ICollection<string>>>>>(boxed);
                Assert.Equal(list, value.Right.Result.Value);
            }

            [Fact]
            public void Either_Task_ActionResult_List_Enumerable()
            {
                var list = ListOf("test");

                Assert.True(list.TryBoxToResult(typeof(Either<Unit, Task<ActionResult<IEnumerable<string>>>>), out var boxed));

                var value = Assert.IsType<Either<Unit, Task<ActionResult<IEnumerable<string>>>>>(boxed);
                Assert.Equal(list, value.Right.Result.Value);
            }

            [Fact]
            public void Either_Task_ActionResult_DoesNotBox()
            {
                Assert.False(TestString.TryBoxToResult(typeof(Either<Unit, Task<ActionResult<List<string>>>>), out var boxed));
                Assert.Equal(TestString, boxed);
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
            public void ValueType()
            {
                Assert.True(TestString.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }

            [Fact]
            public void ReferenceType()
            {
                var testClass = new TestClass();
                Assert.True(testClass.TryUnboxResult(out var unboxed));
                Assert.Equal(testClass, unboxed);
            }

            [Fact]
            public void List()
            {
                var list = ListOf("test1", "test2");

                Assert.True(list.TryUnboxResult(out var unboxed));
                Assert.Equal(list, unboxed);
            }

            [Fact]
            public void List_Tasks()
            {
                var list = ListOf(FromResult("test1"), FromResult("test2"));

                Assert.True(list.TryUnboxResult(out var unboxed));
                Assert.Equal(list, unboxed);
            }

            [Fact]
            public void List_Tasks_Void()
            {
                var list = ListOf(CompletedTask, CompletedTask);

                Assert.True(list.TryUnboxResult(out var unboxed));
                Assert.Equal(list, unboxed);
            }

            [Fact]
            public void List_ActionResults()
            {
                var list = ListOf(new ActionResult<string>("test1"), new ActionResult<string>("test2"));

                Assert.True(list.TryUnboxResult(out var unboxed));
                Assert.Equal(list, unboxed);
            }

            [Fact]
            public void List_ActionResults_Void()
            {
                var list = ListOf<ActionResult>(new OkResult(), new OkResult());

                Assert.True(list.TryUnboxResult(out var unboxed));
                Assert.Equal(list, unboxed);
            }

            [Fact]
            public void List_Eithers()
            {
                var list = ListOf(new Either<int, string>("test1"), new Either<int, string>("test2"));

                Assert.True(list.TryUnboxResult(out var unboxed));
                Assert.Equal(list, unboxed);
            }

            [Fact]
            public void Dictionary()
            {
                var dict = new Dictionary<string, string>
                {
                    {"key1", "value1"},
                    {"key2", "value2"}
                };

                Assert.True(dict.TryUnboxResult(out var unboxed));
                Assert.Equal(dict, unboxed);
            }

            [Fact]
            public void Dictionary_Tasks()
            {
                var dict = new Dictionary<string, Task<string>>
                {
                    {"key1", FromResult("value1")},
                    {"key2", FromResult("value2")}
                };

                Assert.True(dict.TryUnboxResult(out var unboxed));
                Assert.Equal(dict, unboxed);
            }

            [Fact]
            public void Dictionary_Tasks_Void()
            {
                var dict = new Dictionary<string, Task>
                {
                    {"key1", CompletedTask},
                    {"key2", CompletedTask}
                };

                Assert.True(dict.TryUnboxResult(out var unboxed));
                Assert.Equal(dict, unboxed);
            }

            [Fact]
            public void Dictionary_ActionResults()
            {
                var dict = new Dictionary<string, ActionResult<string>>
                {
                    {"key1", new ActionResult<string>("value1")},
                    {"key2", new ActionResult<string>("value2")}
                };

                Assert.True(dict.TryUnboxResult(out var unboxed));
                Assert.Equal(dict, unboxed);
            }

            [Fact]
            public void Dictionary_ActionResults_Void()
            {
                var dict = new Dictionary<string, ActionResult>
                {
                    {"key1", new OkResult()},
                    {"key2", new OkResult()}
                };

                Assert.True(dict.TryUnboxResult(out var unboxed));
                Assert.Equal(dict, unboxed);
            }

            [Fact]
            public void Dictionary_Eithers()
            {
                var dict = new Dictionary<string, Either<int, string>>
                {
                    {"key1", new Either<int, string>("value1")},
                    {"key2", new Either<int, string>("value2")}
                };

                Assert.True(dict.TryUnboxResult(out var unboxed));
                Assert.Equal(dict, unboxed);
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
                var boxed = FromResult(TestString);

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }

            [Fact]
            public void Task_Void()
            {
                var boxed = CompletedTask;

                Assert.False(boxed.TryUnboxResult(out var unboxed));
                Assert.Null(unboxed);

            }

            [Fact]
            public void Task_List()
            {
                var list = ListOf("test");
                var boxed = FromResult(list);

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(list, unboxed);
            }

            [Fact]
            public void Task_List_Tasks()
            {
                var list = ListOf(FromResult("test1"), FromResult("test2"));
                var boxed = FromResult(list);

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(list, unboxed);
            }

            [Fact]
            public void Task_List_Tasks_Void()
            {
                var list = ListOf(CompletedTask, CompletedTask);
                var boxed = FromResult(list);

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
                var boxed = FromResult(new Either<Unit, string>(TestString));

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }

            [Fact]
            public void Task_Either_ActionResult()
            {
                var boxed = FromResult(new Either<Unit, ActionResult<string>>(TestString));

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }

            [Fact]
            public void Task_Task()
            {
                var boxed = FromResult(FromResult(TestString));

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }

            [Fact]
            public void Either_Task()
            {
                var boxed = new Either<Unit, Task<string>>(FromResult(TestString));

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }

            [Fact]
            public void Either_Task_Void()
            {
                var boxed = new Either<Unit, Task>(CompletedTask);

                Assert.False(boxed.TryUnboxResult(out var unboxed));
                Assert.Null(unboxed);
            }

            [Fact]
            public void Either_Task_ActionResult()
            {
                var boxed = new Either<Unit, Task<ActionResult<string>>>(
                    FromResult(new ActionResult<string>(TestString))
                );

                Assert.True(boxed.TryUnboxResult(out var unboxed));
                Assert.Equal(TestString, unboxed);
            }

            [Fact]
            public void Either_Task_ActionResult_Void()
            {
                var result = new OkResult();
                var boxed = new Either<Unit, Task<ActionResult>>(
                    FromResult<ActionResult>(result)
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
