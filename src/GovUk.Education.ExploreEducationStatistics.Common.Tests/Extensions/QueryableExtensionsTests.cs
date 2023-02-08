#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class QueryableExtensions
{
    public class FirstOrNotFound
    {
        [Fact]
        public async Task NotFound_Empty()
        {
            var result = await new List<string>()
                .AsQueryable()
                .FirstOrNotFound();

            result.AssertNotFound();
        }

        [Fact]
        public async Task NotFound_Predicate()
        {
            var result = await ListOf("me")
                .AsQueryable()
                .FirstOrNotFound(s => s == "you");

            result.AssertNotFound();
        }

        [Fact]
        public async Task Found_First()
        {
            var result = await ListOf("me")
                .AsQueryable()
                .FirstOrNotFound();

            result.AssertRight(expected: "me");
        }

        [Fact]
        public async Task Found_First_Multiple()
        {
            var result = await ListOf("me", "you")
                .AsQueryable()
                .FirstOrNotFound();

            result.AssertRight(expected: "me");
        }

        [Fact]
        public async Task Found_Predicate()
        {
            var result = await ListOf("you", "me")
                .AsQueryable()
                .FirstOrNotFound(s => s == "me");

            result.AssertRight(expected: "me");
        }
    }

    public class SingleOrNotFound
    {
        [Fact]
        public async Task NotFound_Empty()
        {
            var result = await new List<string>()
                .AsQueryable()
                .SingleOrNotFound();

            result.AssertNotFound();
        }

        [Fact]
        public async Task NotFound_Predicate()
        {
            var result = await ListOf("me")
                .AsQueryable()
                .SingleOrNotFound(s => s == "you");

            result.AssertNotFound();
        }

        [Fact]
        public async Task Found_First()
        {
            var result = await ListOf("me")
                .AsQueryable()
                .SingleOrNotFound();

            result.AssertRight(expected: "me");
        }

        [Fact]
        public async Task Found_Multiple_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => ListOf("me", "you")
                    .AsQueryable()
                    .SingleOrNotFound()
            );
        }

        [Fact]
        public async Task Found_Predicate()
        {
            var result = await ListOf("you", "me")
                .AsQueryable()
                .SingleOrNotFound(s => s == "me");

            result.AssertRight(expected: "me");
        }

        [Fact]
        public async Task Found_Predicate_Duplicates_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => ListOf("you", "me", "you", "me")
                    .AsQueryable()
                    .SingleOrNotFound(s => s == "me")
            );
        }
    }
}
