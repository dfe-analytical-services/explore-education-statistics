#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace GovUk.Education.ExploreEducationStatistics.Common.Config
{
    /// <summary>
    /// Tries to filter out any potentially sensitive data from telemetry
    /// before it is sent to Application Insights e.g. query string parameters
    /// containing things like access tokens or codes.
    /// </summary>
    public class SensitiveDataTelemetryProcessor : ITelemetryProcessor
    {
        private const string RedactedValue = "__redacted__";

        private readonly HashSet<string> _redactedTokens = new()
        {
            "code",
            "email",
            "phone",
            "token",
            "passphrase",
            "password",
            "pass",
            "user",
            "name",
            "username",
            "firstname",
            "lastname",
            "fullname"
        };

        private readonly char[] _tokenDelimiters =
        {
            '_', '-', ':', '.', '|', ' ', '[', ']', '+'
        };

        private readonly Regex _casingRegex = new("([A-Z][a-z]*|[0-9])", RegexOptions.Compiled);

        private ITelemetryProcessor Next { get; }

        public SensitiveDataTelemetryProcessor(ITelemetryProcessor next)
        {
            Next = next;
        }

        public void Process(ITelemetry item)
        {
            switch (item)
            {
                case RequestTelemetry telemetry:
                {
                    telemetry.Url = FilterUrl(telemetry.Url);
                    break;
                }
                case PageViewTelemetry telemetry:
                {
                    telemetry.Url = FilterUrl(telemetry.Url);

                    if (telemetry.Properties.ContainsKey("refUri"))
                    {
                        telemetry.Properties["refUri"] = FilterUriQuery(telemetry.Properties["refUri"]);
                    }

                    break;
                }
                case DependencyTelemetry telemetry:
                {
                    HandleDependencyTelemetry(telemetry);
                    break;
                }
            }

            Next.Process(item);
        }

        private void HandleDependencyTelemetry(DependencyTelemetry telemetry)
        {
            if (telemetry.Type != "Http")
            {
                return;
            }

            telemetry.Name = FilterUriQuery(telemetry.Name);
            telemetry.Data = FilterUriQuery(telemetry.Data);
        }

        private Uri? FilterUrl(Uri? uri)
        {
            if (uri is null)
            {
                return uri;
            }

            var filteredUri = FilterUriQuery(uri.AbsoluteUri);

            return filteredUri is null ? uri : new Uri(filteredUri);
        }

        private string? FilterUriQuery(string? uri)
        {
            if (uri.IsNullOrEmpty())
            {
                return uri;
            }

            var pathAndQuery = uri.Split('?', 2);

            if (pathAndQuery.Length < 2)
            {
                return uri;
            }

            var queryParts = pathAndQuery[1].Split('&');

            var filteredQuery = queryParts.Select(
                    keyValue =>
                    {
                        var parts = keyValue.Split('=', 2);

                        if (parts.Length < 2)
                        {
                            return keyValue;
                        }

                        var key = parts[0];

                        return IsRedactedKey(key) ? $"{key}={RedactedValue}" : keyValue;
                    }
                )
                .JoinToString('&');

            return $"{pathAndQuery[0]}?{filteredQuery}";
        }

        private bool IsRedactedKey(string key)
        {
            return HttpUtility.UrlDecode(key)
                .Split(_tokenDelimiters, StringSplitOptions.RemoveEmptyEntries)
                .Any(
                    token =>
                    {
                        if (_redactedTokens.Contains(token.ToLower()))
                        {
                            return true;
                        }

                        // The token may be camel/pascal cased, meaning we should split
                        // it up to check if any of the words is a matching token.
                        // Note - we can't just check if any of the redacted tokens are inside of
                        // the current token as redacted tokens may constitute actual words
                        // e.g. `code` is in `encode` (which shouldn't be redacted).
                        var words = _casingRegex.Split(token);

                        return words.Any(word => _redactedTokens.Contains(word.ToLower()));
                    }
                );
        }
    }
}