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
            var either = new Either<string, int>("a failure");

            Assert.True(either.IsLeft);
            Assert.False(either.IsRight);
            Assert.Equal("a failure", either.Left);
            Assert.Throws<ArgumentException>(() => either.Right);
        }

        [Fact]
        public void Right()
        {
            var either = new Either<int, string>("a success");

            Assert.True(either.IsRight);
            Assert.False(either.IsLeft);
            Assert.Equal("a success", either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
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
            var either =
                new Either<int, string>("either1")
                    .OnSuccess(either1Result => either1Result + " either2");
            
            Assert.True(either.IsRight);
            Assert.Equal("either1 either2", either.Right);
        }

        [Fact]
        public void OnSuccess_Left_PriorFailure()
        {
            var eitherResults = new List<string>();
            
            var either = 
                new Either<int, string>(500)
                    .OnSuccess(previousResult =>
                    {
                        eitherResults.Add(previousResult + " either2");
                        return previousResult + "either2";
                    });
            
            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);
            
            Assert.Empty(eitherResults);
        }
        
        [Fact]
        public void OnSuccess_NoArg()
        {
            var either =
                new Either<int, string>("either1")
                    .OnSuccess(() => "either2");
            
            Assert.True(either.IsRight);
            Assert.Equal("either2", either.Right);
        }

        [Fact]
        public void OnSuccess_NoArg_Left_PriorFailure()
        {
            var eitherResults = new List<string>();
            
            var either = 
                new Either<int, string>(500)
                    .OnSuccess(() =>
                    {
                        eitherResults.Add("either2");
                        return "either2";
                    });
            
            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);
            
            Assert.Empty(eitherResults);
        }

        [Fact]
        public async Task OnSuccess_WithEitherTask()
        {
            var either =
                await new Either<int, string>("either1")
                    .OnSuccess(either1Result => 
                        Task.FromResult(new Either<int, string>(either1Result + " either2")));
            
            Assert.True(either.IsRight);
            Assert.Equal("either1 either2", either.Right);
        }

        [Fact]
        public void OrElse_NoArg()
        {
            var either =
                new Either<int, string>(500)
                    .OrElse(() => "either2");
            
            Assert.True(either.IsRight);
            Assert.Equal("either2", either.Right);
        }

        [Fact]
        public void OrElse_NoArg_Left_PriorSuccess()
        {
            var eitherResults = new List<string>();
            
            var either = 
                new Either<int, string>("either1")
                    .OrElse(() =>
                    {
                        eitherResults.Add("either2");
                        return "either2";
                    });
            
            Assert.True(either.IsRight);
            Assert.Equal("either1", either.Right);
            
            Assert.Empty(eitherResults);
        }

        [Fact]
        public async Task OrElse_WithEitherTask()
        {
            var either =
                await new Either<int, string>("either1")
                    .OnSuccess(either1Result => 
                        Task.FromResult(new Either<int, string>(either1Result + " either2")));
            
            Assert.True(either.IsRight);
            Assert.Equal("either1 either2", either.Right);
        }

        [Fact]
        public async Task OnSuccess_WithEitherTask_Left()
        {
            var taskResults = new List<string>();
            
            var either =
                await new Either<int, string>("either1")
                    .OnSuccess(previousResult => Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        taskResults.Add(previousResult + " task1");
                        return new Either<int, string>(500);
                    }))
                    .OnSuccess(previousResult => Task.Run(async () =>
                    {
                        await Task.Delay(10);
                        taskResults.Add(previousResult + " task2");
                        return new Either<int, string>(previousResult + " task2");
                    }));
            
            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);
            
            Assert.Equal("either1 task1", Assert.Single(taskResults));
        }
        
        [Fact]
        public async Task OnSuccess_WithEitherTask_Left_PriorFailure()
        {
            var taskResults = new List<string>();
            
            var either =
                await new Either<int, string>(500)
                    .OnSuccess(previousResult => Task.Run(() =>
                    {
                        taskResults.Add(previousResult + " task1");
                        return new Either<int, string>(600);
                    }));
            
            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);
            
            Assert.Empty(taskResults);
        }
    }

    public class EitherTaskTest
    {
        [Fact]
        public async Task OnSuccess()
        {
            var either = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccess(str => str.ToUpper());

            Assert.True(either.IsRight);
            Assert.Equal("A SUCCESS", either.Right);
        }

        [Fact]
        public async Task OnSuccess_WithRight()
        {
            var either = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccess(_ => new Either<int, string>("another success"));

            Assert.True(either.IsRight);
            Assert.Equal("another success", either.Right);
        }

        [Fact]
        public async Task OnSuccess_WithLeft()
        {
            var exception = new Exception("Something went wrong");
            var either = await Task.FromResult(new Either<Exception, string>("a success"))
                .OnSuccess(_ => new Either<Exception, string>(exception));

            Assert.True(either.IsLeft);
            Assert.Equal(exception, either.Left);
        }

#pragma warning disable 618
        [Fact]
        public async Task OnSuccess_WithTask()
        {
            var taskResults = new List<string>();
            
            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(5);
                    taskResults.Add(previousTaskResult + " task2");
                }))
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                .OnSuccess(previousTaskResult => Task.Run(async () =>
                {
                    Assert.IsType<Unit>(previousTaskResult);
                    await Task.Delay(15);
                    taskResults.Add("task3");
                }));

            Assert.True(either.IsRight);
            Assert.Equal(3, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task1 task2", taskResults[1]);
            Assert.Equal("task3", taskResults[2]);
        }
        
        [Fact]
        public async Task OnSuccess_WithTask_Left()
        {
            var taskResults = new List<string>();
            
            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(5);
                    taskResults.Add(previousTaskResult + " task2");
                    throw new ArgumentException("task2 failed");
                }))
                .OnSuccess(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(15);
                    taskResults.Add(previousTaskResult + " task3");
                })));

            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task1 task2", taskResults[1]);
        }
