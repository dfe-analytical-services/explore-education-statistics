#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Utils.FiltersMetaViewModelBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Utils;

public class FiltersMetaViewModelBuilderTests
{
    [Fact]
    public void BuildFiltersFromFilterItems_NoFilterItems()
    {
        Assert.Empty(BuildFiltersFromFilterItems(new List<FilterItem>()));
    }

    [Fact]
    public void BuildFilters_NoFilters()
    {
        Assert.Empty(BuildFilters(new List<Filter>()));
    }

    [Fact]
    public void BuildFilters_BuildsViewModel()
    {
        var filters = new List<Filter>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Hint = "Filter a hint",
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
                                Label = "Item c"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item d"
                            }
                        }
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Hint = "Filter b hint",
                Label = "Filter b",
                Name = "filter_b",
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group c",
                        FilterItems = new List<FilterItem>
                        {
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
                        Label = "Group d",
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
                            }
                        }
                    }
                }
            }
        };

        var result = (IDictionary<string, FilterMetaViewModel>) BuildFilters(filters);

        Assert.Equal(2, result.Count);

        // Verify filter A
        var filterA = Assert.Contains("FilterA", result);
        Assert.Equal(filters[0].Id, filterA.Id);
        Assert.Equal(filters[0].Hint, filterA.Hint);
        Assert.Equal(filters[0].Label, filterA.Legend);
        Assert.Equal(filters[0].Name, filterA.Name);
        Assert.Null(filterA.TotalValue);

        // Verify filter A's filter groups
        var filterAGroups = (IDictionary<string, FilterGroupMetaViewModel>) filterA.Options;
        Assert.Equal(2, filterAGroups.Count);

        // Verify filter group A
        var groupA = Assert.Contains("GroupA", filterAGroups);
        Assert.Equal(filters[0].FilterGroups[0].Id, groupA.Id);
        Assert.Equal(filters[0].FilterGroups[0].Label, groupA.Label);
        Assert.Equal(0, groupA.Order);

        // Verify filter group A's items
        Assert.Equal(2, groupA.Options.Count);

        Assert.Equal(filters[0].FilterGroups[0].FilterItems[0].Id, groupA.Options[0].Value);
        Assert.Equal(filters[0].FilterGroups[0].FilterItems[0].Label, groupA.Options[0].Label);

        Assert.Equal(filters[0].FilterGroups[0].FilterItems[1].Id, groupA.Options[1].Value);
        Assert.Equal(filters[0].FilterGroups[0].FilterItems[1].Label, groupA.Options[1].Label);

        // Verify filter group B
        var groupB = Assert.Contains("GroupB", filterAGroups);
        Assert.Equal(filters[0].FilterGroups[1].Id, groupB.Id);
        Assert.Equal(filters[0].FilterGroups[1].Label, groupB.Label);
        Assert.Equal(1, groupB.Order);

        // Verify filter group B's items
        Assert.Equal(2, groupB.Options.Count);

        Assert.Equal(filters[0].FilterGroups[1].FilterItems[0].Id, groupB.Options[0].Value);
        Assert.Equal(filters[0].FilterGroups[1].FilterItems[0].Label, groupB.Options[0].Label);

        Assert.Equal(filters[0].FilterGroups[1].FilterItems[1].Id, groupB.Options[1].Value);
        Assert.Equal(filters[0].FilterGroups[1].FilterItems[1].Label, groupB.Options[1].Label);

        // Verify filter B
        var filterB = Assert.Contains("FilterB", result);
        Assert.Equal(filters[1].Id, filterB.Id);
        Assert.Equal(filters[1].Hint, filterB.Hint);
        Assert.Equal(filters[1].Label, filterB.Legend);
        Assert.Equal(filters[1].Name, filterB.Name);
        Assert.Null(filterB.TotalValue);

        // Verify filter B's filter groups
        var filterBGroups = (IDictionary<string, FilterGroupMetaViewModel>) filterB.Options;
        Assert.Equal(2, filterBGroups.Count);

        // Verify filter group C
        var groupC = Assert.Contains("GroupC", filterBGroups);
        Assert.Equal(filters[1].FilterGroups[0].Id, groupC.Id);
        Assert.Equal(filters[1].FilterGroups[0].Label, groupC.Label);
        Assert.Equal(0, groupC.Order);

        // Verify filter group C's items
        Assert.Equal(2, groupC.Options.Count);

        Assert.Equal(filters[1].FilterGroups[0].FilterItems[0].Id, groupC.Options[0].Value);
        Assert.Equal(filters[1].FilterGroups[0].FilterItems[0].Label, groupC.Options[0].Label);

        Assert.Equal(filters[1].FilterGroups[0].FilterItems[1].Id, groupC.Options[1].Value);
        Assert.Equal(filters[1].FilterGroups[0].FilterItems[1].Label, groupC.Options[1].Label);

        // Verify filter group D
        var groupD = Assert.Contains("GroupD", filterBGroups);
        Assert.Equal(filters[1].FilterGroups[1].Id, groupD.Id);
        Assert.Equal(filters[1].FilterGroups[1].Label, groupD.Label);
        Assert.Equal(1, groupD.Order);

        // Verify filter group D's items
        Assert.Equal(2, groupD.Options.Count);

        Assert.Equal(filters[1].FilterGroups[1].FilterItems[0].Id, groupD.Options[0].Value);
        Assert.Equal(filters[1].FilterGroups[1].FilterItems[0].Label, groupD.Options[0].Label);

        Assert.Equal(filters[1].FilterGroups[1].FilterItems[1].Id, groupD.Options[1].Value);
        Assert.Equal(filters[1].FilterGroups[1].FilterItems[1].Label, groupD.Options[1].Label);
    }

    [Fact]
    public void BuildFilters_GetsTotalFilterItemId_TotalItemExistsAndSingleGroup()
    {
        var filter = new Filter
        {
            Id = Guid.NewGuid(),
            Label = "Filter a",
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
                            Label = "Total"
                        }
                    }
                }
            }
        };

        var result = (IDictionary<string, FilterMetaViewModel>) BuildFilters(ListOf(filter));

        Assert.Single(result);
        var filterA = Assert.Contains("FilterA", result);

        // Verify the total item id exists because there's only a single group despite it not being labelled "Total"
        Assert.Equal(filter.FilterGroups[0].FilterItems[1].Id, filterA.TotalValue);
    }

    [Fact]
    public void BuildFilters_GetsTotalFilterItemId_TotalItemExistsAndMultipleGroups()
    {
        var filter = new Filter
        {
            Id = Guid.NewGuid(),
            Label = "Filter a",
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
                            Label = "Item b"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Label = "Total"
                        }
                    }
                }
            }
        };

        var result = (IDictionary<string, FilterMetaViewModel>) BuildFilters(ListOf(filter));

        Assert.Single(result);
        var filterA = Assert.Contains("FilterA", result);

        // Verify the total item id is not present because there's multiple groups none of which are labelled "Total"
        // despite there being a "Total" filter item
        Assert.Null(filterA.TotalValue);
    }

    [Fact]
    public void BuildFilters_GetsTotalFilterItemId_TotalItemExistsInTotalGroup()
    {
        var filter = new Filter
        {
            Id = Guid.NewGuid(),
            Label = "Filter a",
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
                        // Filter item is labelled "Total" but is not in the "Total" group so is ignored
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Label = "Total"
                        }
                    }
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
                            Label = "Item b"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Label = "Total"
                        }
                    }
                }
            }
        };

        var result = (IDictionary<string, FilterMetaViewModel>) BuildFilters(ListOf(filter));

        Assert.Single(result);
        var filterA = Assert.Contains("FilterA", result);

        // Verify the total item id exists because there's a "Total" filter item in a "Total" group
        Assert.Equal(filter.FilterGroups[1].FilterItems[1].Id, filterA.TotalValue);
    }

    [Fact]
    public void BuildFilters_GetsTotalFilterItemId_TotalGroupExistsWithoutTotalItem()
    {
        var filter = new Filter
        {
            Id = Guid.NewGuid(),
            Label = "Filter a",
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
                        }
                    }
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
                            Label = "Item c"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Label = "Item d"
                        }
                    }
                }
            }
        };

        var result = (IDictionary<string, FilterMetaViewModel>) BuildFilters(ListOf(filter));

        Assert.Single(result);
        var filterA = Assert.Contains("FilterA", result);

        // Verify the total item id is not present because there's no "Total" filter item despite there being a
        // "Total" group
        Assert.Null(filterA.TotalValue);
    }

    [Fact]
    public void BuildFilters_OrdersByLabel()
    {
        var filters = new List<Filter>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter c",
                FilterGroups = new List<FilterGroup>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter a",
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group c",
                        FilterItems = new List<FilterItem>()
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group a",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item c"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item a"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Item b"
                            }
                        }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group b",
                        FilterItems = new List<FilterItem>()
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter b",
                FilterGroups = new List<FilterGroup>()
            }
        };

        var result = (IDictionary<string, FilterMetaViewModel>) BuildFilters(filters);

        Assert.Equal(3, result.Count);

        // Verify the filters are sorted by label
        var filterA = Assert.Contains("FilterA", result);
        var filterB = Assert.Contains("FilterB", result);
        var filterC = Assert.Contains("FilterC", result);

        Assert.Equal(0, filterA.Order);
        Assert.Equal(1, filterB.Order);
        Assert.Equal(2, filterC.Order);

        // Verify filter A's filter groups are sorted by label
        var filterAGroups = (IDictionary<string, FilterGroupMetaViewModel>) filterA.Options;
        Assert.Equal(3, filterAGroups.Count);
        var groupA = Assert.Contains("GroupA", filterAGroups);
        var groupB = Assert.Contains("GroupB", filterAGroups);
        var groupC = Assert.Contains("GroupC", filterAGroups);

        Assert.Equal(0, groupA.Order);
        Assert.Equal(1, groupB.Order);
        Assert.Equal(2, groupC.Order);

        // Verify filter group A's items are sorted by label
        Assert.Equal(3, groupA.Options.Count);
        Assert.Equal("Item a", groupA.Options[0].Label);
        Assert.Equal("Item b", groupA.Options[1].Label);
        Assert.Equal("Item c", groupA.Options[2].Label);
    }

    [Fact]
    public void BuildFilters_OrdersByLabel_TotalFirst()
    {
        var filter = new Filter
        {
            Id = Guid.NewGuid(),
            Label = "Filter a",
            FilterGroups = new List<FilterGroup>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Group a",
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
                            Label = "Item a"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Label = "Total"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Label = "Item b"
                        }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Group b",
                    FilterItems = new List<FilterItem>()
                }
            }
        };

        var result = (IDictionary<string, FilterMetaViewModel>) BuildFilters(ListOf(filter));

        Assert.Single(result);

        var filterA = Assert.Contains("FilterA", result);
        Assert.Equal(0, filterA.Order);

        // Verify the filter groups are sorted by label with the "Total" group first
        var filterAGroups = (IDictionary<string, FilterGroupMetaViewModel>) filterA.Options;
        Assert.Equal(3, filterAGroups.Count);
        var groupTotal = Assert.Contains("Total", filterAGroups);
        var groupA = Assert.Contains("GroupA", filterAGroups);
        var groupB = Assert.Contains("GroupB", filterAGroups);

        Assert.Equal(0, groupTotal.Order);
        Assert.Equal(1, groupA.Order);
        Assert.Equal(2, groupB.Order);

        // Verify the "Total" group's items are sorted by label with the "Total" item first 
        Assert.Equal(3, groupTotal.Options.Count);
        Assert.Equal("Total", groupTotal.Options[0].Label);
        Assert.Equal("Item a", groupTotal.Options[1].Label);
        Assert.Equal("Item b", groupTotal.Options[2].Label);

        Assert.Empty(groupA.Options);
        Assert.Empty(groupB.Options);
    }

    [Fact]
    public void BuildFilters_OrdersBySequence()
    {
        var filters = new List<Filter>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter a",
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
                        FilterItems = new List<FilterItem>()
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group c",
                        FilterItems = new List<FilterItem>()
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter b",
                FilterGroups = new List<FilterGroup>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Filter c",
                FilterGroups = new List<FilterGroup>()
            }
        };

        // Define a sequence to sort each of the filters / groups and items into the following order.
        // Include some random elements not in the input list since an ordering configuration contains all
        // filters / groups / items belonging to the Subject, whereas the input list is usually filtered:
        // - Filter c
        // - Filter a
        //   - Group c
        //   - Group a
        //     - Item c
        //     - Item a
        //     - Item b
        //   - Group b
        // - Filter b
        var ordering = new List<FilterSequenceEntry>
        {
            // A filter that does not exist in the input list
            new(
                Guid.NewGuid(),
                new List<FilterGroupSequenceEntry>
                {
                    new(
                        Guid.NewGuid(),
                        new List<Guid>
                        {
                            Guid.NewGuid()
                        }
                    )
                }
            ),
            // Filter c
            new(
                filters[2].Id,
                new List<FilterGroupSequenceEntry>()
            ),
            // Filter a
            new(
                filters[0].Id,
                new List<FilterGroupSequenceEntry>
                {
                    // Group c
                    new(
                        filters[0].FilterGroups[2].Id,
                        new List<Guid>()
                    ),
                    // Group a
                    new(
                        filters[0].FilterGroups[0].Id,
                        new List<Guid>
                        {
                            // Item c, Item a, Item b
                            filters[0].FilterGroups[0].FilterItems[2].Id,
                            filters[0].FilterGroups[0].FilterItems[0].Id,
                            filters[0].FilterGroups[0].FilterItems[1].Id
                        }
                    ),
                    // Group b
                    new(
                        filters[0].FilterGroups[1].Id,
                        new List<Guid>()
                    )
                }
            ),
            // Filter b
            new(
                filters[1].Id,
                new List<FilterGroupSequenceEntry>()
            ),
            // Another filter that does not exist in the input list
            new(
                Guid.NewGuid(),
                new List<FilterGroupSequenceEntry>
                {
                    new(
                        Guid.NewGuid(),
                        new List<Guid>
                        {
                            Guid.NewGuid()
                        }
                    )
                }
            )
        };

        var result = (IDictionary<string, FilterMetaViewModel>) BuildFilters(filters, ordering);

        Assert.Equal(3, result.Count);

        // Verify the filters are sorted by the defined sequence
        var filterA = Assert.Contains("FilterA", result);
        var filterB = Assert.Contains("FilterB", result);
        var filterC = Assert.Contains("FilterC", result);

        Assert.Equal(0, filterC.Order);
        Assert.Equal(1, filterA.Order);
        Assert.Equal(2, filterB.Order);

        // Verify filter A's filter groups are sorted by the defined sequence
        var filterAGroups = (IDictionary<string, FilterGroupMetaViewModel>) filterA.Options;
        Assert.Equal(3, filterAGroups.Count);
        var groupA = Assert.Contains("GroupA", filterAGroups);
        var groupB = Assert.Contains("GroupB", filterAGroups);
        var groupC = Assert.Contains("GroupC", filterAGroups);

        Assert.Equal(0, groupC.Order);
        Assert.Equal(1, groupA.Order);
        Assert.Equal(2, groupB.Order);

        // Verify filter group A's items are sorted by the defined sequence
        Assert.Equal(3, groupA.Options.Count);
        Assert.Equal("Item c", groupA.Options[0].Label);
        Assert.Equal("Item a", groupA.Options[1].Label);
        Assert.Equal("Item b", groupA.Options[2].Label);
    }
}
