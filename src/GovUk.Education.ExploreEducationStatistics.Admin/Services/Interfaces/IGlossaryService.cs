#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IGlossaryService
{
    Task<List<GlossaryCategoryViewModel>> GetGlossary();

    Task<Either<ActionResult, GlossaryEntryViewModel>> GetGlossaryEntry(string slug);
}
