using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Model
{
    public class EitherTests
    {
        [Fact]
        public void TestLeft()
        {
            var left = new Either<string, int>("a failure");
            Assert.True(left.IsLeft);
            Assert.Equal("a failure", left.Left);
            Assert.Throws<ArgumentException>( () => left.Right);
        }
        
        [Fact]
        public void TestRight()
        {
            var right = new Either<int, string>("a success");
            Assert.False(right.IsLeft);
            Assert.Equal("a success", right.Right);
            Assert.Throws<ArgumentException>( () => right.Left);
        }
    }
}