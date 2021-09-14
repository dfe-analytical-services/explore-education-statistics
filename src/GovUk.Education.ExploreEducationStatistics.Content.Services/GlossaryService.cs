#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class GlossaryService : IGlossaryService
    {
        private readonly ContentDbContext _contentDbContext;

        public GlossaryService(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        [BlobCache(typeof(GlossaryCacheKey))]
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
    }
}
