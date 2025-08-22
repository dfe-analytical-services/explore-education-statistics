#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IEducationInNumbersContentService
{
    Task<Either<ActionResult, EducationInNumbersContentViewModel>> GetPageContent(Guid pageId);

    Task<Either<ActionResult, EinContentSectionViewModel>> AddSection(
        Guid pageId,
        int order);

    Task<Either<ActionResult, EinContentSectionViewModel>> UpdateSectionHeading(
        Guid pageId,
        Guid sectionId,
        string heading);
}
