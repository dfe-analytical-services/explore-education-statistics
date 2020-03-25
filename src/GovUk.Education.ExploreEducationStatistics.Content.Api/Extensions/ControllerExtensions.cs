using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Extensions
{
    public static class ControllerExtensions
    {
        public static async Task<ActionResult<T>> JsonContentResultAsync<T>(this ControllerBase controller,
            Func<Task<string>> downloadAction, StatusCodeResult notFoundResult)
        {
            var download = await downloadAction.Invoke();

            if (string.IsNullOrWhiteSpace(download))
            {
                return notFoundResult;
            }

            return JsonConvert.DeserializeObject<T>(download, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
    }
}