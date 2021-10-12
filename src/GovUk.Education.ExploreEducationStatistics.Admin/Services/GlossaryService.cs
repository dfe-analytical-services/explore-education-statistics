#nullable enable
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class GlossaryService : IGlossaryService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;

        public GlossaryService(IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _persistenceHelper = persistenceHelper;
        }

        public async Task<Either<ActionResult, GlossaryEntryViewModel>> GetGlossaryEntry(string slug)
        {
            return await _persistenceHelper
                .CheckEntityExists<GlossaryEntry>(query => query
                    .Where(e => e.Slug == slug))
                .OnSuccess(e => new GlossaryEntryViewModel
                {
                    Title = e.Title,
                    Slug = e.Slug,
                    Body = e.Body,
                });
        }
    }
}
