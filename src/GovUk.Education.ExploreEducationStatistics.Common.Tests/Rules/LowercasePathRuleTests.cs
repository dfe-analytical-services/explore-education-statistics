#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Rules;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Rules;

public class LowercasePathRuleTests
{
    private readonly LowercasePathRule _rule;

    public LowercasePathRuleTests()
    {
        _rule = new LowercasePathRule();
    }

    [Fact]
    public void ApplyRule_PathIsLowercase_DoesNotRedirect()
    {
        // Setup a new context with a request path that is already lowercase and should not be redirected
        var rewriteContext = new RewriteContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Scheme = "https",
                    Host = new HostString("localhost"),
                    PathBase = new PathString("/base"),
                    Path = new PathString("/path"),
                    QueryString = new QueryString("?query=query")
                }
            }
        };

        _rule.ApplyRule(rewriteContext);

        var response = rewriteContext.HttpContext.Response;

        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Equal(RuleResult.ContinueRules, rewriteContext.Result);
        Assert.True(StringValues.IsNullOrEmpty(response.Headers.Location));
    }

    [Theory]
    [InlineData("/_configuration/Admin")]
    [InlineData("/Identity/Account/Login")]
    [InlineData("/static/js/App.js")]
    public void ApplyRule_PathIsNotLowercaseButExcluded_DoesNotRedirect(string path)
    {
        // Setup a new context with a request path that matches the excluded regex as well as not being lowercase
        var rewriteContext = new RewriteContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Scheme = "https",
                    Host = new HostString("localhost"),
                    PathBase = new PathString("/base"),
                    Path = new PathString(path),
                    QueryString = new QueryString("?query=query")
                }
            }
        };

        _rule.ApplyRule(rewriteContext);

        var response = rewriteContext.HttpContext.Response;

        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Equal(RuleResult.ContinueRules, rewriteContext.Result);
        Assert.True(StringValues.IsNullOrEmpty(response.Headers.Location));
    }

    [Fact]
    public void ApplyRule_PathIsNotLowercase_ResponseIsRedirected()
    {
        // Setup a new context with a request path that is not lowercase and should be redirected
        var rewriteContext = new RewriteContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Scheme = "https",
                    Host = new HostString("localhost"),
                    PathBase = new PathString("/base"),
                    Path = new PathString("/Path"),
                    QueryString = new QueryString("?query=query")
                }
            }
        };

        _rule.ApplyRule(rewriteContext);

        var response = rewriteContext.HttpContext.Response;

        Assert.Equal(StatusCodes.Status308PermanentRedirect, response.StatusCode);
        Assert.Equal(RuleResult.EndResponse, rewriteContext.Result);
        // Path is lowercased
        Assert.Equal("https://localhost/base/path?query=query", response.Headers.Location);
    }

    [Fact]
    public void ApplyRule_PathIsNotLowercase_QueryStringIsNotModified()
    {
        // Setup a new context with a request path that is not lowercase and should be redirected,
        // but which also has a query string that should not be modified
        var rewriteContext = new RewriteContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Scheme = "https",
                    Host = new HostString("localhost"),
                    PathBase = new PathString("/base"),
                    Path = new PathString("/Path"),
                    QueryString = new QueryString("?query=Query")
                }
            }
        };

        _rule.ApplyRule(rewriteContext);

        var response = rewriteContext.HttpContext.Response;

        Assert.Equal(StatusCodes.Status308PermanentRedirect, response.StatusCode);
        Assert.Equal(RuleResult.EndResponse, rewriteContext.Result);
        // Query string is not modified
        Assert.Equal("https://localhost/base/path?query=Query", response.Headers.Location);
    }
}
