#nullable enable
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Xunit;
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
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async Task OnSuccess_WithRight()
        {
            var either = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccess(_ => new Either<int, string>("another success"));

            Assert.True(either.IsRight);
            Assert.Equal("another success", either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async Task OnSuccess_WithLeft()
        {
            var exception = new Exception("Something went wrong");
            var either = await Task.FromResult(new Either<Exception, string>("a success"))
                .OnSuccess(_ => new Either<Exception, string>(exception));

            Assert.True(either.IsLeft);
            Assert.Equal(exception, either.Left);
            Assert.Throws<ArgumentException>(() => either.Right);
        }

        [Fact]
        public async Task OnSuccess_WithTask()
        {
            var either = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccess(_ => Task.FromResult("another success"));

            Assert.True(either.IsRight);
            Assert.Equal("another success", either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async Task OnSuccessVoid()
        {
            var either = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccessVoid(
                    async _ =>
                    {
                        await Task.FromResult(true);
                    });

            Assert.True(either.IsRight);
            Assert.Equal(Unit.Instance, either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async Task OnSuccess_WithRightTask()
        {
            var either = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccess(_ => Task.FromResult(new Either<int, string>("another success")));

            Assert.True(either.IsRight);
            Assert.Equal("another success", either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async Task OnSuccess_WithLeftTask()
        {
            var exception = new Exception("Something went wrong");
            var either = await Task.FromResult(new Either<Exception, string>("a success"))
                .OnSuccess(_ => Task.FromResult(new Either<Exception, string>(exception)));

            Assert.True(either.IsLeft);
            Assert.Equal(exception, either.Left);
            Assert.Throws<ArgumentException>(() => either.Right);
        }

        [Fact]
        public async Task OnSuccessDo_WithVoid()
        {
            var either = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccessDo(async () =>
                {
                    await Task.Run(() => {});
                });

            Assert.True(either.IsRight);
            Assert.Equal("a success", either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async Task OnSuccessDo_WithRightTask()
        {
            var either = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccessDo(() => Task.FromResult(new Either<int, string>("another success")));

            Assert.True(either.IsRight);
            Assert.Equal("a success", either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async Task OnSuccessDo_WithLeftTask()
        {
            var exception = new Exception("Something went wrong");
            var either = await Task.FromResult(new Either<Exception, string>("a success"))
                .OnSuccessDo(() => Task.FromResult(new Either<Exception, string>(exception)));

            Assert.True(either.IsLeft);
            Assert.Equal(exception, either.Left);
            Assert.Throws<ArgumentException>(() => either.Right);
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
        public async Task OnFailureFailWith()
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
        public async Task OnSuccessCombineWith_AllSuccess()
        {
            var either = await Task.FromResult(new Either<int, string>("Success number one!"))
                .OnSuccessCombineWith(firstSuccess =>
                {
                    Assert.Equal("Success number one!", firstSuccess);
                    return Task.FromResult(new Either<int, string>("Success number two!"));
                });

            Assert.True(either.IsRight);
            Assert.Equal(new Tuple<string, string>("Success number one!", "Success number two!"), either.Right);
        }

        [Fact]
        public async Task OnSuccessCombineWith_AllSuccessMixedTypes()
        {
            var either = await Task.FromResult(new Either<int, string>("Success number one!"))
                .OnSuccessCombineWith(firstSuccess =>
                {
                    Assert.Equal("Success number one!", firstSuccess);
                    return Task.FromResult(new Either<int, char>('2'));
                });

            Assert.True(either.IsRight);
            Assert.Equal(new Tuple<string, char>("Success number one!", '2'), either.Right);
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
        public async Task OrElse_FirstSucceeds()
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
        public async Task OrElse_FirstFails()
        {
            var either = await Task.FromResult(new Either<int, string>(500))
                .OrElse(() => Task.FromResult(new Either<int, string>("Success number two!")));

            Assert.True(either.IsRight);
            Assert.Equal("Success number two!", either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async Task OrElse_BothFail()
        {
            var either = await Task.FromResult(new Either<int, string>(500))
                .OrElse(() => Task.FromResult(new Either<int, string>(600)));

            Assert.True(either.IsLeft);
            Assert.Equal(600, either.Left);
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
    }
}
