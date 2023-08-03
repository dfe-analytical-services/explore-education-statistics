#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class GlossaryService : IGlossaryService
    {
        private readonly ContentDbContext _contentDbContext;

        public GlossaryService(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<List<GlossaryCategoryViewModel>> GetGlossary()
        {
            var glossaryEntries = await _contentDbContext.GlossaryEntries
                .ToListAsync();
            return GlossaryUtils.BuildGlossary(glossaryEntries);
        }

        public async Task<Either<ActionResult, GlossaryEntryViewModel>> GetGlossaryEntry(string slug)
        {
            return await _contentDbContext.GlossaryEntries
                .SingleOrNotFoundAsync(e => e.Slug == slug)
                .OnSuccess(e => new GlossaryEntryViewModel(
                    Title: e.Title,
                    Slug: e.Slug,
                    Body: e.Body
                ));
        }
    }
}
