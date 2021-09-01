#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

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

            if (lastModified.HasValue
                && requestHeaders.IfModifiedSince.HasValue
                && requestHeaders.IfModifiedSince.Value >= lastModified)
            {
                return controller.StatusCode(StatusCodes.Status304NotModified);
            }

            return await Task.FromResult(Unit.Instance);
        }

        public static async Task<Either<ActionResult, Unit>> CacheWithETag(
            this ControllerBase controller,
            string content,
            bool isWeak = true)
        {
            var eTag = new EntityTagHeaderValue($"\"{content}\"", isWeak);

            controller.Response.GetTypedHeaders().ETag = eTag;

            var requestHeaders = controller.Request.GetTypedHeaders();

            if (requestHeaders.IfNoneMatch.Contains(eTag))
            {
                return controller.StatusCode(StatusCodes.Status304NotModified);
            }

            return await Task.FromResult(Unit.Instance);
        }

        public static async Task<Either<ActionResult, Unit>> CacheWithLastModifiedAndETag(
            this ControllerBase controller,
            DateTimeOffset? lastModified,
            string eTagContent,
            bool isWeak = true)
        {
            var eTag = new EntityTagHeaderValue($"\"{eTagContent}\"", isWeak);

            controller.Response.GetTypedHeaders().ETag = eTag;

            var requestHeaders = controller.Request.GetTypedHeaders();

            if (lastModified.HasValue
                && requestHeaders.IfModifiedSince.HasValue
                && requestHeaders.IfModifiedSince.Value >= lastModified
                && requestHeaders.IfNoneMatch.Contains(eTag))
            {
                return controller.StatusCode(StatusCodes.Status304NotModified);
            }

            return await Task.FromResult(Unit.Instance);
        }
    }
}