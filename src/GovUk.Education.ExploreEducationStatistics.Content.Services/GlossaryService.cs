#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
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

        [BlobCache(typeof(GlossaryCacheKey))]
        public async Task<List<GlossaryCategoryViewModel>> GetAllGlossaryEntries()
        {
            var categories = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            return await categories
                    .ToAsyncEnumerable()
                    .SelectAwait(async category =>
                    {
                        var entries = await _contentDbContext.GlossaryEntries.AsQueryable()
                            .Where(e => e.Title.Substring(0, 1).ToUpper() == category.ToString())
                            .Select(e => new GlossaryEntryViewModel
                            {
                                Title = e.Title,
                                Slug = e.Slug,
                                Body = e.Body,
                            })
                            .OrderBy(e => e.Title)
                            .ToListAsync();
                        return new GlossaryCategoryViewModel
                        {
                            Heading = category.ToString(),
                            Entries = entries,
                        };
                    })
                    .OrderBy(c => c.Heading)
                    .ToListAsync();
        }
    }
}
