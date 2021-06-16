using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies
{
    public interface IMethodologyAmendmentService
    {
        Task<Either<ActionResult, MethodologyViewModel>> CreateMethodologyAmendment(Guid originalMethodologyId);
    }
}