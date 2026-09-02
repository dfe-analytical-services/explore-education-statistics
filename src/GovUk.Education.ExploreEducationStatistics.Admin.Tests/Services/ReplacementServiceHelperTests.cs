using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReplacementServiceHelperTests
{
    [Fact]
    public void ReplaceFilterSequence_Success()
    {
        var originalFilterAId = Guid.NewGuid();
        var originalGroupA1Id = Guid.NewGuid();
        var originalIndicatorA1AId = Guid.NewGuid(); // mapped to replacement IA1A
        var originalIndicatorA1BId = Guid.NewGuid(); // mapped to replacement IA1B

        var originalFilterBId = Guid.NewGuid();
        var originalGroupB1Id = Guid.NewGuid();
        var originalIndicatorB1AId = Guid.NewGuid(); // mapped to replacement IB1A
        var originalIndicatorB1BId = Guid.NewGuid(); // not in replacement

        var originalGroupB2Id = Guid.NewGuid(); // not in replacement
        var originalIndicatorB2AId = Guid.NewGuid();

        var originalFilterDId = Guid.NewGuid(); // not in replacement
        var originalGroupD1Id = Guid.NewGuid();
        var originalIndicatorD1AId = Guid.NewGuid();

        var replacementFilterAId = Guid.NewGuid();
        var replacementGroupA1Id = Guid.NewGuid();
        var replacementIndicatorA1AId = Guid.NewGuid();
        var replacementIndicatorA1BId = Guid.NewGuid();
        var replacementIndicatorA1CId = Guid.NewGuid(); // new to replacement

        var replacementFilterBId = Guid.NewGuid();
        var replacementGroupB1Id = Guid.NewGuid();
        var replacementIndicatorB1AId = Guid.NewGuid();

        var replacementGroupB3Id = Guid.NewGuid(); // new to replacement
        var replacementIndicatorB3AId = Guid.NewGuid();

        var replacementFilterCId = Guid.NewGuid(); // new to replacement
        var replacementGroupC1Id = Guid.NewGuid();
        var replacementIndicatorC1AId = Guid.NewGuid();

        var mapping = new DataSetMapping
        {
            FilterMappings = new Dictionary<Guid, FilterMapping>()
            {
                {
                    originalFilterAId,
                    new FilterMapping
                    {
                        OriginalId = originalFilterAId,

                        ReplacementId = replacementFilterAId,

                        FilterGroupMappings = new Dictionary<Guid, FilterGroupMapping>
                        {
                            {
                                originalGroupA1Id,
                                new FilterGroupMapping
                                {
                                    OriginalId = originalGroupA1Id,

                                    ReplacementId = replacementGroupA1Id,

                                    FilterItemMappings = new Dictionary<Guid, FilterItemMapping>
                                    {
                                        {
                                            originalIndicatorA1AId,
                                            new FilterItemMapping
                                            {
                                                OriginalId = originalIndicatorA1AId,
                                                ReplacementId = replacementIndicatorA1AId,
                                            }
                                        },
                                        {
                                            originalIndicatorA1BId,
                                            new FilterItemMapping
                                            {
                                                OriginalId = originalIndicatorA1BId,
                                                ReplacementId = replacementIndicatorA1BId,
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
                {
                    originalFilterBId,
                    new FilterMapping
                    {
                        OriginalId = originalFilterBId,
                        ReplacementId = replacementFilterBId,
                        FilterGroupMappings = new Dictionary<Guid, FilterGroupMapping>
                        {
                            {
                                originalGroupB1Id,
                                new FilterGroupMapping
                                {
                                    OriginalId = originalGroupB1Id,
                                    ReplacementId = replacementGroupB1Id,
                                    FilterItemMappings = new Dictionary<Guid, FilterItemMapping>
                                    {
                                        {
                                            originalIndicatorB1AId,
                                            new FilterItemMapping
                                            {
                                                OriginalId = originalIndicatorB1AId,
                                                ReplacementId = replacementIndicatorB1AId,
                                            }
                                        },
                                        {
                                            originalIndicatorB1BId,
                                            new FilterItemMapping
                                            {
                                                OriginalId = originalIndicatorB1BId,
                                                ReplacementId = null,
                                            }
                                        },
                                    },
                                }
                            },
                            {
                                originalGroupB2Id,
                                new FilterGroupMapping
                                {
                                    OriginalId = originalGroupB2Id,
                                    FilterItemMappings = new Dictionary<Guid, FilterItemMapping>
                                    {
                                        {
                                            originalIndicatorB2AId,
                                            new FilterItemMapping { OriginalId = originalIndicatorB2AId }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
                {
                    originalFilterDId,
                    new FilterMapping
                    {
                        OriginalId = originalFilterDId,
                        FilterGroupMappings = new Dictionary<Guid, FilterGroupMapping>
                        {
                            {
                                originalGroupD1Id,
                                new FilterGroupMapping
                                {
                                    OriginalId = originalGroupD1Id,
                                    FilterItemMappings = new Dictionary<Guid, FilterItemMapping>
                                    {
                                        {
                                            originalIndicatorD1AId,
                                            new FilterItemMapping { OriginalId = originalIndicatorD1AId }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
        };

        List<Filter> replacementFilters =
        [
            new Filter
            {
                Id = replacementFilterAId,
                FilterGroups =
                [
                    new FilterGroup
                    {
                        Id = replacementGroupA1Id,
                        FilterId = replacementFilterAId,
                        FilterItems =
                        [
                            new FilterItem { Id = replacementIndicatorA1AId, FilterGroupId = replacementGroupA1Id },
                            new FilterItem { Id = replacementIndicatorA1BId, FilterGroupId = replacementGroupA1Id },
                            new FilterItem
                            {
                                Id = replacementIndicatorA1CId,
                                FilterGroupId = replacementGroupA1Id,
                                Label = "Indicator A1C",
                            },
                        ],
                    },
                ],
            },
            new Filter
            {
                Id = replacementFilterBId,
                FilterGroups =
                [
                    new FilterGroup
                    {
                        Id = replacementGroupB1Id,
                        FilterId = replacementFilterBId,
                        FilterItems =
                        [
                            new FilterItem { Id = replacementIndicatorB1AId, FilterGroupId = replacementGroupB1Id },
                        ],
                    },
                    new FilterGroup
                    {
                        Id = replacementGroupB3Id,
                        FilterId = replacementFilterBId,
                        FilterItems =
                        [
                            new FilterItem { Id = replacementIndicatorB3AId, FilterGroupId = replacementGroupB3Id },
                        ],
                    },
                ],
            },
            new Filter
            {
                Id = replacementFilterCId,
                Label = "Filter C",
                FilterGroups =
                [
                    new FilterGroup
                    {
                        Id = replacementGroupC1Id,
                        FilterId = replacementFilterCId,
                        Label = "Group C1",
                        FilterItems =
                        [
                            new FilterItem
                            {
                                Id = replacementIndicatorC1AId,
                                FilterGroupId = replacementGroupC1Id,
                                Label = "Indicator C1A",
                            },
                        ],
                    },
                ],
            },
        ];

        List<FilterSequenceEntry> indicatorSequence =
        [
            new(
                Id: originalFilterAId,
                FilterGroupSequence:
                [
                    new FilterGroupSequenceEntry(
                        Id: originalGroupA1Id,
                        FilterItemSequence: [originalIndicatorA1AId, originalIndicatorA1BId]
                    ),
                ]
            ),
            new(
                Id: originalFilterBId,
                FilterGroupSequence:
                [
                    new FilterGroupSequenceEntry(
                        Id: originalGroupB1Id,
                        FilterItemSequence: [originalIndicatorB1AId, originalIndicatorB1BId]
                    ),
                ]
            ),
        ];

        var updatedSequence = ReplacementServiceHelper.ReplaceFilterSequence(
            indicatorSequence,
            mapping,
            replacementFilters
        );

        Assert.NotNull(updatedSequence);
        Assert.Equal(3, updatedSequence.Count);

        var filterA = updatedSequence[0];
        Assert.Equal(replacementFilterAId, filterA.Id);

        var groupA1 = Assert.Single(filterA.FilterGroupSequence);
        Assert.Equal(replacementGroupA1Id, groupA1.Id);

        Assert.Equal(3, groupA1.FilterItemSequence.Count);
        Assert.Equal(replacementIndicatorA1AId, groupA1.FilterItemSequence[0]);
        Assert.Equal(replacementIndicatorA1BId, groupA1.FilterItemSequence[1]);
        Assert.Equal(replacementIndicatorA1CId, groupA1.FilterItemSequence[2]);

        var filterB = updatedSequence[1];
        Assert.Equal(replacementFilterBId, filterB.Id);
        Assert.Equal(2, filterB.FilterGroupSequence.Count);

        var groupB1 = filterB.FilterGroupSequence[0];
        Assert.Equal(replacementGroupB1Id, groupB1.Id);
        var indicatorB1AId = Assert.Single(groupB1.FilterItemSequence);
        Assert.Equal(replacementIndicatorB1AId, indicatorB1AId);

        // groupB2 is not in the replacement

        var groupB3 = filterB.FilterGroupSequence[1];
        Assert.Equal(replacementGroupB3Id, groupB3.Id);
        var indicatorB3AId = Assert.Single(groupB3.FilterItemSequence);
        Assert.Equal(replacementIndicatorB3AId, indicatorB3AId);

        var filterC = updatedSequence[2];
        Assert.Equal(replacementFilterCId, filterC.Id);

        var groupC = Assert.Single(filterC.FilterGroupSequence);
        Assert.Equal(replacementGroupC1Id, groupC.Id);
        var indicatorC1A = Assert.Single(groupC.FilterItemSequence);
        Assert.Equal(replacementIndicatorC1AId, indicatorC1A);
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
            originalDataFileId: Guid.NewGuid(),
            replacementDataFileId: Guid.NewGuid(),
            originalIndicatorGroups: originalGroups,
            replacementIndicatorGroups: replacementGroups
        );

        var updatedSequence = ReplacementServiceHelper.ReplaceIndicatorSequence(
            mapping: mapping,
            originalGroupIdToLabelMap: originalGroups.ToDictionary(g => g.Id, g => g.Label),
            replacementGroupLabelToIdMap: replacementGroups.ToDictionary(g => g.Label, g => g.Id),
            originalReleaseFile.IndicatorSequence,
            replacementGroups
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
            // No Group c here, as it will be added in replacements - we check it gets appended after Group d, not alphabetically ordered
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
            originalDataFileId: Guid.NewGuid(),
            replacementDataFileId: Guid.NewGuid(),
            originalIndicatorGroups: originalGroups,
            replacementIndicatorGroups: replacementGroups
        );

        var updatedSequence = ReplacementServiceHelper.ReplaceIndicatorSequence(
            mapping: mapping,
            originalGroupIdToLabelMap: originalGroups.ToDictionary(g => g.Id, g => g.Label),
            replacementGroupLabelToIdMap: replacementGroups.ToDictionary(g => g.Label, g => g.Id),
            originalReleaseFile.IndicatorSequence,
            replacementGroups
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
            originalDataFileId: Guid.NewGuid(),
            replacementDataFileId: Guid.NewGuid(),
            originalIndicatorGroups: originalGroups,
            replacementIndicatorGroups: replacementGroups
        );

        var updatedSequence = ReplacementServiceHelper.ReplaceIndicatorSequence(
            mapping: mapping,
            originalGroupIdToLabelMap: originalGroups.ToDictionary(g => g.Id, g => g.Label),
            replacementGroupLabelToIdMap: replacementGroups.ToDictionary(g => g.Label, g => g.Id),
            originalReleaseFile.IndicatorSequence,
            replacementGroups
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
            originalDataFileId: Guid.NewGuid(),
            replacementDataFileId: Guid.NewGuid(),
            originalIndicatorGroups: originalGroups,
            replacementIndicatorGroups: replacementGroups
        );

        var updatedSequence = ReplacementServiceHelper.ReplaceIndicatorSequence(
            mapping: mapping,
            originalGroupIdToLabelMap: originalGroups.ToDictionary(g => g.Id, g => g.Label),
            replacementGroupLabelToIdMap: replacementGroups.ToDictionary(g => g.Label, g => g.Id),
            originalReleaseFile.IndicatorSequence,
            replacementGroups
        );

        Assert.NotNull(updatedSequence);

        var groupD = Assert.Single(updatedSequence);
        Assert.Equal(replacementGroups[2].Id, groupD.Id);

        var indicatorAId = Assert.Single(groupD.ChildSequence);
        Assert.Equal(replacementGroups[2].Indicators[0].Id, indicatorAId);
    }

    [Fact]
    public void ReplaceIndicatorSequence_MappedToNewGroupWithDifferentLabel_Success()
    {
        var originalGroupAId = Guid.NewGuid();
        var originalGroups = new List<IndicatorGroup>
        {
            new()
            {
                Id = originalGroupAId,
                Label = "Group a",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Name = "indicator_a",
                        Label = "Indicator A",
                        IndicatorGroupId = originalGroupAId,
                    },
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Name = "indicator_b",
                        Label = "Indicator B",
                        IndicatorGroupId = originalGroupAId,
                    },
                ],
            },
        };

        var originalReleaseFile = new ReleaseFile
        {
            IndicatorSequence =
            [
                new IndicatorGroupSequenceEntry(
                    Id: originalGroupAId, // Group a
                    ChildSequence:
                    [
                        originalGroups[0].Indicators[1].Id, // indicator b
                        originalGroups[0].Indicators[0].Id, // indicator a
                    ]
                ),
            ],
        };

        var replacementGroupAId = Guid.NewGuid();
        var replacementGroupBId = Guid.NewGuid();
        var replacementGroups = new List<IndicatorGroup>
        {
            new()
            {
                Id = replacementGroupAId,
                Label = "Group a",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Name = "indicator_a",
                        Label = "Indicator A",
                        IndicatorGroupId = replacementGroupAId,
                    },
                ],
            },
            new() // 'Group b' is a new group
            {
                Id = replacementGroupBId,
                Label = "Group b",
                Indicators =
                [
                    new Indicator
                    {
                        Id = Guid.NewGuid(),
                        Name = "indicator_b",
                        Label = "Indicator B",
                        IndicatorGroupId = replacementGroupBId,
                    },
                ],
            },
        };

        var mapping = new DataSetMapping
        {
            OriginalDataFileId = Guid.NewGuid(),
            ReplacementDataFileId = Guid.NewGuid(),
            IndicatorMappings = new Dictionary<Guid, IndicatorMapping>
            {
                {
                    // "Indicator A" mapped to "Indicator A"
                    // original/replacement have matching group label
                    originalGroups[0].Indicators[0].Id,
                    new IndicatorMapping
                    {
                        OriginalId = originalGroups[0].Indicators[0].Id,
                        OriginalLabel = originalGroups[0].Indicators[0].Label,
                        OriginalGroupId = originalGroups[0].Id,
                        OriginalGroupLabel = originalGroups[0].Label,
                        ReplacementId = replacementGroups[0].Indicators[0].Id,
                        ReplacementLabel = replacementGroups[0].Indicators[0].Label,
                        ReplacementGroupId = replacementGroups[0].Id,
                        ReplacementGroupLabel = replacementGroups[0].Label,
                        Status = MapStatus.AutoSet,
                    }
                },
                {
                    // "Indicator B" mapped to "Indicator B"
                    // original/replacement have different group label!
                    originalGroups[0].Indicators[1].Id,
                    new IndicatorMapping
                    {
                        OriginalId = originalGroups[0].Indicators[1].Id,
                        OriginalLabel = originalGroups[0].Indicators[1].Label,
                        OriginalGroupId = originalGroups[0].Id,
                        OriginalGroupLabel = originalGroups[0].Label,
                        ReplacementId = replacementGroups[1].Indicators[0].Id,
                        ReplacementLabel = replacementGroups[1].Indicators[0].Label,
                        ReplacementGroupId = replacementGroups[1].Id,
                        ReplacementGroupLabel = replacementGroups[1].Label,
                        Status = MapStatus.ManuallySet,
                    }
                },
            },
        };

        var updatedSequence = ReplacementServiceHelper.ReplaceIndicatorSequence(
            mapping: mapping,
            originalGroupIdToLabelMap: originalGroups.ToDictionary(g => g.Id, g => g.Label),
            replacementGroupLabelToIdMap: replacementGroups.ToDictionary(g => g.Label, g => g.Id),
            originalReleaseFile.IndicatorSequence,
            replacementGroups
        );

        Assert.NotNull(updatedSequence);

        Assert.Equal(2, updatedSequence.Count);
        var groupA = updatedSequence[0];
        var groupB = updatedSequence[1];
        Assert.Equal(replacementGroups[0].Id, groupA.Id);
        Assert.Equal(replacementGroups[1].Id, groupB.Id);

        var indicatorAId = Assert.Single(groupA.ChildSequence);
        Assert.Equal(replacementGroups[0].Indicators[0].Id, indicatorAId);

        var indicatorBId = Assert.Single(groupB.ChildSequence);
        Assert.Equal(replacementGroups[1].Indicators[0].Id, indicatorBId);
    }

    private static DataSetMapping GenerateMapping(
        Guid originalDataFileId,
        Guid replacementDataFileId,
        List<IndicatorGroup> originalIndicatorGroups = null,
        List<IndicatorGroup> replacementIndicatorGroups = null
    )
    {
        Dictionary<Guid, IndicatorMapping> indicatorsMappings = new();
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
        }

        return new DataSetMapping
        {
            OriginalDataFileId = originalDataFileId,
            ReplacementDataFileId = replacementDataFileId,
            IndicatorMappings = indicatorsMappings,
        };
    }
}
