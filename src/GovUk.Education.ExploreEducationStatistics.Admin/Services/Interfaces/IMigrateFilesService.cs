﻿using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IMigrateFilesService
    {
        Task<Either<ActionResult, Unit>> MigrateFilenames(ReleaseFileTypes type);
    }
}