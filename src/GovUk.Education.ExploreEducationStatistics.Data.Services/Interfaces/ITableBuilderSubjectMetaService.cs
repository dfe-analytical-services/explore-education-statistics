using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta.TableBuilder;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface ITableBuilderSubjectMetaService
    {
        Task<Either<ActionResult, TableBuilderSubjectMetaViewModel>> GetSubjectMeta(Guid subjectId);
        
        Task<Either<ActionResult, TableBuilderSubjectMetaViewModel>> GetSubjectMeta(SubjectMetaQueryContext query);
    }
}