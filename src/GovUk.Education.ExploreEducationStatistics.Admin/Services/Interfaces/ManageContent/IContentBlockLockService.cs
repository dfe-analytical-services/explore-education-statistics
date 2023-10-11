#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;

public interface IContentBlockLockService
{
    Task<Either<ActionResult, ContentBlockLockViewModel>> LockContentBlock(Guid id, bool force = false);

    Task<Either<ActionResult, Unit>> UnlockContentBlock(Guid id, bool force = false);
}