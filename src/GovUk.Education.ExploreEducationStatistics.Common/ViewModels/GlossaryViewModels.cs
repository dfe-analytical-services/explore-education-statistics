#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

public record GlossaryEntryViewModel(string Title, string Slug, string Body);

public record GlossaryCategoryViewModel(char Heading, List<GlossaryEntryViewModel> Entries)
{
    // ReSharper disable once StringLiteralTypo
    private static readonly char[] AzCharArray = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    public static Dictionary<char, GlossaryCategoryViewModel> BuildGlossaryCategories()
    {
        return AzCharArray.ToDictionary(
            c => c,
            c => new GlossaryCategoryViewModel(
                Heading: c,
                Entries: new List<GlossaryEntryViewModel>()
            ));
    }
}
