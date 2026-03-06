using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReplacementServiceHelperTests
{
    [Fact]
    public void ReplaceFilterSequence()
    {
        // Define a set of filters, filter groups and filter items belonging to the original subject
        var originalFilters = new List<Filter>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter a",
                Name = "filter_a",
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group a",
                        FilterItems = new List<FilterItem>
                        {
                            new() { Id = Guid.NewGuid(), Label = "Item a" },
                            new() { Id = Guid.NewGuid(), Label = "Item b" },
                            new() { Id = Guid.NewGuid(), Label = "Item c" },
                        },
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group b",
                        FilterItems = new List<FilterItem>
                        {
                            new() { Id = Guid.NewGuid(), Label = "Item d" },
                            new() { Id = Guid.NewGuid(), Label = "Item e" },
                            new() { Id = Guid.NewGuid(), Label = "Item f" },
                        },
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group c",
                        FilterItems = new List<FilterItem>
                        {
                            new() { Id = Guid.NewGuid(), Label = "Item g" },
                            new() { Id = Guid.NewGuid(), Label = "Item h" },
                            new() { Id = Guid.NewGuid(), Label = "Item i" },
                        },
                    },
                },
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter b",
                Name = "filter_b",
                FilterGroups = new List<FilterGroup>(),
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter c",
                Name = "filter_c",
                FilterGroups = new List<FilterGroup>(),
            },
        };

        // Define a sequence for the original subject which is expected to be updated after the replacement
        var originalReleaseFile = new ReleaseFile
        {
            FilterSequence = new List<FilterSequenceEntry>
            {
                // Filter c
                new(originalFilters[2].Id, new List<FilterGroupSequenceEntry>()),
                // Filter a
                new(
                    originalFilters[0].Id,
                    new List<FilterGroupSequenceEntry>
                    {
                        // Group c
                        new(
                            originalFilters[0].FilterGroups[2].Id,
                            new List<Guid>
                            {
                                // Item i, Item g, Item h
                                originalFilters[0].FilterGroups[2].FilterItems[2].Id,
                                originalFilters[0].FilterGroups[2].FilterItems[0].Id,
                                originalFilters[0].FilterGroups[2].FilterItems[1].Id,
                            }
                        ),
                        // Group a
                        new(
                            originalFilters[0].FilterGroups[0].Id,
                            new List<Guid>
                            {
                                // Item c, Indicator a, Indicator b
                                originalFilters[0].FilterGroups[0].FilterItems[2].Id,
                                originalFilters[0].FilterGroups[0].FilterItems[0].Id,
                                originalFilters[0].FilterGroups[0].FilterItems[1].Id,
                            }
                        ),
                        // Group b
                        new(
                            originalFilters[0].FilterGroups[1].Id,
                            new List<Guid>
                            {
                                // Item f, Item d, Item e
                                originalFilters[0].FilterGroups[1].FilterItems[2].Id,
                                originalFilters[0].FilterGroups[1].FilterItems[0].Id,
                                originalFilters[0].FilterGroups[1].FilterItems[1].Id,
                            }
                        ),
                    }
                ),
                // Filter b
                new(originalFilters[1].Id, new List<FilterGroupSequenceEntry>()),
            },
        };

        // Define the set of filters, filter groups and filter items belonging to the replacement subject
        var replacementFilters = new List<Filter>
        {
            // 'Filter a' is updated
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter a",
                Name = "filter_a",
                FilterGroups = new List<FilterGroup>
                {
                    // 'Group a' is removed
                    // 'Group e' is added
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group e",
                        FilterItems = new List<FilterItem>(),
                    },
                    // Group 'Total' is added
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Total",
                        FilterItems = new List<FilterItem>
                        {
                            new() { Id = Guid.NewGuid(), Label = "Item m" },
                            new() { Id = Guid.NewGuid(), Label = "Total" },
                            new() { Id = Guid.NewGuid(), Label = "Item l" },
                        },
                    },
                    // 'Group d' is added
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group d",
                        FilterItems = new List<FilterItem>(),
                    },
                    // 'Group b' is updated
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group b",
                        FilterItems = new List<FilterItem>
                        {
                            new() { Id = Guid.NewGuid(), Label = "Item d" },
                            // 'Item e' is removed
                            // 'Item k' is added
                            new() { Id = Guid.NewGuid(), Label = "Item k" },
                            // 'Item j' is added
                            new() { Id = Guid.NewGuid(), Label = "Item j" },
                            new() { Id = Guid.NewGuid(), Label = "Item f" },
                        },
                    },
                    // 'Group c' remains identical
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group c",
                        FilterItems = new List<FilterItem>
                        {
                            new() { Id = Guid.NewGuid(), Label = "Item g" },
                            new() { Id = Guid.NewGuid(), Label = "Item h" },
                            new() { Id = Guid.NewGuid(), Label = "Item i" },
                        },
                    },
                },
            },
            // 'Filter b' is removed
            // 'Filter e' is added
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter e",
                Name = "filter_e",
                FilterGroups = new List<FilterGroup>(),
            },
            // 'Filter d' is added
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter d",
                Name = "filter_d",
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group g",
                        FilterItems = new List<FilterItem>(),
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Total",
                        FilterItems = new List<FilterItem>
                        {
                            new() { Id = Guid.NewGuid(), Label = "Item o" },
                            new() { Id = Guid.NewGuid(), Label = "Total" },
                            new() { Id = Guid.NewGuid(), Label = "Item n" },
                        },
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group f",
                        FilterItems = new List<FilterItem>(),
                    },
                },
            },
            // 'Filter c' remains identical
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter c",
                Name = "filter_c",
                FilterGroups = new List<FilterGroup>(),
            },
        };

        var updatedSequence = ReplacementServiceHelper.ReplaceFilterSequence(
            originalFilters: originalFilters,
            replacementFilters: replacementFilters,
            originalReleaseFile
        );

        // Verify the updated sequence of filters
        Assert.NotNull(updatedSequence);

        Assert.Equal(4, updatedSequence!.Count);

        // 'Filter c' was first in the original sequence and is identical in the replacement subject so it should be first
        var filterC = updatedSequence[0];
        Assert.Equal(replacementFilters[3].Id, filterC.Id);
        Assert.Empty(filterC.ChildSequence);

        // 'Filter a' should be next in the sequence
        var filterA = updatedSequence[1];
        Assert.Equal(replacementFilters[0].Id, filterA.Id);

        var filterAGroups = filterA.ChildSequence;
        Assert.Equal(5, filterAGroups.Count);

        // 'Group c' was first in the original sequence and is untouched in the replacement subject so it should be first
        var filterAGroupC = filterAGroups[0];
        Assert.Equal(replacementFilters[0].FilterGroups[4].Id, filterAGroupC.Id);
        Assert.Equal(3, filterAGroupC.ChildSequence.Count);
        Assert.Equal(replacementFilters[0].FilterGroups[4].FilterItems[2].Id, filterAGroupC.ChildSequence[0]);
        Assert.Equal(replacementFilters[0].FilterGroups[4].FilterItems[0].Id, filterAGroupC.ChildSequence[1]);
        Assert.Equal(replacementFilters[0].FilterGroups[4].FilterItems[1].Id, filterAGroupC.ChildSequence[2]);

        // 'Group a' would've been the next group but has been removed

        // 'Group b' should be second based on the original sequence
        // Check 'Item e' was removed and both 'Item j' and 'Item k' have been appended in order
        var filterAGroupB = filterAGroups[1];
        Assert.Equal(replacementFilters[0].FilterGroups[3].Id, filterAGroupB.Id);
        Assert.Equal(4, filterAGroupB.ChildSequence.Count);
        // 'Item f' is first from the original sequence
        Assert.Equal(replacementFilters[0].FilterGroups[3].FilterItems[3].Id, filterAGroupB.ChildSequence[0]);
        // 'Item d' is second from the original sequence
        Assert.Equal(replacementFilters[0].FilterGroups[3].FilterItems[0].Id, filterAGroupB.ChildSequence[1]);
        // 'Item j' is appended first alphabetically
        Assert.Equal(replacementFilters[0].FilterGroups[3].FilterItems[2].Id, filterAGroupB.ChildSequence[2]);
        // 'Item k' is appended second alphabetically
        Assert.Equal(replacementFilters[0].FilterGroups[3].FilterItems[1].Id, filterAGroupB.ChildSequence[3]);

        // Group 'Total' is new so it should be appended first and its filter items should be ordered by label
        var filterAGroupTotal = filterAGroups[2];
        Assert.Equal(replacementFilters[0].FilterGroups[1].Id, filterAGroupTotal.Id);
        // Item 'Total' should be first
        Assert.Equal(replacementFilters[0].FilterGroups[1].FilterItems[1].Id, filterAGroupTotal.ChildSequence[0]);
        // 'Item l' should be second alphabetically
        Assert.Equal(replacementFilters[0].FilterGroups[1].FilterItems[2].Id, filterAGroupTotal.ChildSequence[1]);
        // 'Item m' should be third alphabetically
        Assert.Equal(replacementFilters[0].FilterGroups[1].FilterItems[0].Id, filterAGroupTotal.ChildSequence[2]);

        // 'Group d' is new so it should be appended in order and its filter items should be ordered by label
        var filterAGroupD = filterAGroups[3];
        Assert.Equal(replacementFilters[0].FilterGroups[2].Id, filterAGroupD.Id);
        Assert.Empty(filterAGroupD.ChildSequence);

        // 'Group e' is new so it should be appended in order
        var filterAGroupE = filterAGroups[4];
        Assert.Equal(replacementFilters[0].FilterGroups[0].Id, filterAGroupE.Id);
        Assert.Empty(filterAGroupE.ChildSequence);

        // 'Filter b' would've been the next filter but has been removed

        // 'Filter d' is new so it should be appended in order and its filter groups and filter items should be ordered by label
        var filterD = updatedSequence[2];
        Assert.Equal(replacementFilters[2].Id, filterD.Id);

        var filterDGroups = filterD.ChildSequence;
        Assert.Equal(3, filterDGroups.Count);

        // Group 'Total' should be first and its filter items should be ordered by label
        var filterDGroupTotal = filterDGroups[0];
        Assert.Equal(replacementFilters[2].FilterGroups[1].Id, filterDGroupTotal.Id);
        Assert.Equal(3, filterDGroupTotal.ChildSequence.Count);
        // Item 'Total' should be first
        Assert.Equal(replacementFilters[2].FilterGroups[1].FilterItems[1].Id, filterDGroupTotal.ChildSequence[0]);
        // 'Item n' should be second alphabetically
        Assert.Equal(replacementFilters[2].FilterGroups[1].FilterItems[2].Id, filterDGroupTotal.ChildSequence[1]);
        // 'Item o' should be third alphabetically
        Assert.Equal(replacementFilters[2].FilterGroups[1].FilterItems[0].Id, filterDGroupTotal.ChildSequence[2]);

        // 'Group f' should be second alphabetically
        var filterDGroupF = filterDGroups[1];
        Assert.Equal(replacementFilters[2].FilterGroups[2].Id, filterDGroupF.Id);
        Assert.Empty(filterDGroupF.ChildSequence);

        // 'Group g' should be third alphabetically
        var filterDGroupG = filterDGroups[2];
        Assert.Equal(replacementFilters[2].FilterGroups[0].Id, filterDGroupG.Id);
        Assert.Empty(filterDGroupG.ChildSequence);

        // 'Filter e' is new so it should be appended in order
        var filterE = updatedSequence[3];
        Assert.Equal(replacementFilters[1].Id, filterE.Id);
        Assert.Empty(filterE.ChildSequence);
    }

    [Fact]
    public void ReplaceIndicatorSequence_Success()
    {
        var originalGroups = new List<IndicatorGroup>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group a",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator a",
                        Name = "indicator_a",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator b",
                        Name = "indicator_b",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator c",
                        Name = "indicator_c",
                    },
                ],
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group b",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator d",
                        Name = "indicator_d",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator e",
                        Name = "indicator_e",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator f",
                        Name = "indicator_f",
                    },
                ],
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group c",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator g",
                        Name = "indicator_g",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator h",
                        Name = "indicator_h",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator i",
                        Name = "indicator_i",
                    },
                ],
            },
        };

        // Define a sequence for the original subject which is expected to be updated after the replacement
        var originalReleaseFile = new ReleaseFile
        {
            IndicatorSequence =
            [
                new IndicatorGroupSequenceEntry(
                    originalGroups[2].Id, // Group c
                    [
                        originalGroups[2].Indicators[2].Id, // i
                        originalGroups[2].Indicators[0].Id, // g
                        originalGroups[2].Indicators[1].Id, // h
                    ]
                ),
                new IndicatorGroupSequenceEntry(
                    originalGroups[0].Id, // Group a
                    [
                        originalGroups[0].Indicators[2].Id, // c
                        originalGroups[0].Indicators[0].Id, // a
                        originalGroups[0].Indicators[1].Id, // b
                    ]
                ),
                new(
                    originalGroups[1].Id, // Group b
                    [
                        originalGroups[1].Indicators[2].Id, // f
                        originalGroups[1].Indicators[0].Id, // d
                        originalGroups[1].Indicators[1].Id, // e
                    ]
                ),
            ],
        };

        // Define the set of indicator groups and indicators belonging to the replacement subject
        var replacementGroups = new List<IndicatorGroup>
        {
            // 'Group a' is removed
            new() // 'Group d' is added
            {
                Id = Guid.NewGuid(),
                Label = "Group d",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator n",
                        Name = "indicator_n",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator l",
                        Name = "indicator_l",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator m",
                        Name = "indicator_m",
                    },
                ],
            },
            new() // 'Group b' is updated
            {
                Id = Guid.NewGuid(),
                Label = "Group b",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator d",
                        Name = "indicator_d",
                    },
                    // 'Indicator e' is removed

                    new Indicator() // 'Indicator k' is added
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator k",
                        Name = "indicator_k",
                    },
                    new Indicator() // 'Indicator j' is added
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator j",
                        Name = "indicator_j",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator f",
                        Name = "indicator_f",
                    },
                ],
            },
            new() // 'Group c' is unchanged
            {
                Id = Guid.NewGuid(),
                Label = "Group c",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator g",
                        Name = "indicator_g",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator h",
                        Name = "indicator_h",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator i",
                        Name = "indicator_i",
                    },
                ],
            },
        };

        var mapping = GenerateMapping(
            originalDataSetId: Guid.NewGuid(),
            replacementDataSetId: Guid.NewGuid(),
            originalIndicatorGroups: originalGroups,
            replacementIndicatorGroups: replacementGroups
        );

        var updatedSequence = ReplacementServiceHelper.ReplaceIndicatorSequence(
            mapping: mapping,
            originalGroupIdToLabelMap: originalGroups.ToDictionary(g => g.Id, g => g.Label),
            replacementGroupLabelToIdMap: replacementGroups.ToDictionary(g => g.Label, g => g.Id),
            originalReleaseFile.IndicatorSequence
        );

        // Verify the updated sequence of indicators
        Assert.NotNull(updatedSequence);

        Assert.Equal(3, updatedSequence!.Count);

        // 'Group c' was first in the original sequence and is identical in the replacement subject so it should be first
        var groupC = updatedSequence[0];
        Assert.Equal(replacementGroups[2].Id, groupC.Id);
        Assert.Equal(3, groupC.ChildSequence.Count);
        Assert.Equal(replacementGroups[2].Indicators[2].Id, groupC.ChildSequence[0]); // i
        Assert.Equal(replacementGroups[2].Indicators[0].Id, groupC.ChildSequence[1]); // g
        Assert.Equal(replacementGroups[2].Indicators[1].Id, groupC.ChildSequence[2]); // h

        // 'Group a' would've been the next group but has been removed

        // 'Group b' should be second based on the original sequence
        // Check 'Indicator e' was removed and both 'Indicator j' and 'Indicator k' have been appended in order
        var groupB = updatedSequence[1];
        Assert.Equal(replacementGroups[1].Id, groupB.Id);
        Assert.Equal(4, groupB.ChildSequence.Count);
        // f and d from original sequence, j and k appended on the end alphabetically
        Assert.Equal(replacementGroups[1].Indicators[3].Id, groupB.ChildSequence[0]); // f
        Assert.Equal(replacementGroups[1].Indicators[0].Id, groupB.ChildSequence[1]); // d
        Assert.Equal(replacementGroups[1].Indicators[2].Id, groupB.ChildSequence[2]); // j
        Assert.Equal(replacementGroups[1].Indicators[1].Id, groupB.ChildSequence[3]); // k

        // 'Group d' is new so it should be appended in order and its indicators should be ordered by label
        var groupD = updatedSequence[2];
        Assert.Equal(replacementGroups[0].Id, groupD.Id);
        Assert.Equal(3, groupD.ChildSequence.Count);
        Assert.Equal(replacementGroups[0].Indicators[1].Id, groupD.ChildSequence[0]); // l
        Assert.Equal(replacementGroups[0].Indicators[2].Id, groupD.ChildSequence[1]); // m
        Assert.Equal(replacementGroups[0].Indicators[0].Id, groupD.ChildSequence[2]); // n
    }

    [Fact]
    public void ReplaceIndicatorSequence_PreexistingIndicatorsMovedGroups_Success()
    {
        var originalGroups = new List<IndicatorGroup>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group a",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator a",
                        Name = "indicator_a",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator b",
                        Name = "indicator_b",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator c",
                        Name = "indicator_c",
                    },
                ],
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group b",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator d",
                        Name = "indicator_d",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator e",
                        Name = "indicator_e",
                    },
                ],
            },
            // Group c will be added in replacements - we skip it to check it gets appended after Group d, not alphabetically ordered
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group d",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator f",
                        Name = "indicator_f",
                    },
                ],
            },
        };

        var originalReleaseFile = new ReleaseFile
        {
            IndicatorSequence =
            [
                new IndicatorGroupSequenceEntry(
                    originalGroups[0].Id, // Group a
                    [
                        originalGroups[0].Indicators[0].Id, // a
                        originalGroups[0].Indicators[1].Id, // b
                        originalGroups[0].Indicators[2].Id, // c
                    ]
                ),
                new IndicatorGroupSequenceEntry(
                    originalGroups[1].Id, // Group b
                    [
                        originalGroups[1].Indicators[0].Id, // d
                        originalGroups[1].Indicators[1].Id, // e
                    ]
                ),
                new IndicatorGroupSequenceEntry(
                    originalGroups[2].Id, // Group d
                    [
                        originalGroups[2].Indicators[0].Id, // f
                    ]
                ),
            ],
        };

        // Define the set of indicator groups and indicators belonging to the replacement subject
        var replacementGroups = new List<IndicatorGroup>
        {
            // 'Group a' itself is removed, but indicators from it have been moved to other groups

            new() // 'Group b' now additionally has indicator a, d remains, e has moved to another group
            {
                Id = Guid.NewGuid(),
                Label = "Group b",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator d",
                        Name = "indicator_d",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator a",
                        Name = "indicator_a",
                    },
                ],
            },
            new() // 'Group c' is a new group with indicators b and c
            {
                Id = Guid.NewGuid(),
                Label = "Group c",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator c",
                        Name = "indicator_c",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator b",
                        Name = "indicator_b",
                    },
                ],
            },
            new() // 'Group d' additionally has indicator e
            {
                Id = Guid.NewGuid(),
                Label = "Group d",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator f",
                        Name = "indicator_f",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator e",
                        Name = "indicator_e",
                    },
                ],
            },
        };

        var mapping = GenerateMapping(
            originalDataSetId: Guid.NewGuid(),
            replacementDataSetId: Guid.NewGuid(),
            originalIndicatorGroups: originalGroups,
            replacementIndicatorGroups: replacementGroups
        );

        var updatedSequence = ReplacementServiceHelper.ReplaceIndicatorSequence(
            mapping: mapping,
            originalGroupIdToLabelMap: originalGroups.ToDictionary(g => g.Id, g => g.Label),
            replacementGroupLabelToIdMap: replacementGroups.ToDictionary(g => g.Label, g => g.Id),
            originalReleaseFile.IndicatorSequence
        );

        // Verify the updated sequence of indicators
        Assert.NotNull(updatedSequence);

        Assert.Equal(3, updatedSequence!.Count);

        // 'Group a' has been removed, so shouldn't appear in the replacement sequence

        var groupB = updatedSequence[0];
        Assert.Equal(replacementGroups[0].Id, groupB.Id);
        Assert.Equal(2, groupB.ChildSequence.Count);
        // d was in the group previously so is first, a has is new so is 2nd, e has been moved to another group
        Assert.Equal(replacementGroups[0].Indicators[0].Id, groupB.ChildSequence[0]); // d
        Assert.Equal(replacementGroups[0].Indicators[1].Id, groupB.ChildSequence[1]); // a

        // 'Group d' should be next, as 'Group c' is a new group
        var groupD = updatedSequence[1];
        Assert.Equal(replacementGroups[2].Id, groupD.Id);
        Assert.Equal(2, groupD.ChildSequence.Count);
        // f was in the original sequence, so should be first, while e has moved and is new to this group so
        // should be second
        Assert.Equal(replacementGroups[2].Indicators[0].Id, groupD.ChildSequence[0]); // f
        Assert.Equal(replacementGroups[2].Indicators[1].Id, groupD.ChildSequence[1]); // e

        // 'Group c' is new so it should be last in the new order
        var groupC = updatedSequence[2];
        Assert.Equal(replacementGroups[1].Id, groupC.Id);
        Assert.Equal(2, groupC.ChildSequence.Count);
        // b and c are both new, so should be added in alphabetical order
        Assert.Equal(replacementGroups[1].Indicators[1].Id, groupC.ChildSequence[0]); // b
        Assert.Equal(replacementGroups[1].Indicators[0].Id, groupC.ChildSequence[1]); // c
    }

    [Fact]
    public void ReplaceIndicatorSequence_NewIndicatorsAddedToGroups_Success()
    {
        var originalGroups = new List<IndicatorGroup>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group a",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator a",
                        Name = "indicator_a",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator b",
                        Name = "indicator_b",
                    },
                ],
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group b",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator c",
                        Name = "indicator_c",
                    },
                ],
            },
        };

        var originalReleaseFile = new ReleaseFile
        {
            IndicatorSequence =
            [
                new IndicatorGroupSequenceEntry(
                    originalGroups[0].Id, // Group a
                    [
                        originalGroups[0].Indicators[0].Id, // a
                        originalGroups[0].Indicators[1].Id, // b
                    ]
                ),
                new IndicatorGroupSequenceEntry(
                    originalGroups[1].Id, // Group b
                    [
                        originalGroups[1].Indicators[0].Id, // c
                    ]
                ),
            ],
        };

        var replacementGroups = new List<IndicatorGroup>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group a",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator c", // moved from 'Group b'
                        Name = "indicator_c",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator d", // new
                        Name = "indicator_d",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator a",
                        Name = "indicator_a",
                    },
                ],
            },
            // 'Group b' has been removed
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group c",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator e", // new
                        Name = "indicator_e",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator b", // moved from 'Group a'
                        Name = "indicator_b",
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator f", // new
                        Name = "indicator_f",
                    },
                ],
            },
        };

        var mapping = GenerateMapping(
            originalDataSetId: Guid.NewGuid(),
            replacementDataSetId: Guid.NewGuid(),
            originalIndicatorGroups: originalGroups,
            replacementIndicatorGroups: replacementGroups
        );

        var updatedSequence = ReplacementServiceHelper.ReplaceIndicatorSequence(
            mapping: mapping,
            originalGroupIdToLabelMap: originalGroups.ToDictionary(g => g.Id, g => g.Label),
            replacementGroupLabelToIdMap: replacementGroups.ToDictionary(g => g.Label, g => g.Id),
            originalReleaseFile.IndicatorSequence
        );

        // Verify the updated sequence of indicators
        Assert.NotNull(updatedSequence);

        Assert.Equal(2, updatedSequence!.Count);

        var groupA = updatedSequence[0];
        Assert.Equal(replacementGroups[0].Id, groupA.Id);
        Assert.Equal(3, groupA.ChildSequence.Count);
        // a was in the previously order so is first, c and d are new
        Assert.Equal(replacementGroups[0].Indicators[2].Id, groupA.ChildSequence[0]); // a
        Assert.Equal(replacementGroups[0].Indicators[0].Id, groupA.ChildSequence[1]); // c
        Assert.Equal(replacementGroups[0].Indicators[1].Id, groupA.ChildSequence[2]); // d

        // 'Group b' has been removed, so shouldn't appear in the replacement sequence

        var groupC = updatedSequence[1];
        Assert.Equal(replacementGroups[1].Id, groupC.Id);
        Assert.Equal(3, groupC.ChildSequence.Count);
        // c is new, has b moved from Group a and new indicators e and f,  was in the group previously so is first, a has is new so is 2nd, e has been moved to another group
        Assert.Equal(replacementGroups[1].Indicators[1].Id, groupC.ChildSequence[0]); // b
        Assert.Equal(replacementGroups[1].Indicators[0].Id, groupC.ChildSequence[1]); // e
        Assert.Equal(replacementGroups[1].Indicators[2].Id, groupC.ChildSequence[2]); // f
    }

    [Fact]
    public void ReplaceIndicatorSequence_GroupsWithNoIndicators_Success()
    {
        // The previous incarnation of ReplaceIndicatorSequence allowed indicator groups in IndicatorSequence without any
        // actual indicators. This shouldn't be possible, and afaik hasn't happened on environments, but I quickly wrote
        // this test to prove we're covered regardless.
        var originalGroups = new List<IndicatorGroup>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group a",
                Indicators = [],
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group b",
                Indicators = [],
            },
        };

        var originalReleaseFile = new ReleaseFile
        {
            IndicatorSequence =
            [
                new IndicatorGroupSequenceEntry(
                    originalGroups[0].Id, // Group a
                    []
                ),
                new IndicatorGroupSequenceEntry(
                    originalGroups[1].Id, // Group b
                    []
                ),
            ],
        };

        var replacementGroups = new List<IndicatorGroup>
        {
            // 'Group a' is removed
            new() // 'Group b' remains the same
            {
                Id = Guid.NewGuid(),
                Label = "Group b",
                Indicators = [],
            },
            new() // 'Group c' is new with no indicators
            {
                Id = Guid.NewGuid(),
                Label = "Group c",
                Indicators = [],
            },
            new() // 'Group d' is new with no indicators
            {
                Id = Guid.NewGuid(),
                Label = "Group d",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Name = "indicator_a",
                        Label = "Indicator a",
                    },
                ],
            },
        };

        var mapping = GenerateMapping(
            originalDataSetId: Guid.NewGuid(),
            replacementDataSetId: Guid.NewGuid(),
            originalIndicatorGroups: originalGroups,
            replacementIndicatorGroups: replacementGroups
        );

        var updatedSequence = ReplacementServiceHelper.ReplaceIndicatorSequence(
            mapping: mapping,
            originalGroupIdToLabelMap: originalGroups.ToDictionary(g => g.Id, g => g.Label),
            replacementGroupLabelToIdMap: replacementGroups.ToDictionary(g => g.Label, g => g.Id),
            originalReleaseFile.IndicatorSequence
        );

        Assert.NotNull(updatedSequence);

        var groupD = Assert.Single(updatedSequence);
        Assert.Equal(replacementGroups[2].Id, groupD.Id);

        var indicatorAId = Assert.Single(groupD.ChildSequence);
        Assert.Equal(replacementGroups[2].Indicators[0].Id, indicatorAId);
    }

    private static DataSetMapping GenerateMapping(
        Guid originalDataSetId,
        Guid replacementDataSetId,
        List<IndicatorGroup> originalIndicatorGroups = null,
        List<IndicatorGroup> replacementIndicatorGroups = null
    )
    {
        Dictionary<Guid, IndicatorMapping> indicatorsMappings = new();
        List<UnsetIndicator> candidateIndicatorReplacements = [];
        if (originalIndicatorGroups != null && replacementIndicatorGroups != null)
        {
            // emulates automapping that occurs from indicator groups when a mapping is initially generated
            indicatorsMappings = originalIndicatorGroups
                .SelectMany(group => group.Indicators, (group, indicator) => new { group, indicator })
                .ToDictionary(
                    pair => pair.indicator.Id,
                    pair =>
                    {
                        var replacementPair = replacementIndicatorGroups
                            .SelectMany(group => group.Indicators, (group, indicator) => new { group, indicator })
                            .SingleOrDefault(replacementPair =>
                                pair.group.Label == replacementPair.group.Label
                                && pair.indicator.Name == replacementPair.indicator.Name
                            );
                        return new IndicatorMapping
                        {
                            OriginalId = pair.indicator.Id,
                            OriginalLabel = pair.indicator.Label,
                            OriginalColumnName = pair.indicator.Name,
                            OriginalGroupId = pair.group.Id,
                            OriginalGroupLabel = pair.group.Label,
                            ReplacementId = replacementPair?.indicator.Id,
                            ReplacementLabel = replacementPair?.indicator.Label,
                            ReplacementGroupId = replacementPair?.group.Id,
                            Status = replacementPair == null ? MapStatus.Unset : MapStatus.AutoSet,
                        };
                    }
                );
            candidateIndicatorReplacements = replacementIndicatorGroups
                .SelectMany(group => group.Indicators, (group, indicator) => new { group, indicator })
                .ExceptBy(
                    indicatorsMappings.Values.Select(mapping => mapping.ReplacementId),
                    candidatePair => candidatePair.indicator.Id
                )
                .Select(pair => new UnsetIndicator
                {
                    Id = pair.indicator.Id,
                    Label = pair.indicator.Label,
                    GroupId = pair.group.Id,
                    GroupLabel = pair.group.Label,
                })
                .ToList();
        }

        return new DataSetMapping
        {
            OriginalDataSetId = originalDataSetId,
            ReplacementDataSetId = replacementDataSetId,
            IndicatorMappings = indicatorsMappings,
            UnsetReplacementIndicators = candidateIndicatorReplacements,
        };
    }
}
