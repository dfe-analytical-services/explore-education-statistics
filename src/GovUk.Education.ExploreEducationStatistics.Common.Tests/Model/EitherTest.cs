#nullable enable
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions.AssertExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Model
{
    public class EitherTest
    {
        [Fact]
        public void Left()
        {
            var result = new Either<string, int>("a failure");

            Assert.True(result.IsLeft);
            Assert.False(result.IsRight);
            Assert.Equal("a failure", result.Left);
            Assert.Throws<ArgumentException>(() => result.Right);
        }

        [Fact]
        public void Right()
        {
            var result = new Either<int, string>("a success");

            Assert.True(result.IsRight);
            Assert.False(result.IsLeft);
            Assert.Equal("a success", result.Right);
            Assert.Throws<ArgumentException>(() => result.Left);
        }

        [Fact]
        public void Fold_Right()
        {
            var result = new Either<int, string>("a success")
                .Fold(
                    value => value.ToString(),
                    value => value.ToUpper()
                );

            Assert.Equal("A SUCCESS", result);
        }

        [Fact]
        public void Fold_Left()
        {
            var result = new Either<int, string>(10)
                .Fold(
                    value => value.ToString(),
                    value => value.ToUpper()
                );

            Assert.Equal("10", result);
        }

        [Fact]
        public void FoldRight_Right()
        {
            var result = new Either<int, string>("a success")
                .FoldRight(value => value.ToUpper(), "a default");

            Assert.Equal("A SUCCESS", result);
        }

        [Fact]
        public void FoldRight_Left()
        {
            var result = new Either<int, string>(10)
                .FoldRight(value => value.ToUpper(), "a default");

            Assert.Equal("a default", result);
        }

        [Fact]
        public void FoldLeft_Left()
        {
            var result = new Either<int, string>(10)
                .FoldLeft(value => value.ToString(), "a default");

            Assert.Equal("10", result);
        }

        [Fact]
        public void FoldLeft_Right()
        {
            var result = new Either<int, string>("a success")
                .FoldLeft(value => value.ToString(), "a default");

            Assert.Equal("a default", result);
        }

        [Fact]
        public void OnSuccess()
        {
            var result =
                new Either<int, string>("either1")
                    .OnSuccess(previousResult => previousResult + " either2");

            result.AssertRight("either1 either2");
        }

        [Fact]
        public void OnSuccess_Left_PriorFailure()
        {
            var results = new List<string>();

            var result =
                new Either<int, string>(500)
                    .OnSuccess(previousResult =>
                    {
                        results.Add(previousResult + " either2");
                        return previousResult + "either2";
                    });

            result.AssertLeft(500);

            Assert.Empty(results);
        }

        [Fact]
        public void OnSuccess_NoArg()
        {
            var result =
                new Either<int, string>("either1")
                    .OnSuccess(() => "either2");

            result.AssertRight("either2");
        }

        [Fact]
        public void OnSuccess_NoArg_Left_PriorFailure()
        {
            var results = new List<string>();

            var result =
                new Either<int, string>(500)
                    .OnSuccess(() =>
                    {
                        results.Add("either2");
                        return "either2";
                    });

            result.AssertLeft(500);

            Assert.Empty(results);
        }

        [Fact]
        public async Task OnSuccess_WithEitherTask()
        {
            var result =
                await new Either<int, string>("either1")
                    .OnSuccess(previousResult =>
                        Task.FromResult(new Either<int, string>(previousResult + " either2")));

            result.AssertRight("either1 either2");
        }

        [Fact]
        public void OrElse_NoArg()
        {
            var result =
                new Either<int, string>(500)
                    .OrElse(() => "either2");

            result.AssertRight("either2");
        }

        [Fact]
        public void OrElse_NoArg_PriorSuccess()
        {
            var results = new List<string>();

            var result =
                new Either<int, string>("either1")
                    .OrElse(() =>
                    {
                        results.Add("either2");
                        return "either2";
                    });

            result.AssertRight("either1");

            Assert.Empty(results);
        }

        [Fact]
        public async Task OnSuccess_WithEitherTask_Left()
        {
            var results = new List<string>();

            var result =
                await new Either<int, string>("either1")
                    .OnSuccess(previousResult => Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        results.Add(previousResult + " task1");
                        return new Either<int, string>(500);
                    }))
                    .OnSuccess(previousResult => Task.Run(async () =>
                    {
                        await Task.Delay(10);
                        results.Add(previousResult + " task2");
                        return new Either<int, string>(previousResult + " task2");
                    }));

            result.AssertLeft(500);

            Assert.Equal("either1 task1", Assert.Single(results));
        }

        [Fact]
        public async Task OnSuccess_WithEitherTask_Left_PriorFailure()
        {
            var results = new List<string>();

            var result =
                await new Either<int, string>(500)
                    .OnSuccess(previousResult => Task.Run(() =>
                    {
                        results.Add(previousResult + " task1");
                        return new Either<int, string>(600);
                    }));

            result.AssertLeft(500);

            Assert.Empty(results);
        }
    }

    public class EitherTaskTest
    {
        [Fact]
        public async Task OnSuccess()
        {
            var result = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccess(str => str.ToUpper());

            result.AssertRight("A SUCCESS");
        }

        [Fact]
        public async Task OnSuccess_WithRight()
        {
            var result = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccess(_ => new Either<int, string>("another success"));

            result.AssertRight("another success");
        }

        [Fact]
        public async Task OnSuccess_WithLeft()
        {
            var result = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccess(_ => new Either<int, string>(500));

            result.AssertLeft(500);
        }

#pragma warning disable 618
        [Fact]
        public async Task OnSuccess_WithTask()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(5);
                    results.Add(previousResult + " task2");
                }))
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                .OnSuccess(previousResult => Task.Run(async () =>
                {
                    Assert.IsType<Unit>(previousResult);
                    await Task.Delay(15);
                    results.Add("task3");
                }));

            Assert.True(result.IsRight);
            Assert.Equal(3, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task1 task2", results[1]);
            Assert.Equal("task3", results[2]);
        }

        [Fact]
        public async Task OnSuccess_WithTask_Throws()
        {
            var results = new List<string>();

            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(5);
                    results.Add(previousResult + " task2");
                    throw new Exception("exception thrown");
                }))
                .OnSuccess(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(15);
                    results.Add(previousResult + " task3");
                })));

            Assert.Equal("exception thrown", exception.Message);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task1 task2", results[1]);
        }
