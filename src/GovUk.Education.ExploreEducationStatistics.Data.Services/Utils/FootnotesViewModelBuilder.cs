#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;

public static class FootnotesViewModelBuilder
{
    public static List<FootnoteViewModel> BuildFootnotes(IEnumerable<Footnote> footnotes)
    {
        return footnotes.Select(
            footnote => new FootnoteViewModel(footnote.Id, footnote.Content)
        ).ToList();
    }
}
