#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IEducationInNumbersService
{
    Task<Either<ActionResult, EducationInNumbersPageViewModel>> GetPage(
        string? slug,
        bool? published = null);

    Task<Either<ActionResult, List<EducationInNumbersPageViewModel>>> ListLatestPages();

    Task<Either<ActionResult, EducationInNumbersPageViewModel>> CreatePage(
        CreateEducationInNumbersPageRequest request);

    Task<Either<ActionResult, EducationInNumbersPageViewModel>> CreateAmendment(
        Guid id);

    Task<Either<ActionResult, EducationInNumbersPageViewModel>> UpdatePage(
        Guid id,
        UpdateEducationInNumbersPageRequest request);

    Task<Either<ActionResult, List<EducationInNumbersPageViewModel>>> Reorder(
        List<Guid> newOrder);

    Task<Either<ActionResult, Unit>> Delete(Guid id);
}
