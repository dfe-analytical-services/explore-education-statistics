using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public class ResponseErrorAssertUtils
{
        public static void AssertHasErrors(IEnumerable<ErrorViewModel> errors, List<ErrorViewModel> expectedErrors)
        {
            foreach (var error in errors)
            {
                var foundError = expectedErrors.Find(expected => expected.Message == error.Message);
                if (foundError == null)
                {
                    Assert.Fail($"Error message not found in expectedErrors:\n{error.Message}");
                }

                Assert.Equal(foundError.Code, error.Code);
                Assert.Equal(foundError.Path, error.Path);

                expectedErrors.Remove(foundError);
            }

            if (expectedErrors.Count != 0)
            {
                var expectedErrorMessages = expectedErrors
                    .Select(e => e.Message)
                    .ToList()
                    .JoinToString('\n');
                Assert.Fail($"expectedErrors messages were not in the response:\n{expectedErrorMessages}");
            }
        }

        public static void AssertBadRequestHasErrors(ActionResult result, List<ErrorViewModel> expectedErrors)
        {
            var badRequest = (BadRequestObjectResult)result;
            var validationProblems = (ValidationProblemViewModel)badRequest.Value!;
            var errors = validationProblems!.Errors;

            AssertHasErrors(errors, expectedErrors);
        }
}
