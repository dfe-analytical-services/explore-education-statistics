using System.Threading.Tasks;
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Xunit;

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
    }

    public class EitherTaskTest
    {
        [Fact]
        public async void OnSuccess()
        {
            var either = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccess(str => str.ToUpper());

            Assert.True(either.IsRight);
            Assert.Equal("A SUCCESS", either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async void OnSuccess_WithRight()
        {
            var either = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccess(str => new Either<int, string>("another success"));

            Assert.True(either.IsRight);
            Assert.Equal("another success", either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async void OnSuccess_WithLeft()
        {
            var exception = new Exception("Something went wrong");
            var either = await Task.FromResult(new Either<Exception, string>("a success"))
                .OnSuccess(str => new Either<Exception, string>(exception));

            Assert.True(either.IsLeft);
            Assert.Equal(exception, either.Left);
            Assert.Throws<ArgumentException>(() => either.Right);
        }

        [Fact]
        public async void OnSuccess_WithTask()
        {
            var either = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccess(str => Task.FromResult("another success"));

            Assert.True(either.IsRight);
            Assert.Equal("another success", either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async void OnSuccess_WithVoidTask()
        {
            var either = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccess(
                    async str =>
                    {
                        await Task.FromResult(true);
                    });

            Assert.True(either.IsRight);
            Assert.Equal(Unit.Instance, either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async void OnSuccess_WithRightTask()
        {
            var either = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccess(str => Task.FromResult(new Either<int, string>("another success")));

            Assert.True(either.IsRight);
            Assert.Equal("another success", either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async void OnSuccess_WithLeftTask()
        {
            var exception = new Exception("Something went wrong");
            var either = await Task.FromResult(new Either<Exception, string>("a success"))
                .OnSuccess(str => Task.FromResult(new Either<Exception, string>(exception)));

            Assert.True(either.IsLeft);
            Assert.Equal(exception, either.Left);
            Assert.Throws<ArgumentException>(() => either.Right);
        }

        [Fact]
        public async void OnSuccessDo_WithVoid()
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
        public async void OnSuccessDo_WithRightTask()
        {
            var either = await Task.FromResult(new Either<int, string>("a success"))
                .OnSuccessDo(() => Task.FromResult(new Either<int, string>("another success")));

            Assert.True(either.IsRight);
            Assert.Equal("a success", either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async void OnSuccessDo_WithLeftTask()
        {
            var exception = new Exception("Something went wrong");
            var either = await Task.FromResult(new Either<Exception, string>("a success"))
                .OnSuccessDo(() => Task.FromResult(new Either<Exception, string>(exception)));

            Assert.True(either.IsLeft);
            Assert.Equal(exception, either.Left);
            Assert.Throws<ArgumentException>(() => either.Right);
        }

        [Fact]
        public async void OnFailureDo_WithTask()
        {
            var exception = new Exception("Something went wrong");
            var either = await Task.FromResult(new Either<Exception, string>(exception))
                .OnFailureDo(_ => Task.FromResult(new Exception("Another failure!")));

            Assert.True(either.IsLeft);
            Assert.Equal(exception, either.Left);
            Assert.Throws<ArgumentException>(() => either.Right);
        }

        [Fact]
        public async void OnFailureSucceedWith()
        {
            var exception = new Exception("Something went wrong");
            var either = await Task.FromResult(new Either<Exception, string>(exception))
                .OnFailureSucceedWith(_ => Task.FromResult("recovered from failure"));

            Assert.True(either.IsRight);
            Assert.Equal("recovered from failure", either.Right);
            Assert.Throws<ArgumentException>(() => either.Left);
        }

        [Fact]
        public async void OnFailureFailWith()
        {
            var exception = new Exception("Something went wrong");
            var nextException = new Exception("Another failure!");

            var either = await Task.FromResult(new Either<Exception, string>(exception))
                .OnFailureFailWith(_ => Task.FromResult(nextException));

            Assert.True(either.IsLeft);
            Assert.Equal(nextException, either.Left);
            Assert.Throws<ArgumentException>(() => either.Right);
        }
    }
}