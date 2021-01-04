using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class ControllerExtensions
    {
        public static async Task<Either<ActionResult, Unit>> CacheWithLastModified(
            this ControllerBase controller,
            DateTimeOffset? lastModified)
        {
            controller.Response.GetTypedHeaders().LastModified = lastModified;

            var requestHeaders = controller.Request.GetTypedHeaders();

            if (requestHeaders.IfModifiedSince.HasValue
                && requestHeaders.IfModifiedSince.Value >= lastModified)
            {
                return controller.StatusCode(StatusCodes.Status304NotModified);
            }

            return await Task.FromResult(Unit.Instance);
        }
    }
}