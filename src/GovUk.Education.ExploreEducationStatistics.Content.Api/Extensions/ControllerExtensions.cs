using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Extensions
{
    public static class ControllerExtensions
    {
        public static async Task<ActionResult<string>> JsonContentResultAsync(this ControllerBase controller,
            Func<Task<string>> downloadAction, StatusCodeResult notFoundResult)
        {
            var download = await downloadAction.Invoke();

            if (string.IsNullOrWhiteSpace(download))
            {
                return notFoundResult;
            }

            return controller.Content(download, "application/json");
        }
    }
}