#pragma warning restore 618
        
        [Fact]
        public async Task OnSuccess_WithGenericTask()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add(previousTaskResult + " task2");
                    return previousTaskResult + " task2";
                }))
                .OnSuccess(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    taskResults.Add(previousTaskResult + " task3");
                    return previousTaskResult + " task3";
                }));

            Assert.True(either.IsRight);
            Assert.Equal("task1 task2 task3", either.Right);

            Assert.Equal(3, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task1 task2", taskResults[1]);
            Assert.Equal("task1 task2 task3", taskResults[2]);
        }
        
        [Fact]
        public async Task OnSuccess_WithGenericTask_Left()
        {
            var taskResults = new List<string>();

            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add(previousTaskResult + " task2");
                    throw new ArgumentException("task2 failed");
#pragma warning disable 162
                    return previousTaskResult + " task2";
#pragma warning restore 162
                }))
                .OnSuccess(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    taskResults.Add(previousTaskResult + " task3");
                    return previousTaskResult + " task3";
                })));

            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task1 task2", taskResults[1]);
        }

        [Fact]
        public async Task OnSuccess_WithGenericTaskNoArg()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add("task2");
                    return "task2";
                }))
                .OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    taskResults.Add("task3");
                    return "task3";
                }));

            Assert.True(either.IsRight);
            Assert.Equal("task3", either.Right);

            Assert.Equal(3, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task2", taskResults[1]);
            Assert.Equal("task3", taskResults[2]);
        }
        
        [Fact]
        public async Task OnSuccess_WithGenericTaskNoArg_Left()
        {
            var taskResults = new List<string>();

            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add("task2");
                    throw new ArgumentException("task2 failed");
#pragma warning disable 162
                    return "task2";
#pragma warning restore 162
                }))
                .OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    taskResults.Add("task3");
                    return "task3";
                })));

            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task2", taskResults[1]);
        }

        [Fact]
        public async Task OnSuccess_WithGenericTaskNoArg_Left_PriorFailure()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>(500);
                })
                .OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add("task2");
                    return "task2";
                }));

            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);

            Assert.Equal("task1", Assert.Single(taskResults));
        }

        [Fact]
        public async Task OnSuccess_WithEitherTask()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add(previousTaskResult + " task2");
                    return new Either<int, string>(previousTaskResult + " task2");
                }))
                .OnSuccess(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    taskResults.Add(previousTaskResult + " task3");
                    return new Either<int, string>(previousTaskResult + " task3");
                }));

            Assert.True(either.IsRight);
            Assert.Equal("task1 task2 task3", either.Right);

            Assert.Equal(3, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task1 task2", taskResults[1]);
            Assert.Equal("task1 task2 task3", taskResults[2]);
        }
        
        [Fact]
        public async Task OnSuccess_WithEitherTask_Left()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add(previousTaskResult + " task2");
                    return new Either<int, string>(500);
                }))
                .OnSuccess(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    taskResults.Add(previousTaskResult + " task3");
                    return new Either<int, string>(previousTaskResult + " task3");
                }));

            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);
            
            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task1 task2", taskResults[1]);
        }

        [Fact]
        public async Task OnSuccess_WithEitherTaskNoArg()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add("task2");
                    return new Either<int, string>("task2");
                })).OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    taskResults.Add("task3");
                    return new Either<int, string>("task3");
                }));

            Assert.True(either.IsRight);
            Assert.Equal("task3", either.Right);

            Assert.Equal(3, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task2", taskResults[1]);
            Assert.Equal("task3", taskResults[2]);
        }
        
        [Fact]
        public async Task OnSuccess_WithEitherTaskNoArg_Left()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add("task2");
                    return new Either<int, string>(500);
                }))
                .OnSuccess(() => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    taskResults.Add("task3");
                    return new Either<int, string>("task3");
                }));

            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);
            
            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task2", taskResults[1]);
        }
        
        [Fact]
        public async Task OnSuccessVoid_WithTask()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add(previousTaskResult + " task2");
                }))
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                .OnSuccessVoid(previousTaskResult => Task.Run(async () =>
                {
                    Assert.IsType<Unit>(previousTaskResult);
                    await Task.Delay(0);
                    taskResults.Add("task3");
                }));

            Assert.True(either.IsRight);
            Assert.Equal(Unit.Instance, either.Right);

            Assert.Equal(3, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task1 task2", taskResults[1]);
            Assert.Equal("task3", taskResults[2]);
        }
        
        [Fact]
        public async Task OnSuccessVoid_WithTask_Left()
        {
            var taskResults = new List<string>();

            await Assert.ThrowsAsync<ArgumentException>(async () => await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add(previousTaskResult + " task2");
                    throw new ArgumentException("task2 failed");
                }))
                .OnSuccessVoid(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    taskResults.Add(previousTaskResult + " task3");
                })));

            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task1 task2", taskResults[1]);
        }
        
        
        [Fact]
        public async Task OnSuccessVoid_WithTask_Left_PriorFailure()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>(500);
                })
                .OnSuccessVoid(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add(previousTaskResult + " task2");
                }));

            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);

            Assert.Equal("task1", Assert.Single(taskResults));
        }
        
        [Fact]
        public async Task OnSuccessVoid_WithTaskNoArg()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid(() => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add("task2");
                }))
                .OnSuccessVoid(() => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    taskResults.Add("task3");
                }));

            Assert.True(either.IsRight);
            Assert.Equal(Unit.Instance, either.Right);

            Assert.Equal(3, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task2", taskResults[1]);
            Assert.Equal("task3", taskResults[2]);
        }
        
        [Fact]
        public async Task OnSuccessVoid_WithTaskNoArg_Left()
        {
            var taskResults = new List<string>();

            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid(() => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add("task2");
                    throw new ArgumentException("task2 failed");
                }))
                .OnSuccessVoid(() => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    taskResults.Add("task3");
                })));

            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task2", taskResults[1]);
        }
        
        [Fact]
        public async Task OnSuccessVoid_WithTaskNoArg_Left_PriorFailure()
        {
            var taskResults = new List<string>();

            var either = await 
                Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>(500);
                })
                .OnSuccessVoid(() => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add("task2");
                }));

            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);
            
            Assert.Single(taskResults);
            Assert.Equal("task1", taskResults[0]);
        }
        
        [Fact]
        public async Task OnSuccessVoid_WithAction()
        {
            var actionResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    actionResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid(() => actionResults.Add("task2"))
                .OnSuccessVoid(() => actionResults.Add("task3"));

            Assert.True(either.IsRight);
            Assert.Equal(Unit.Instance, either.Right);

            Assert.Equal(3, actionResults.Count);
            Assert.Equal("task1", actionResults[0]);
            Assert.Equal("task2", actionResults[1]);
            Assert.Equal("task3", actionResults[2]);
        }
        
        [Fact]
        public async Task OnSuccessVoid_WithAction_Left()
        {
            var taskResults = new List<string>();

            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid(() => 
                {
                    taskResults.Add("task2");
                    throw new ArgumentException("task2 failed");
                })
                .OnSuccessVoid(() =>
                {
                    taskResults.Add("task3");
                }));

            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task2", taskResults[1]);
        }
        
        [Fact]
        public async Task OnSuccessVoid_WithAction_PriorFailure()
        {
            var actionResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    actionResults.Add("task1");
                    return new Either<int, string>(500);
                })
                .OnSuccessVoid(() => actionResults.Add("task2"));

            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);

            Assert.Equal("task1", Assert.Single(actionResults));
        }

        [Fact]
        public async Task OnSuccessVoid()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid();

            Assert.True(either.IsRight);
            Assert.Equal(Unit.Instance, either.Right);

            Assert.Equal("task1", Assert.Single(taskResults));
        }
        
        [Fact]
        public async Task OnSuccessVoid_Left()
        {
            var taskResults = new List<string>();

            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        taskResults.Add("task1");
                        return new Either<int, string>("task1");
                    })
                    .OnSuccessDo(() =>
                    {
                        taskResults.Add("task2");
                        throw new ArgumentException();
                    })
                    // method under test here
                    .OnSuccessVoid()
                    .OnSuccessVoid(() =>
                    {
                        taskResults.Add("task3");
                    }));

            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task2", taskResults[1]);
        }
        
        [Fact]
        public async Task OnSuccessVoid_WithEitherTask()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid(previousTaskResult =>
                {
                    taskResults.Add(previousTaskResult + " task2");
                    return Task.FromResult(new Either<int, string>("task2"));
                });

            Assert.True(either.IsRight);
            Assert.Equal(Unit.Instance, either.Right);

            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task1 task2", taskResults[1]);
        }
        
        [Fact]
        public async Task OnSuccessVoid_WithEitherTask_Left()
        {
            var taskResults = new List<string>();

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        taskResults.Add("task1");
                        return new Either<int, string>("task1");
                    })
                    .OnSuccessVoid(previousTaskResult =>
                    {
                        taskResults.Add(previousTaskResult + " task2");
                        throw new ArgumentException();
#pragma warning disable 162
                        return Task.FromResult(new Either<int, string>("task2"));
#pragma warning restore 162
                    })
                    .OnSuccessVoid(previousTaskResult =>
                    {
                        taskResults.Add(previousTaskResult + " task3");
                        return Task.FromResult(new Either<int, string>("task3"));
                    }));

            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task1 task2", taskResults[1]);
        }
        
        [Fact]
        public async Task OnSuccessVoid_WithEitherTaskNoArg()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessVoid(() =>
                {
                    taskResults.Add("task2");
                    return Task.FromResult(new Either<int, string>("task2"));
                });

            Assert.True(either.IsRight);
            Assert.Equal(Unit.Instance, either.Right);

            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task2", taskResults[1]);
        }
        
        [Fact]
        public async Task OnSuccessVoid_WithEitherTaskNoArg_Left()
        {
            var taskResults = new List<string>();

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        taskResults.Add("task1");
                        return new Either<int, string>("task1");
                    })
                    .OnSuccessVoid(() =>
                    {
                        taskResults.Add("task2");
                        throw new ArgumentException();
#pragma warning disable 162
                        return Task.FromResult(new Either<int, string>("task2"));
#pragma warning restore 162
                    })
                    .OnSuccessVoid(() =>
                    {
                        taskResults.Add("task3");
                        return Task.FromResult(new Either<int, string>("task3"));
                    }));

            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task2", taskResults[1]);
        }
        
        [Fact]
        public async Task OnSuccessDo_WithVoid()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessDo(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add("task2");
                    await Task.Run(() => {});
                });

            Assert.True(either.IsRight);
            Assert.Equal("task1", either.Right);

            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task2", taskResults[1]);
        }

        [Fact]
        public async Task OnSuccessDo_WithEitherTaskNoArg()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessDo(async () => await Task.Run(() =>
                {
                    Task.Delay(20);
                    taskResults.Add("task2");
                    return new Either<int, string>("task2");
                }));
            
            Assert.True(either.IsRight);
            Assert.Equal("task1", either.Right);

            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task2", taskResults[1]);
        }

        [Fact]
        public async Task OnSuccessDo_WithEitherTaskNoArg_Left()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessDo(async () => await Task.Run(() =>
                {
                    Task.Delay(10);
                    taskResults.Add("task2");
                    return new Either<int, string>(500);
                }))
                .OnSuccessDo(async () => await Task.Run(() =>
                {
                    Task.Delay(20);
                    taskResults.Add("task3");
                    return new Either<int, string>(600);
                }));

            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);

            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task2", taskResults[1]);
        }

        [Fact]
        public async Task OnSuccessDo_WithEitherTaskNoArg_Left_PriorFailure()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>(500);
                })
                .OnSuccessDo(async () => await Task.Run(() =>
                {
                    Task.Delay(20);
                    taskResults.Add("task2");
                    return new Either<int, string>("task2");
                }));
            
            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);

            Assert.Equal("task1", Assert.Single(taskResults));
        }

        [Fact]
        public async Task OnSuccessDo_WithTask()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessDo(async task1Results => await Task.Run(() =>
                {
                    Task.Delay(20);
                    taskResults.Add(task1Results + " task2");
                }))
                .OnSuccessDo(async task1Results => await Task.Run(() =>
                {
                    Task.Delay(20);
                    taskResults.Add(task1Results + " task3");
                }));
            
            Assert.True(either.IsRight);
            Assert.Equal("task1", either.Right);

            Assert.Equal(3, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task1 task2", taskResults[1]);
            Assert.Equal("task1 task3", taskResults[2]);
        }

        [Fact]
        public async Task OnSuccessDo_WithTask_Left()
        {
            var taskResults = new List<string>();

            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessDo(async task1Results => await Task.Run(() =>
                {
                    Task.Delay(10);
                    taskResults.Add(task1Results + " task2");
                    throw new ArgumentException();
                }))
                .OnSuccessDo(async task1Results => await Task.Run(() =>
                {
                    Task.Delay(20);
                    taskResults.Add(task1Results + " task3");
                })));

            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task1 task2", taskResults[1]);
        }

        [Fact]
        public async Task OnSuccessDo_WithTask_Left_PriorFailure()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>(500);
                })
                .OnSuccessDo(async task1Result => await Task.Run(() =>
                {
                    Task.Delay(20);
                    taskResults.Add(task1Result + " task2");
                }));
            
            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);

            Assert.Equal("task1", Assert.Single(taskResults));
        }

        [Fact]
        public async Task OnFailureDo_WithTask()
        {
            var exception = new Exception("Something went wrong");
            var either = await Task.FromResult(new Either<Exception, string>(exception))
                .OnFailureDo(_ => Task.FromResult(new Exception("Another failure!")));

            Assert.True(either.IsLeft);
            Assert.Equal(exception, either.Left);
            Assert.Throws<ArgumentException>(() => either.Right);
        }

        [Fact]
        public async Task OnFailureDo_WithTask_PriorSuccess()
        {
            var either = await Task.FromResult(new Either<Exception, string>("Success"))
                .OnFailureDo(_ => Task.FromResult(new Exception("Something went wrong")));

            Assert.True(either.IsRight);
            Assert.Equal("Success", either.Right);
        }

        [Fact]
        public async Task OnFailureSucceedWith()
        {
            var exception = new Exception("Something went wrong");
            var either = await Task.FromResult(new Either<Exception, string>(exception))
                .OnFailureSucceedWith(_ => Task.FromResult("recovered from failure"));

            Assert.True(either.IsRight);
            Assert.Equal("recovered from failure", either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async Task OnFailureSucceedWith_PriorSuccess()
        {
            var either = await Task.FromResult(new Either<Exception, string>("Success1"))
                .OnFailureSucceedWith(_ => Task.FromResult("Success2"));

            Assert.True(either.IsRight);
            Assert.Equal("Success1", either.Right);
        }
        
        [Fact]
        public async Task OnFailureFailWith()
        {
            var exception = new Exception("Something went wrong");
            var nextException = new Exception("Another failure!");

            var either = await Task.FromResult(new Either<Exception, string>(exception))
                .OnFailureFailWith(() => nextException);

            Assert.True(either.IsLeft);
            Assert.Equal(nextException, either.Left);
            Assert.Throws<ArgumentException>(() => either.Right);
        }
        
        [Fact]
        public async Task OnFailureFailWith_GenericTask()
        {
            var exception = new Exception("Something went wrong");
            var nextException = new Exception("Another failure!");

            var either = await Task.FromResult(new Either<Exception, string>(exception))
                .OnFailureFailWith(_ => Task.FromResult(nextException));

            Assert.True(either.IsLeft);
            Assert.Equal(nextException, either.Left);
            Assert.Throws<ArgumentException>(() => either.Right);
        }
        
        [Fact]
        public async Task OnFailureFailWith_GenericTask_PriorFailure()
        {
            var either = await Task.FromResult(new Either<Exception, string>("Success"))
                .OnFailureFailWith(_ => Task.FromResult(new Exception("Something went wrong")));

            Assert.True(either.IsRight);
            Assert.Equal("Success", either.Right);
        }

        [Fact]
        public async Task OnSuccessCombineWith_AllSuccess()
        {
            var either = await Task.FromResult(new Either<int, string>("Success number one!"))
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                .OnSuccessCombineWith(firstSuccess =>
                {
                    Assert.Equal("Success number one!", firstSuccess);
                    return Task.FromResult(new Either<int, string>("Success number two!"));
                });

            Assert.True(either.IsRight);
            Assert.Equal(TupleOf("Success number one!", "Success number two!"), either.Right);
        }

        [Fact]
        public async Task OnSuccessCombineWith_AllSuccessMixedTypes()
        {
            var either = await Task.FromResult(new Either<int, string>("Success number one!"))
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                .OnSuccessCombineWith(firstSuccess =>
                {
                    Assert.Equal("Success number one!", firstSuccess);
                    return Task.FromResult(new Either<int, char>('2'));
                });

            Assert.True(either.IsRight);
            Assert.Equal(TupleOf("Success number one!", '2'), either.Right);
        }

        [Fact]
        public async Task OnSuccessCombineWith_FirstFails()
        {
            var either = await Task.FromResult(new Either<int, string>(500))
                .OnSuccessCombineWith(_ =>
                {
                    AssertFail("Second call should not be called if the first failed");
                    return Task.FromResult(new Either<int, string>("this should not be called"));
                });

            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);
        }

        [Fact]
        public async Task OnSuccessCombineWith_SecondFails()
        {
            var either = await Task.FromResult(new Either<int, string>("Success number one!"))
                .OnSuccessCombineWith(_ => Task.FromResult(new Either<int, string>(500)));

            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);
        }
        
        [Fact]
        public async Task OnSuccessCombineWith_WithEitherTask()
        {
            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    return new Either<int, string>("task1");
                })
                .OnSuccessCombineWith(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    return new Either<int, string>(previousTaskResult + " task2");
                }));

            Assert.True(either.IsRight);
            Assert.Equal("task1", either.Right.Item1);
            Assert.Equal("task1 task2", either.Right.Item2);
        }
        
        [Fact]
        public async Task OnSuccessCombineWith_WithEitherTask_Left()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                .OnSuccessCombineWith(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(10);
                    taskResults.Add(previousTaskResult + " task2");
                    return new Either<int, string>(500);
                }))
                .OnSuccessCombineWith(previousTaskResult => Task.Run(async () =>
                {
                    await Task.Delay(0);
                    taskResults.Add(previousTaskResult + " task3");
                    return new Either<int, string>(previousTaskResult + " task3");
                }));

            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);
            
            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task1 task2", taskResults[1]);
        }
        
        [Fact]
        public async Task OnSuccessCombineWith_WithEitherTaskTuple3()
        {
            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    return new Either<int, string>("task1");
                })
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                .OnSuccessCombineWith(previousTaskResult => Task.Run(async () =>
                {
                    Assert.Equal("task1", previousTaskResult);
                    await Task.Delay(10);
                    return new Either<int, string>("task2");
                }))
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                .OnSuccessCombineWith(previousTaskResult => Task.Run(async () =>
                {
                    Assert.Equal("task1", previousTaskResult.Item1);
                    Assert.Equal("task2", previousTaskResult.Item2);
                    await Task.Delay(10);
                    return new Either<int, string>("task3");
                }));

            Assert.True(either.IsRight);
            var (value1, value2, value3) = either.Right;
            Assert.Equal("task1", value1);
            Assert.Equal("task2", value2);
            Assert.Equal("task3", value3);
        }
        
        [Fact]
        public async Task OnSuccessCombineWith_WithEitherTaskTuple3_Left()
        {
            var taskResults = new List<string>();

            var either = await Task.Run(async () =>
                {
                    await Task.Delay(20);
                    taskResults.Add("task1");
                    return new Either<int, string>("task1");
                })
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                .OnSuccessCombineWith(previousTaskResult => Task.Run(async () =>
                {
                    Assert.Equal("task1", previousTaskResult);
                    await Task.Delay(10);
                    taskResults.Add("task2");
                    return new Either<int, string>(500);
                }))
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                .OnSuccessCombineWith(previousTaskResult => Task.Run(async () =>
                {
                    Assert.Equal("task1", previousTaskResult.Item1);
                    Assert.Equal("task2", previousTaskResult.Item2);
                    await Task.Delay(0);
                    taskResults.Add("task3");
                    return new Either<int, string>("task3");
                }));

            Assert.True(either.IsLeft);
            Assert.Equal(500, either.Left);
            
            Assert.Equal(2, taskResults.Count);
            Assert.Equal("task1", taskResults[0]);
            Assert.Equal("task2", taskResults[1]);
        }

        [Fact]
        public async Task OrElse_EitherTask_FirstSucceeds()
        {
            var either = await Task.FromResult(new Either<int, string>("Success number one!"))
                .OrElse(() =>
                {
                    AssertFail("Second call should not be called if the first succeeds");
                    return Task.FromResult(new Either<int, string>("this should not be called"));
                });

            Assert.True(either.IsRight);
            Assert.Equal("Success number one!", either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async Task OrElse_EitherTask_FirstFails()
        {
            var either = await Task.FromResult(new Either<int, string>(500))
                .OrElse(() => Task.FromResult(new Either<int, string>("Success number two!")));

            Assert.True(either.IsRight);
            Assert.Equal("Success number two!", either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async Task OrElse_EitherTask_BothFail()
        {
            var either = await Task.FromResult(new Either<int, string>(500))
                .OrElse(() => Task.FromResult(new Either<int, string>(600)));

            Assert.True(either.IsLeft);
            Assert.Equal(600, either.Left);
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
            await Assert.ThrowsAsync<ArgumentException>(() =>
                Task.FromResult(new Either<int, string>(500))
                .OrElse(() =>
                {
                    throw new ArgumentException();
#pragma warning disable 162
                    return "failure";
#pragma warning restore 162
                }));
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

            var either = await Task.FromResult(new Either<int, string>(500))
                .OnFailureVoid(
                    failure =>
                    {
                        failures.Add(failure);
                    }
                );

            either.AssertLeft();

            var failure = Assert.Single(failures);
            Assert.Equal(500, failure);
        }

        [Fact]
        public async Task OnFailureVoid_PriorSuccess()
        {
            var failures = new List<int>();

            var either = await Task.FromResult(new Either<int, string>("Success"))
                .OnFailureVoid(
                    failure =>
                    {
                        failures.Add(failure);
                    }
                );

            either.AssertRight("Success");

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

            var results = await eitherList.OnSuccessAll();
            
            Assert.True(results.IsLeft);
            Assert.Equal(500, results.Left);
        }
    }
}
