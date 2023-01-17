#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class HttpRequestExtensions
{
    public static async Task<TJsonType?> GetJsonBody<TJsonType>(this HttpRequest request, bool allowEmptyBody = true)
        where TJsonType : class
    {
        var requestBody = await new StreamReader(request.Body).ReadToEndAsync();

        if (requestBody.IsNullOrEmpty() && allowEmptyBody)
        {
            return null;
        }
        
        var json = JsonConvert.DeserializeObject<TJsonType>(requestBody);

        if (json == null)
        {
            throw new ArgumentException($"Could not deserialize request body to type {typeof(TJsonType)}");
        }

        return json;
    }
    
    public static async Task<dynamic?> GetJsonBody(this HttpRequest request)
    {
        var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
        return JsonConvert.DeserializeObject(requestBody);
    }
}