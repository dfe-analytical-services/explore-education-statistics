using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IGlossaryService
{
    Task<List<GlossaryCategoryViewModel>> GetGlossary();

    Task<Either<ActionResult, GlossaryEntryViewModel>> GetGlossaryEntry(string slug);
}
