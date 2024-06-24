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
            List<ErrorViewModel> notFoundErrors = [];

            foreach (var error in errors)
            {

                var foundError = expectedErrors.Find(expected => expected.Message == error.Message);
                if (foundError == null)
                {
                    notFoundErrors.Add(error);
                    continue;
                }

                Assert.Equal(foundError.Code, error.Code);
                Assert.Equal(foundError.Path, error.Path);

                expectedErrors.Remove(foundError);
            }

            if (notFoundErrors.Count != 0)
            {
                var notFoundErrorMessages = notFoundErrors
                    .Select(e => e.Message)
                    .ToList()
                    .JoinToString('\n');
                Assert.Fail($"Error message(s) not found in expectedErrors:\n{notFoundErrorMessages}");
            }

            if (expectedErrors.Count != 0)
            {
                var expectedErrorMessages = expectedErrors
                    .Select(e => e.Message)
                    .ToList()
                    .JoinToString('\n');
                Assert.Fail($"expectedErrors message(s) were not in the response:\n{expectedErrorMessages}");
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
