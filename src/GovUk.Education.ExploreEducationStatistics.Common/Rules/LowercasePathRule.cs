#nullable enable
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;

namespace GovUk.Education.ExploreEducationStatistics.Common.Rules;

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
