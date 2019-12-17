using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils
{
    public static class EitherTaskExtensions
    {
        public static async Task<ActionResult> HandleFailures<Tr>(
            this Task<Either<ActionResult, Tr>> validationErrorsRaisingAction) where Tr : ActionResult
        {
            var result = await validationErrorsRaisingAction;
            
            return result.IsRight ? result.Right : result.Left;
        }
    }
}