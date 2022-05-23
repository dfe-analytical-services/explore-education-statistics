using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Xunit;

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
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item a"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item b"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item c"
                            }
                        }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group b",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item d"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item e"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item f"
                            }
                        }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group c",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item g"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item h"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item i"
                            }
                        }
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter b",
                Name = "filter_b",
                FilterGroups = new List<FilterGroup>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter c",
                Name = "filter_c",
                FilterGroups = new List<FilterGroup>()
            }
        };

        // Define a sequence for the original subject which is expected to be updated after the replacement
        var originalReleaseSubject = new ReleaseSubject
        {
            FilterSequence = new List<FilterSequenceEntry>
            {
                // Filter c
                new(
                    originalFilters[2].Id,
                    new List<FilterGroupSequenceEntry>()
                ),
                // Filter a
                new(originalFilters[0].Id,
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
                                originalFilters[0].FilterGroups[2].FilterItems[1].Id
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
                                originalFilters[0].FilterGroups[0].FilterItems[1].Id
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
                                originalFilters[0].FilterGroups[1].FilterItems[1].Id
                            }
                        )
                    }
                ),
                // Filter b
                new(
                    originalFilters[1].Id,
                    new List<FilterGroupSequenceEntry>()
                )
            }
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
                        FilterItems = new List<FilterItem>()
                    },
                    // Group 'Total' is added
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Total",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item m"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Total"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item l"
                            }
                        }
                    },
                    // 'Group d' is added
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group d",
                        FilterItems = new List<FilterItem>()
                    },
                    // 'Group b' is updated
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group b",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item d"
                            },
                            // 'Item e' is removed
                            // 'Item k' is added
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item k"
                            },
                            // 'Item j' is added
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item j"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item f"
                            }
                        }
                    },
                    // 'Group c' remains identical
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group c",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item g"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item h"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item i"
                            }
                        }
                    }
                }
            },
            // 'Filter b' is removed
            // 'Filter e' is added
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter e",
                Name = "filter_e",
                FilterGroups = new List<FilterGroup>()
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
                        FilterItems = new List<FilterItem>()
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Total",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item o"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Total"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item n"
                            }
                        }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group f",
                        FilterItems = new List<FilterItem>()
                    }
                }
            },
            // 'Filter c' remains identical
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter c",
                Name = "filter_c",
                FilterGroups = new List<FilterGroup>()
            }
        };

        var updatedSequence = ReplacementServiceHelper.ReplaceFilterSequence(
            originalFilters: originalFilters,
            replacementFilters: replacementFilters,
            originalReleaseSubject);

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
    public void ReplaceIndicatorSequence()
    {
        // Define a set of indicator groups and indicators belonging to the original subject
        var originalGroups = new List<IndicatorGroup>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group a",
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator a",
                        Name = "indicator_a"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator b",
                        Name = "indicator_b"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator c",
                        Name = "indicator_c"
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group b",
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator d",
                        Name = "indicator_d"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator e",
                        Name = "indicator_e"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator f",
                        Name = "indicator_f"
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group c",
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator g",
                        Name = "indicator_g"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator h",
                        Name = "indicator_h"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator i",
                        Name = "indicator_i"
                    }
                }
            }
        };

        // Define a sequence for the original subject which is expected to be updated after the replacement
        var originalReleaseSubject = new ReleaseSubject
        {
            IndicatorSequence = new List<IndicatorGroupSequenceEntry>
            {
                // Group c
                new(
                    originalGroups[2].Id,
                    new List<Guid>
                    {
                        // Indicator i, Indicator g, Indicator h
                        originalGroups[2].Indicators[2].Id,
                        originalGroups[2].Indicators[0].Id,
                        originalGroups[2].Indicators[1].Id
                    }
                ),
                // Group a
                new(
                    originalGroups[0].Id,
                    new List<Guid>
                    {
                        // Indicator c, Indicator a, Indicator b
                        originalGroups[0].Indicators[2].Id,
                        originalGroups[0].Indicators[0].Id,
                        originalGroups[0].Indicators[1].Id
                    }
                ),
                // Group b
                new(
                    originalGroups[1].Id,
                    new List<Guid>
                    {
                        // Indicator f, Indicator d, Indicator e
                        originalGroups[1].Indicators[2].Id,
                        originalGroups[1].Indicators[0].Id,
                        originalGroups[1].Indicators[1].Id
                    }
                )
            }
        };

        // Define the set of indicator groups and indicators belonging to the replacement subject
        var replacementGroups = new List<IndicatorGroup>
        {
            // 'Group a' is removed
            // 'Group e' is added
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group e",
                Indicators = new List<Indicator>()
            },
            // 'Group d' is added
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group d",
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator n",
                        Name = "indicator_n"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator l",
                        Name = "indicator_l"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator m",
                        Name = "indicator_m"
                    }
                }
            },
            // 'Group b' is updated
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group b",
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator d",
                        Name = "indicator_d"
                    },
                    // 'Indicator e' is removed
                    // 'Indicator k' is added
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator k",
                        Name = "indicator_k"
                    },
                    // 'Indicator j' is added
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator j",
                        Name = "indicator_j"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator f",
                        Name = "indicator_f"
                    }
                }
            },
            // 'Group c' remains identical
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group c",
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator g",
                        Name = "indicator_g"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator h",
                        Name = "indicator_h"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator i",
                        Name = "indicator_i"
                    }
                }
            }
        };

        var updatedSequence = ReplacementServiceHelper.ReplaceIndicatorSequence(
            originalIndicatorGroups: originalGroups,
            replacementIndicatorGroups: replacementGroups,
            originalReleaseSubject);

        // Verify the updated sequence of indicators
        Assert.NotNull(updatedSequence);

        Assert.Equal(4, updatedSequence!.Count);

        // 'Group c' was first in the original sequence and is identical in the replacement subject so it should be first
        var groupC = updatedSequence[0];
        Assert.Equal(replacementGroups[3].Id, groupC.Id);
        Assert.Equal(3, groupC.ChildSequence.Count);
        Assert.Equal(replacementGroups[3].Indicators[2].Id, groupC.ChildSequence[0]);
        Assert.Equal(replacementGroups[3].Indicators[0].Id, groupC.ChildSequence[1]);
        Assert.Equal(replacementGroups[3].Indicators[1].Id, groupC.ChildSequence[2]);

        // 'Group a' would've been the next group but has been removed

        // 'Group b' should be second based on the original sequence
        // Check 'Indicator e' was removed and both 'Indicator j' and 'Indicator k' have been appended in order
        var groupB = updatedSequence[1];
        Assert.Equal(replacementGroups[2].Id, groupB.Id);
        Assert.Equal(4, groupB.ChildSequence.Count);
        // 'Indicator f' is first from the original sequence
        Assert.Equal(replacementGroups[2].Indicators[3].Id, groupB.ChildSequence[0]);
        // 'Indicator d' is second from the original sequence 
        Assert.Equal(replacementGroups[2].Indicators[0].Id, groupB.ChildSequence[1]);
        // 'Indicator j' is appended first alphabetically
        Assert.Equal(replacementGroups[2].Indicators[2].Id, groupB.ChildSequence[2]);
        // 'Indicator k' is appended second alphabetically
        Assert.Equal(replacementGroups[2].Indicators[1].Id, groupB.ChildSequence[3]);

        // 'Group d' is new so it should be appended in order and its indicators should be ordered by label
        var groupD = updatedSequence[2];
        Assert.Equal(replacementGroups[1].Id, groupD.Id);
        Assert.Equal(3, groupD.ChildSequence.Count);
        // 'Indicator l' should be first alphabetically
        Assert.Equal(replacementGroups[1].Indicators[1].Id, groupD.ChildSequence[0]);
        // 'Indicator m' should be second alphabetically
        Assert.Equal(replacementGroups[1].Indicators[2].Id, groupD.ChildSequence[1]);
        // 'Indicator n' should be third alphabetically
        Assert.Equal(replacementGroups[1].Indicators[0].Id, groupD.ChildSequence[2]);

        // 'Group e' is new so it should be appended in order
        var groupE = updatedSequence[3];
        Assert.Equal(replacementGroups[0].Id, groupE.Id);
        Assert.Empty(groupE.ChildSequence);
    }
}
