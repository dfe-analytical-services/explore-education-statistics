#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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

    Task<Either<ActionResult, List<EinContentSectionViewModel>>> ReorderSections(
        Guid pageId,
        List<Guid> newSectionOrder);

    Task<Either<ActionResult, List<EinContentSectionViewModel>>> DeleteSection(
        Guid pageId,
        Guid sectionId);

    Task<Either<ActionResult, EinContentBlockViewModel>> AddBlock(
        Guid pageId,
        Guid sectionId,
        EinBlockType type,
        int? order);

    Task<Either<ActionResult, EinContentBlockViewModel>> UpdateHtmlBlock(
        Guid pageId,
        Guid sectionId,
        Guid blockId,
        EinHtmlBlockUpdateRequest request);

    Task<Either<ActionResult, List<EinContentBlockViewModel>>> ReorderBlocks(
        Guid pageId,
        Guid sectionId,
        List<Guid> newBlockOrder);
}
