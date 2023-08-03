#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;

public class GlossaryUtilsTests
{
    // ReSharper disable once StringLiteralTypo
    private static readonly char[] AzCharArray = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    [Fact]
    public void BuildGlossary()
    {
        var entries = new List<GlossaryEntry>
        {
            new()
            {
                Title = "Exclusions",
                Slug = "exclusions-slug",
                Body = "Exclusions body"
            },
            new()
            {
                Title = "Absence",
                Slug = "absence-slug",
                Body = "Absence body"
            },
            new()
            {
                Title = "expedient", // lowercase first letter
                Slug = "expedient-slug",
                Body = "Expedient body"
            }
        };

        var result = GlossaryUtils.BuildGlossary(entries);

        Assert.Equal(26, result.Count);

        var expectedHeadingWithEntries = new[] { 'A', 'E' };
        Assert.All(result.WithIndex(), tuple =>
        {
            var (category, index) = tuple;
            Assert.Equal(AzCharArray[index], category.Heading);
            if (!expectedHeadingWithEntries.Contains(category.Heading))
            {
                Assert.Empty(category.Entries);
            }
        });

        var categoryA = result[0];
        Assert.Single(categoryA.Entries);

        AssertGlossaryEntry(entries[1], categoryA.Entries[0]);

        var categoryE = result[4];
        Assert.Equal(2, categoryE.Entries.Count);

        AssertGlossaryEntry(entries[0], categoryE.Entries[0]);
        AssertGlossaryEntry(entries[2], categoryE.Entries[1]);
    }

    [Fact]
    public void ListGlossaryEntries_NoEntries()
    {
        var result = GlossaryUtils.BuildGlossary(new List<GlossaryEntry>());

        Assert.Equal(26, result.Count);

        Assert.All(result.WithIndex(), tuple =>
        {
            var (category, index) = tuple;
            Assert.Equal(AzCharArray[index], category.Heading);
            Assert.Empty(category.Entries);
        });
    }

    private static void AssertGlossaryEntry(GlossaryEntry glossaryEntry, GlossaryEntryViewModel viewModel)
    {
        Assert.Equal(glossaryEntry.Title, viewModel.Title);
        Assert.Equal(glossaryEntry.Slug, viewModel.Slug);
        Assert.Equal(glossaryEntry.Body, viewModel.Body);
    }
}
