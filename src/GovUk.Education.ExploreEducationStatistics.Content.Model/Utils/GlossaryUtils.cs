#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Utils;

public static class GlossaryUtils
{
    public static List<GlossaryCategoryViewModel> BuildGlossary(IReadOnlyList<GlossaryEntry> entries)
    {
        var categories = GlossaryCategoryViewModel.BuildGlossaryCategories();

        var orderedEntries = entries
            .OrderBy(e => e.Title)
            .ToList();

        orderedEntries.ForEach(e =>
        {
            categories[char.ToUpper(e.Title[0])]
                .Entries
                .Add(new GlossaryEntryViewModel(
                    Title: e.Title,
                    Slug: e.Slug,
                    Body: e.Body
                ));
        });

        return categories
            .Values
            .OrderBy(c => c.Heading)
            .ToList();
    }
}