#pragma warning restore 618

        [Fact]
        public async Task OnSuccess_WithGenericTask()
        {
            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    return new Either<int, string>("task1");
                })
                .OnSuccess(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    return previousResult + " task2";
                }))
                .OnSuccess(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    return previousResult + " task3";
                }));

            result.AssertRight("task1 task2 task3");
        }

        [Fact]
        public async Task OnSuccess_WithGenericTask_Throws()
        {
            var results = new List<string>();

            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    results.Add(previousResult + " task2");
                    throw new Exception("exception thrown");
#pragma warning disable 162
                    return previousResult + " task2";
#pragma warning restore 162
                }))
                .OnSuccess(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    results.Add(previousResult + " task3");
                    return previousResult + " task3";
                })));

            Assert.Equal("exception thrown", exception.Message);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task1 task2", results[1]);
        }

        [Fact]
        public async Task OnSuccess_WithGenericTaskNoArg()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    results.Add("task2");
                    return "task2";
                }))
                .OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    results.Add("task3");
                    return "task3";
                }));

            result.AssertRight("task3");

            Assert.Equal(3, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task2", results[1]);
            Assert.Equal("task3", results[2]);
        }

        [Fact]
        public async Task OnSuccess_WithGenericTaskNoArg_Throws()
        {
            var results = new List<string>();

            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    results.Add("task2");
                    throw new Exception("exception thrown");
#pragma warning disable 162
                    return "task2";
#pragma warning restore 162
                }))
                .OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    results.Add("task3");
                    return "task3";
                })));

            Assert.Equal("exception thrown", exception.Message);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task2", results[1]);
        }

        [Fact]
        public async Task OnSuccess_WithGenericTaskNoArg_Left_PriorFailure()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>(500);
                })
                .OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    results.Add("task2");
                    return "task2";
                }));

            result.AssertLeft(500);

            Assert.Equal("task1", Assert.Single(results));
        }

        [Fact]
        public async Task OnSuccess_WithEitherTask()
        {
            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    return new Either<int, string>("task1");
                })
                .OnSuccess(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    return new Either<int, string>(previousResult + " task2");
                }))
                .OnSuccess(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    return new Either<int, string>(previousResult + " task3");
                }));

            result.AssertRight("task1 task2 task3");
        }

        [Fact]
        public async Task OnSuccess_WithEitherTask_Left()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    results.Add(previousResult + " task2");
                    return new Either<int, string>(500);
                }))
                .OnSuccess(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    results.Add(previousResult + " task3");
                    return new Either<int, string>(previousResult + " task3");
                }));

            result.AssertLeft(500);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task1 task2", results[1]);
        }

        [Fact]
        public async Task OnSuccess_WithEitherTaskNoArg()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    results.Add("task2");
                    return new Either<int, string>("task2");
                })).OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    results.Add("task3");
                    return new Either<int, string>("task3");
                }));

            result.AssertRight("task3");

            Assert.Equal(3, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task2", results[1]);
            Assert.Equal("task3", results[2]);
        }

        [Fact]
        public async Task OnSuccess_WithEitherTaskNoArg_Left()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    results.Add("task2");
                    return new Either<int, string>(500);
                }))
                .OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    results.Add("task3");
                    return new Either<int, string>("task3");
                }));

            result.AssertLeft(500);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task2", results[1]);
        }

        [Fact]
        public async Task OnSuccessVoid_WithTask()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    results.Add(previousResult + " task2");
                }))
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                .OnSuccessVoid(previousResult => Task.Run(async () =>
                {
                    Assert.IsType<Unit>(previousResult);
                    await Task.Delay(0);
                    results.Add("task3");
                }));

            result.AssertRight(Unit.Instance);

            Assert.Equal(3, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task1 task2", results[1]);
            Assert.Equal("task3", results[2]);
        }

        [Fact]
        public async Task OnSuccessVoid_WithTask_Throws()
        {
            var results = new List<string>();

            var exception = await Assert.ThrowsAsync<Exception>(async () => await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    results.Add(previousResult + " task2");
                    throw new Exception("exception thrown");
                }))
                .OnSuccessVoid(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    results.Add(previousResult + " task3");
                })));

            Assert.Equal("exception thrown", exception.Message);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task1 task2", results[1]);
        }


        [Fact]
        public async Task OnSuccessVoid_WithTask_Left_PriorFailure()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>(500);
                })
                .OnSuccessVoid(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    results.Add(previousResult + " task2");
                }));

            result.AssertLeft(500);

            Assert.Equal("task1", Assert.Single(results));
        }

        [Fact]
        public async Task OnSuccessVoid_WithTaskNoArg()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid(() => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    results.Add("task2");
                }))
                .OnSuccessVoid(() => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    results.Add("task3");
                }));

            result.AssertRight(Unit.Instance);

            Assert.Equal(3, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task2", results[1]);
            Assert.Equal("task3", results[2]);
        }

        [Fact]
        public async Task OnSuccessVoid_WithTaskNoArg_Throws()
        {
            var results = new List<string>();

            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid(() => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    results.Add("task2");
                    throw new Exception("exception thrown");
                }))
                .OnSuccessVoid(() => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    results.Add("task3");
                })));

            Assert.Equal("exception thrown", exception.Message);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task2", results[1]);
        }

        [Fact]
        public async Task OnSuccessVoid_WithTaskNoArg_Left_PriorFailure()
        {
            var results = new List<string>();

            var result = await
                Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>(500);
                })
                .OnSuccessVoid(() => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    results.Add("task2");
                }));

            result.AssertLeft(500);

            Assert.Single(results);
            Assert.Equal("task1", results[0]);
        }

        [Fact]
        public async Task OnSuccessVoid_WithAction()
        {
            var actionResults = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    actionResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid(() => actionResults.Add("task2"))
                .OnSuccessVoid(() => actionResults.Add("task3"));

            result.AssertRight(Unit.Instance);

            Assert.Equal(3, actionResults.Count);
            Assert.Equal("task1", actionResults[0]);
            Assert.Equal("task2", actionResults[1]);
            Assert.Equal("task3", actionResults[2]);
        }

        [Fact]
        public async Task OnSuccessVoid_WithAction_Throws()
        {
            var results = new List<string>();

            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid(() =>
                {
                    results.Add("task2");
                    throw new Exception("exception thrown");
                })
                .OnSuccessVoid(() =>
                {
                    results.Add("task3");
                }));

            Assert.Equal("exception thrown", exception.Message);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task2", results[1]);
        }

        [Fact]
        public async Task OnSuccessVoid_WithAction_PriorFailure()
        {
            var actionResults = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    actionResults.Add("task1");
                    return new Either<int, string>(500);
                })
                .OnSuccessVoid(() => actionResults.Add("task2"));

            result.AssertLeft(500);

            Assert.Equal("task1", Assert.Single(actionResults));
        }

        [Fact]
        public async Task OnSuccessVoid()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid();

            result.AssertRight(Unit.Instance);

            Assert.Equal("task1", Assert.Single(results));
        }

        [Fact]
        public async Task OnSuccessVoid_Throws()
        {
            var results = new List<string>();

            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        results.Add("task1");
                        return new Either<int, string>("task1");
                    })
                    .OnSuccessDo(() =>
                    {
                        results.Add("task2");
                        throw new Exception("exception thrown");
                    })
                    // method under test here
                    .OnSuccessVoid()
                    .OnSuccessVoid(() =>
                    {
                        results.Add("task3");
                    }));

            Assert.Equal("exception thrown", exception.Message);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task2", results[1]);
        }

        [Fact]
        public async Task OnSuccessVoid_WithEitherTask()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid(previousResult =>
                {
                    results.Add(previousResult + " task2");
                    return Task.FromResult(new Either<int, string>("task2"));
                });

            result.AssertRight(Unit.Instance);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task1 task2", results[1]);
        }

        [Fact]
        public async Task OnSuccessVoid_WithEitherTask_Throws()
        {
            var results = new List<string>();

            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        results.Add("task1");
                        return new Either<int, string>("task1");
                    })
                    .OnSuccessVoid(previousResult =>
                    {
                        results.Add(previousResult + " task2");
                        throw new Exception("exception thrown");
#pragma warning disable 162
                        return Task.FromResult(new Either<int, string>("task2"));
#pragma warning restore 162
                    })
                    .OnSuccessVoid(previousResult =>
                    {
                        results.Add(previousResult + " task3");
                        return Task.FromResult(new Either<int, string>("task3"));
                    }));

            Assert.Equal("exception thrown", exception.Message);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task1 task2", results[1]);
        }

        [Fact]
        public async Task OnSuccessVoid_WithEitherTaskNoArg()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid(() =>
                {
                    results.Add("task2");
                    return Task.FromResult(new Either<int, string>("task2"));
                });

            result.AssertRight(Unit.Instance);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task2", results[1]);
        }

        [Fact]
        public async Task OnSuccessVoid_WithEitherTaskNoArg_Throws()
        {
            var results = new List<string>();

            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        results.Add("task1");
                        return new Either<int, string>("task1");
                    })
                    .OnSuccessVoid(() =>
                    {
                        results.Add("task2");
                        throw new Exception("exception thrown");
#pragma warning disable 162
                        return Task.FromResult(new Either<int, string>("task2"));
#pragma warning restore 162
                    })
                    .OnSuccessVoid(() =>
                    {
                        results.Add("task3");
                        return Task.FromResult(new Either<int, string>("task3"));
                    }));

            Assert.Equal("exception thrown", exception.Message);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task2", results[1]);
        }

        [Fact]
        public async Task OnSuccessDo_WithVoid()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessDo(async () =>
                {
                    await Task.Delay(10);
                    results.Add("task2");
                    await Task.Run(() => {});
                });

            result.AssertRight("task1");

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task2", results[1]);
        }

        [Fact]
        public async Task OnSuccessDo_WithEitherTaskNoArg()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessDo(async () => await Task.Run(() =>
                {
                    Task.Delay(20);
                    results.Add("task2");
                    return new Either<int, string>("task2");
                }));

            result.AssertRight("task1");

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task2", results[1]);
        }

        [Fact]
        public async Task OnSuccessDo_WithEitherTaskNoArg_Left()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessDo(async () => await Task.Run(() =>
                {
                    Task.Delay(10);
                    results.Add("task2");
                    return new Either<int, string>(500);
                }))
                .OnSuccessDo(async () => await Task.Run(() =>
                {
                    Task.Delay(20);
                    results.Add("task3");
                    return new Either<int, string>(600);
                }));

            result.AssertLeft(500);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task2", results[1]);
        }

        [Fact]
        public async Task OnSuccessDo_WithEitherTaskNoArg_Left_PriorFailure()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>(500);
                })
                .OnSuccessDo(async () => await Task.Run(() =>
                {
                    Task.Delay(20);
                    results.Add("task2");
                    return new Either<int, string>("task2");
                }));

            result.AssertLeft(500);

            Assert.Equal("task1", Assert.Single(results));
        }

        [Fact]
        public async Task OnSuccessDo_WithEitherNoArg()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessDo(() =>
                {
                    results.Add("task2");
                    return new Either<int, string>("task2");
                });

            result.AssertRight("task1");

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task2", results[1]);
        }

        [Fact]
        public async Task OnSuccessDo_WithEitherNoArg_Left()
        {
            var results = new List<string>();

            var result = await Task.Run(() =>
                {
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessDo(() =>
                {
                    results.Add("task2");
                    return new Either<int, string>(500);
                })
                .OnSuccessDo(() =>
                {
                    results.Add("task3");
                    return new Either<int, string>(600);
                });

            result.AssertLeft(500);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task2", results[1]);
        }

        [Fact]
        public async Task OnSuccessDo_WithEitherNoArg_Left_PriorFailure()
        {
            var results = new List<string>();

            var result = await Task.Run(() =>
                {
                    results.Add("task1");
                    return new Either<int, string>(500);
                })
                .OnSuccessDo(() =>
                {
                    results.Add("task2");
                    return new Either<int, string>("task2");
                });

            result.AssertLeft(500);

            Assert.Equal("task1", Assert.Single(results));
        }

        [Fact]
        public async Task OnSuccessDo_WithTask()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessDo(async previousResult => await Task.Run(() =>
                {
                    Task.Delay(20);
                    results.Add(previousResult + " task2");
                }))
                .OnSuccessDo(async previousResult => await Task.Run(() =>
                {
                    Task.Delay(20);
                    results.Add(previousResult + " task3");
                }));

            result.AssertRight("task1");

            Assert.Equal(3, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task1 task2", results[1]);
            Assert.Equal("task1 task3", results[2]);
        }

        [Fact]
        public async Task OnSuccessDo_WithTask_Throws()
        {
            var results = new List<string>();

            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessDo(async previousResult => await Task.Run(() =>
                {
                    Task.Delay(10);
                    results.Add(previousResult + " task2");
                    throw new Exception("exception thrown");
                }))
                .OnSuccessDo(async previousResult => await Task.Run(() =>
                {
                    Task.Delay(20);
                    results.Add(previousResult + " task3");
                })));

            Assert.Equal("exception thrown", exception.Message);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task1 task2", results[1]);
        }

        [Fact]
        public async Task OnSuccessDo_WithTask_Left_PriorFailure()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>(500);
                })
                .OnSuccessDo(async task1Result => await Task.Run(() =>
                {
                    Task.Delay(20);
                    results.Add(task1Result + " task2");
                }));

            result.AssertLeft(500);

            Assert.Equal("task1", Assert.Single(results));
        }

        [Fact]
        public async Task OnFailureDo_WithTask()
        {
            var result = await Task.FromResult(new Either<int, string>(500))
                .OnFailureDo(_ => Task.FromResult(600));

            result.AssertLeft(500);
        }

        [Fact]
        public async Task OnFailureDo_WithTask_PriorSuccess()
        {
            var result = await Task.FromResult(new Either<int, string>("Success"))
                .OnFailureDo(_ => Task.FromResult(500));

            result.AssertRight("Success");
        }

        [Fact]
        public async Task OnFailureSucceedWith()
        {
            var result = await Task.FromResult(new Either<int, string>(500))
                .OnFailureSucceedWith(_ => Task.FromResult("recovered from failure"));

            result.AssertRight("recovered from failure");
        }

        [Fact]
        public async Task OnFailureSucceedWith_PriorSuccess()
        {
            var result = await Task.FromResult(new Either<int, string>("Success1"))
                .OnFailureSucceedWith(_ => Task.FromResult("Success2"));

            result.AssertRight("Success1");
        }

        [Fact]
        public async Task OnFailureFailWith()
        {
            var result = await Task.FromResult(new Either<int, string>(500))
                .OnFailureFailWith(() => 600);

            result.AssertLeft(600);
        }

        [Fact]
        public async Task OnFailureFailWith_GenericTask()
        {
            var result = await Task.FromResult(new Either<int, string>(500))
                .OnFailureFailWith(_ => Task.FromResult(600));

            result.AssertLeft(600);
        }

        [Fact]
        public async Task OnFailureFailWith_GenericTask_PriorFailure()
        {
            var result = await Task.FromResult(new Either<int, string>("Success"))
                .OnFailureFailWith(_ => Task.FromResult(500));

            result.AssertRight("Success");
        }

        [Fact]
        public async Task OnSuccessCombineWith_AllSuccess()
        {
            var result = await Task.FromResult(new Either<int, string>("Success number one!"))
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                .OnSuccessCombineWith(firstSuccess =>
                {
                    Assert.Equal("Success number one!", firstSuccess);
                    return Task.FromResult(new Either<int, string>("Success number two!"));
                });

            result.AssertRight(TupleOf("Success number one!", "Success number two!"));
        }

        [Fact]
        public async Task OnSuccessCombineWith_AllSuccessMixedTypes()
        {
            var result = await
                Task.FromResult(new Either<int, string>("Success number one!"))
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                .OnSuccessCombineWith(firstSuccess =>
                {
                    Assert.Equal("Success number one!", firstSuccess);
                    return Task.FromResult(new Either<int, char>('2'));
                });

            result.AssertRight(TupleOf("Success number one!", '2'));
        }

        [Fact]
        public async Task OnSuccessCombineWith_FirstFails()
        {
            var result = await Task.FromResult(new Either<int, string>(500))
                .OnSuccessCombineWith(_ =>
                {
                    AssertFail("Second call should not be called if the first failed");
                    return Task.FromResult(new Either<int, string>("this should not be called"));
                });

            result.AssertLeft(500);
        }

        [Fact]
        public async Task OnSuccessCombineWith_SecondFails()
        {
            var result = await Task.FromResult(new Either<int, string>("Success number one!"))
                .OnSuccessCombineWith(_ => Task.FromResult(new Either<int, string>(500)));

            result.AssertLeft(500);
        }

        [Fact]
        public async Task OnSuccessCombineWith_WithEitherTask()
        {
            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    return new Either<int, string>("task1");
                })
                .OnSuccessCombineWith(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    return new Either<int, string>(previousResult + " task2");
                }));

            Assert.True(result.IsRight);
            Assert.Equal("task1", result.Right.Item1);
            Assert.Equal("task1 task2", result.Right.Item2);
        }

        [Fact]
        public async Task OnSuccessCombineWith_WithEitherTask_Left()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessCombineWith(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    results.Add(previousResult + " task2");
                    return new Either<int, string>(500);
                }))
                .OnSuccessCombineWith(previousResult => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    results.Add(previousResult + " task3");
                    return new Either<int, string>(previousResult + " task3");
                }));

            result.AssertLeft(500);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task1 task2", results[1]);
        }

        [Fact]
        public async Task OnSuccessCombineWith_WithEitherTaskTuple3()
        {
            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    return new Either<int, string>("task1");
                })
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                .OnSuccessCombineWith(previousResult => Task.Run(async () =>
                {
                    Assert.Equal("task1", previousResult);
                    await Task.Delay(10);
                    return new Either<int, string>("task2");
                }))
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                .OnSuccessCombineWith(previousResult => Task.Run(async () =>
                {
                    Assert.Equal("task1", previousResult.Item1);
                    Assert.Equal("task2", previousResult.Item2);
                    await Task.Delay(10);
                    return new Either<int, string>("task3");
                }));

            Assert.True(result.IsRight);
            var (value1, value2, value3) = result.Right;
            Assert.Equal("task1", value1);
            Assert.Equal("task2", value2);
            Assert.Equal("task3", value3);
        }

        [Fact]
        public async Task OnSuccessCombineWith_WithEitherTaskTuple3_Left()
        {
            var results = new List<string>();

            var result = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    results.Add("task1");
                    return new Either<int, string>("task1");
                })
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                .OnSuccessCombineWith(previousResult => Task.Run(async () =>
                {
                    Assert.Equal("task1", previousResult);
                    await Task.Delay(10);
                    results.Add("task2");
                    return new Either<int, string>(500);
                }))
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                .OnSuccessCombineWith(previousResult => Task.Run(async () =>
                {
                    Assert.Equal("task1", previousResult.Item1);
                    Assert.Equal("task2", previousResult.Item2);
                    await Task.Delay(0);
                    results.Add("task3");
                    return new Either<int, string>("task3");
                }));

            result.AssertLeft(500);

            Assert.Equal(2, results.Count);
            Assert.Equal("task1", results[0]);
            Assert.Equal("task2", results[1]);
        }

        [Fact]
        public async Task OrElse_EitherTask_FirstSucceeds()
        {
            var result = await Task.FromResult(new Either<int, string>("Success number one!"))
                .OrElse(() =>
                {
                    AssertFail("Second call should not be called if the first succeeds");
                    return Task.FromResult(new Either<int, string>("this should not be called"));
                });

            result.AssertRight("Success number one!");
        }

        [Fact]
        public async Task OrElse_EitherTask_FirstFails()
        {
            var result = await Task.FromResult(new Either<int, string>(500))
                .OrElse(() => Task.FromResult(new Either<int, string>("Success number two!")));

            result.AssertRight("Success number two!");
        }

        [Fact]
        public async Task OrElse_EitherTask_BothFail()
        {
            var result = await Task.FromResult(new Either<int, string>(500))
                .OrElse(() => Task.FromResult(new Either<int, string>(600)));

            result.AssertLeft(600);
        }

        [Fact]
        public async Task OrElse_GenericTaskNoArg_FirstSucceeds()
        {
            var result = await Task.FromResult(new Either<int, string>("Success number one!"))
                .OrElse(() =>
                {
                    AssertFail("Second call should not be called if the first succeeds");
                    return Task.FromResult("this should not be called");
                });

            Assert.Equal("Success number one!", result);
        }

        [Fact]
        public async Task OrElse_GenericTaskNoArg_FirstFails()
        {
            var result = await Task.FromResult(new Either<int, string>(500))
                .OrElse(() => Task.FromResult("Success number two!"));

            Assert.Equal("Success number two!", result);
        }

        [Fact]
        public async Task OrElse_GenericTaskNoArg_BothFail()
        {
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                Task.FromResult(new Either<int, string>(500))
                .OrElse(() =>
                {
                    throw new Exception("exception thrown");
#pragma warning disable 162
                    return "failure";
#pragma warning restore 162
                }));

            Assert.Equal("exception thrown", exception.Message);
        }

        [Fact]
        public async Task OrElse_GenericTaskNoArg_PriorSuccess()
        {
            var result = await
                Task.FromResult(new Either<int, string>("Success1"))
                .OrElse(() => "Success2");

            Assert.Equal("Success1", result);
        }

        [Fact]
        public async Task OnFailureVoid()
        {
            var failures = new List<int>();

            var result = await Task.FromResult(new Either<int, string>(500))
                .OnFailureVoid(
                    failure =>
                    {
                        failures.Add(failure);
                    }
                );

            result.AssertLeft();

            var failure = Assert.Single(failures);
            Assert.Equal(500, failure);
        }

        [Fact]
        public async Task OnFailureVoid_PriorSuccess()
        {
            var failures = new List<int>();

            var result = await Task.FromResult(new Either<int, string>("Success"))
                .OnFailureVoid(
                    failure =>
                    {
                        failures.Add(failure);
                    }
                );

            result.AssertRight("Success");

            Assert.Empty(failures);
        }

        [Fact]
        public async Task OnSuccessAll()
        {
            var eitherTask1 = Task.Run(async () =>
            {
                await Task.Delay(50);
                return new Either<int, string>("task1");
            });
            var eitherTask2 = Task.Run(async () =>
            {
                await Task.Delay(10);
                return new Either<int, string>("task2");
            });
            var eitherTask3 = Task.Run(async () =>
            {
                await Task.Delay(30);
                return new Either<int, string>("task3");
            });

            var eitherList = ListOf(eitherTask1, eitherTask2, eitherTask3);

            var results = await eitherList.OnSuccessAll();

            Assert.True(results.IsRight);
            Assert.Equal(3, results.Right.Count);
            Assert.Equal("task1", results.Right[0]);
            Assert.Equal("task2", results.Right[1]);
            Assert.Equal("task3", results.Right[2]);
        }

        [Fact]
        public async Task OnSuccessAll_Left()
        {
            var eitherTask1 = Task.Run(async () =>
            {
                await Task.Delay(50);
                return new Either<int, string>("task1");
            });
            var eitherTask2 = Task.Run(async () =>
            {
                await Task.Delay(10);
                return new Either<int, string>(500);
            });
            var eitherTask3 = Task.Run(async () =>
            {
                await Task.Delay(30);
                return new Either<int, string>("task3");
            });

            var eitherList = ListOf(eitherTask1, eitherTask2, eitherTask3);

            var result = await eitherList.OnSuccessAll();

            result.AssertLeft(500);
        }
    }
}
