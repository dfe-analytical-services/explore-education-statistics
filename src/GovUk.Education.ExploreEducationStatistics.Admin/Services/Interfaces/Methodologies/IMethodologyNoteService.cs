#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies
{
    public interface IMethodologyNoteService
    {
        Task<Either<ActionResult, MethodologyNoteViewModel>> AddNote(
            Guid methodologyVersionId,
            MethodologyNoteAddRequest request);

        Task<Either<ActionResult, Unit>> DeleteNote(
            Guid methodologyVersionId,
            Guid methodologyNoteId);

        Task<Either<ActionResult, MethodologyNoteViewModel>> UpdateNote(
            Guid methodologyVersionId,
            Guid methodologyNoteId,
            MethodologyNoteUpdateRequest request);
    }
}
