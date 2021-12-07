#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class GlossaryService : IGlossaryService
    {
        private readonly ContentDbContext _contentDbContext;

        public GlossaryService(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<List<GlossaryCategoryViewModel>> GetAllGlossaryEntries()
        {
            var categories = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            var categoryDictionary = categories.ToDictionary(
                c => c.ToString(),
                c => new GlossaryCategoryViewModel
                {
                    Heading = c.ToString(),
                    Entries = new List<GlossaryEntryViewModel>(),
                });

            var allGlossaryEntries = await _contentDbContext.GlossaryEntries
                .ToAsyncEnumerable()
                .OrderBy(e => e.Title)
                .ToListAsync();

            allGlossaryEntries
                .ForEach(e =>
                {
                    categoryDictionary[e.Title[..1].ToUpper()].Entries
                        .Add(new GlossaryEntryViewModel
                    {
                        Title = e.Title,
                        Slug = e.Slug,
                        Body = e.Body,
                    });
                });

            return categoryDictionary
                .Values
                .OrderBy(c => c.Heading)
                .ToList();
        }

        public async Task<Either<ActionResult, GlossaryEntryViewModel>> GetGlossaryEntry(string slug)
        {
            var glossaryEntry = await _contentDbContext.GlossaryEntries
                .ToAsyncEnumerable()
                .Where(e => e.Slug == slug)
                .SingleOrDefaultAsync();

            if (glossaryEntry == null)
            {
                return new NotFoundResult();
            }

            return new GlossaryEntryViewModel
            {
                Title = glossaryEntry!.Title,
                Slug = glossaryEntry.Slug,
                Body = glossaryEntry.Body,
            };
        }
    }
}
