#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface ISubjectMetaService
    {
        Task<Either<ActionResult, ISubjectMetaViewModel>> GetSubjectMeta(Guid subjectId);
        Task<Either<ActionResult, ISubjectMetaViewModel>> GetSubjectMeta(SubjectMetaQueryContext query);
        Task<Either<ActionResult, ISubjectMetaViewModel>> GetSubjectMetaRestricted(Guid subjectId);
        Task<Either<ActionResult, ISubjectMetaViewModel>> GetSubjectMetaRestricted(SubjectMetaQueryContext query);
    }
}
