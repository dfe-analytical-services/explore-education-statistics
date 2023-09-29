#nullable enable
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;

namespace GovUk.Education.ExploreEducationStatistics.Common.Rules;

/// <summary>
/// Rule which is used to redirect any request paths that contain uppercase characters to lowercase.
/// This excludes any paths that match the regex defined in <see cref="ExcludedPathsRegex"/>.
/// Examples of requests that we might want to exclude:
/// <list type="bullet">
/// <item>
/// <description>Oidc configuration</description>
/// </item>
/// <item>
/// <description>Identity endpoints</description>
/// </item>
/// <item>
/// <description>Static assets</description>
/// </item>
/// </list>
/// </summary>
public class LowercasePathRule : IRule
{
    private const string ExcludedPathsRegex = "^/(_configuration|identity|static).*$";

    public void ApplyRule(RewriteContext context)
    {
        var request = context.HttpContext.Request;

        var scheme = request.Scheme;
        var pathBase = request.PathBase;
        var path = request.Path;
        var host = request.Host;
        var query = request.QueryString;

        if (IsApplicable(path))
        {
            var location = $"{scheme}://{host.Value}{pathBase.Value}{path.Value}".ToLower() + query;

            var response = context.HttpContext.Response;
            response.StatusCode = StatusCodes.Status308PermanentRedirect;
            response.Headers[HeaderNames.Location] = location;
            context.Result = RuleResult.EndResponse;
        }
        else
        {
            context.Result = RuleResult.ContinueRules;
        }
    }

    private static bool IsApplicable(PathString path)
    {
        return path.HasValue
               && !Regex.IsMatch(path, ExcludedPathsRegex, RegexOptions.IgnoreCase)
               && path.Value.Any(char.IsUpper);
    }
}
