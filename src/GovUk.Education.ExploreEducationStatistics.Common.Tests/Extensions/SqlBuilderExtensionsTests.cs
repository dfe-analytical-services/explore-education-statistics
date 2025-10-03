#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using InterpolatedSql;
using InterpolatedSql.SqlBuilders;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public abstract class SqlBuilderExtensionsTests
{
    private const string ParamStr = "test";
    private const int ParamInt = 10;
    private const bool ParamBool = true;

    public class AppendRangeTests : SqlBuilderExtensionsTests
    {
        [Theory]
        [InlineData("some_field", "some_field")]
        [InlineData("some_field = true", "some_field = true")]
        [InlineData("some_field = true", "some_field", " = ", "true")]
        [InlineData("where some_field = true", "where", "some_field = true")]
        [InlineData("(where some_field = true)", "(", "where", "some_field = true", ")")]
        public void LiteralsWithoutJoin(string expectedSql, params string[] literals)
        {
            var builder = new SqlBuilder();

            builder.AppendRange(literals);

            var sql = builder.Build();

            Assert.Equal(expectedSql, sql.Sql);
            Assert.Empty(sql.SqlParameters);
        }

        [Theory]
        [InlineData("a, list, of, things", ", ", "a", "list", "of", "things")]
        [InlineData("a, b, , d", ", ", "a", "b", "", "d")]
        [InlineData("(@p0, @p1, @p2, @p3)", ", ", "(@p0", "@p1", "@p2", "@p3)")]
        [InlineData("a = true AND b = false", " AND ", "a = true", "b = false")]
        [InlineData("a = true OR b = false", " OR ", "a = true", "b = false")]
        public void LiteralsWithJoin(string expectedSql, string joinString, params string[] literals)
        {
            var builder = new SqlBuilder();

            builder.AppendRange(literals, joinString: joinString);

            var sql = builder.Build();

            Assert.Equal(expectedSql, sql.Sql);
            Assert.Empty(sql.SqlParameters);
        }

        [Fact]
        public void LiteralsWithInterpolations()
        {
            var literals = new[] { $"field_a = {ParamStr}", $"field_b = {ParamBool}" };

            var builder = new SqlBuilder();

            builder.AppendRange(literals, joinString: " OR ");

            var sql = builder.Build();

            Assert.Equal($"field_a = {ParamStr} OR field_b = {ParamBool}", sql.Sql);
            Assert.Empty(sql.SqlParameters);
        }

        [Theory]
        [MemberData(nameof(InterpolatedSqlData))]
        public void InterpolatedSql(
            string expectedSql,
            object[] expectedParams,
            string? joinString,
            IInterpolatedSql[] fragments
        )
        {
            var builder = new SqlBuilder();

            builder.AppendRange(fragments, joinString: joinString);

            var sql = builder.Build();

            Assert.Equal(expectedSql, sql.Sql);
            Assert.Equal(expectedParams.Length, sql.SqlParameters.Count);
            Assert.Equal(expectedParams, sql.SqlParameters.Select(p => p.Argument));
        }

        [Theory]
        [MemberData(nameof(SqlBuildersData))]
        public void SqlBuilders(string expectedSql, object[] expectedParams, string? joinString, SqlBuilder[] builders)
        {
            var builder = new SqlBuilder();

            builder.AppendRange(builders, joinString: joinString);

            var sql = builder.Build();

            Assert.Equal(expectedSql, sql.Sql);
            Assert.Equal(expectedParams.Length, sql.SqlParameters.Count);
            Assert.Equal(expectedParams, sql.SqlParameters.Select(p => p.Argument));
        }

        [Theory]
        [MemberData(nameof(FormattableStringsData))]
        public void FormattableStrings(
            string expectedSql,
            object[] expectedParams,
            string? joinString,
            FormattableString[] strings
        )
        {
            var builder = new SqlBuilder();

            builder.AppendRange(strings, joinString: joinString);

            var sql = builder.Build();

            Assert.Equal(expectedSql, sql.Sql);
            Assert.Equal(expectedParams.Length, sql.SqlParameters.Count);
            Assert.Equal(expectedParams, sql.SqlParameters.Select(p => p.Argument));
        }

        private static IEnumerable<SqlBuilderTestCase> SqlBuilderTestCases()
        {
            return new List<SqlBuilderTestCase>
            {
                new(
                    ExpectedSql: "field_a = @p0 AND field_b = @p1",
                    ExpectedParameters: [ParamBool, ParamInt],
                    Builders: [new SqlBuilder($"field_a = {ParamBool}"), new SqlBuilder($"field_b = {ParamInt}")],
                    JoinString: " AND "
                ),
                new(
                    ExpectedSql: "field_a = @param AND field_b = @otherParam",
                    // Not binding any parameters as we're using literal text
                    ExpectedParameters: [],
                    Builders: [new SqlBuilder($"field_a = @param"), new SqlBuilder($"field_b = @otherParam")],
                    JoinString: "AND "
                ),
                new(
                    ExpectedSql: "FieldB = @p0 OR FieldC = @p1",
                    ExpectedParameters: [ParamStr, ParamInt],
                    Builders: [new SqlBuilder($"FieldB = {ParamStr}"), new SqlBuilder($"FieldC = {ParamInt}")],
                    JoinString: " OR "
                ),
                new(
                    ExpectedSql: "@p0, @p1, @p2",
                    ExpectedParameters: [ParamStr, ParamBool, ParamInt],
                    Builders:
                    [
                        new SqlBuilder($"{ParamStr}"),
                        new SqlBuilder($"{ParamBool}"),
                        new SqlBuilder($"{ParamInt}"),
                    ],
                    JoinString: ", "
                ),
                new(
                    ExpectedSql: "IN (@p0, @p1, @p2)",
                    ExpectedParameters: [ParamStr, ParamBool, ParamInt],
                    Builders:
                    [
                        new SqlBuilder($"IN ({ParamStr}"),
                        new SqlBuilder($"{ParamBool}"),
                        new SqlBuilder($"{ParamInt})"),
                    ],
                    JoinString: ", "
                ),
                new(
                    ExpectedSql: "some_field = @p0",
                    ExpectedParameters: [ParamInt],
                    Builders: [new SqlBuilder($"some_field"), new SqlBuilder($" = {ParamInt}")]
                ),
                new(
                    ExpectedSql: "WHERE field_a = @p0 AND field_b = @p1 OR field_c = @p2",
                    ExpectedParameters: [ParamBool, ParamInt, ParamStr],
                    Builders:
                    [
                        new SqlBuilder($"WHERE"),
                        new SqlBuilder($"field_a = {ParamBool}"),
                        new SqlBuilder($"AND field_b = {ParamInt}"),
                        new SqlBuilder($"OR field_c = {ParamStr}"),
                    ]
                ),
            };
        }

        public static TheoryData<string, object[], string?, IInterpolatedSql[]> InterpolatedSqlData =>
            CreateData(b => b.Build());

        public static TheoryData<string, object[], string?, SqlBuilder[]> SqlBuildersData => CreateData(b => b);

        public static TheoryData<string, object[], string?, FormattableString[]> FormattableStringsData =>
            CreateData(b => b.AsFormattableString());

        private static TheoryData<string, object[], string?, TFragment[]> CreateData<TFragment>(
            Func<SqlBuilder, TFragment> build
        )
        {
            var data = new TheoryData<string, object[], string?, TFragment[]>();

            foreach (var testCase in SqlBuilderTestCases())
            {
                data.Add(
                    testCase.ExpectedSql,
                    testCase.ExpectedParameters,
                    testCase.JoinString,
                    testCase.Builders.Select(build).ToArray()
                );
            }

            return data;
        }

        private record SqlBuilderTestCase(
            string ExpectedSql,
            object[] ExpectedParameters,
            SqlBuilder[] Builders,
            string? JoinString = null
        );
    }
}
