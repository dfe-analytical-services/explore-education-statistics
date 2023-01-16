#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Utils.IndicatorsMetaViewModelBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Utils;

public class IndicatorsMetaViewModelBuilderTests
{
    [Fact]
    public void BuildIndicatorGroups_NoIndicatorGroups()
    {
        Assert.Empty(BuildIndicatorGroups(new List<IndicatorGroup>()));
    }

    [Fact]
    public void BuildIndicatorGroups_BuildsViewModel()
    {
        var indicatorGroups = new List<IndicatorGroup>
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
                        DecimalPlaces = 1,
                        Label = "Indicator a",
                        Name = "indicator_a",
                        Unit = Unit.Percent
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        DecimalPlaces = 2,
                        Label = "Indicator b",
                        Name = "indicator_b",
                        Unit = Unit.Pound
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
                        DecimalPlaces = 1,
                        Label = "Indicator c",
                        Name = "indicator_c",
                        Unit = Unit.Percent
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        DecimalPlaces = 2,
                        Label = "Indicator d",
                        Name = "indicator_d",
                        Unit = Unit.Pound
                    }
                }
            }
        };

        var result = (IDictionary<string, IndicatorGroupMetaViewModel>) BuildIndicatorGroups(indicatorGroups);

        Assert.Equal(2, result.Count);

        // Verify indicator group A
        var groupA = Assert.Contains("GroupA", result);
        Assert.Equal(indicatorGroups[0].Id, groupA.Id);
        Assert.Equal(indicatorGroups[0].Label, groupA.Label);
        Assert.Equal(0, groupA.Order);

        // Verify indicator group A's indicators
        Assert.Equal(2, groupA.Options.Count);

        Assert.Equal(indicatorGroups[0].Indicators[0].Id, groupA.Options[0].Value);
        Assert.Equal(indicatorGroups[0].Indicators[0].DecimalPlaces, groupA.Options[0].DecimalPlaces);
        Assert.Equal(indicatorGroups[0].Indicators[0].Label, groupA.Options[0].Label);
        Assert.Equal(indicatorGroups[0].Indicators[0].Name, groupA.Options[0].Name);
        Assert.Equal(indicatorGroups[0].Indicators[0].Unit, groupA.Options[0].Unit);

        Assert.Equal(indicatorGroups[0].Indicators[1].Id, groupA.Options[1].Value);
        Assert.Equal(indicatorGroups[0].Indicators[1].DecimalPlaces, groupA.Options[1].DecimalPlaces);
        Assert.Equal(indicatorGroups[0].Indicators[1].Label, groupA.Options[1].Label);
        Assert.Equal(indicatorGroups[0].Indicators[1].Name, groupA.Options[1].Name);
        Assert.Equal(indicatorGroups[0].Indicators[1].Unit, groupA.Options[1].Unit);

        // Verify indicator group B
        var groupB = Assert.Contains("GroupB", result);
        Assert.Equal(indicatorGroups[1].Id, groupB.Id);
        Assert.Equal(indicatorGroups[1].Label, groupB.Label);
        Assert.Equal(1, groupB.Order);

        // Verify indicator group B's indicators
        Assert.Equal(2, groupB.Options.Count);

        Assert.Equal(indicatorGroups[1].Indicators[0].Id, groupB.Options[0].Value);
        Assert.Equal(indicatorGroups[1].Indicators[0].DecimalPlaces, groupB.Options[0].DecimalPlaces);
        Assert.Equal(indicatorGroups[1].Indicators[0].Label, groupB.Options[0].Label);
        Assert.Equal(indicatorGroups[1].Indicators[0].Name, groupB.Options[0].Name);
        Assert.Equal(indicatorGroups[1].Indicators[0].Unit, groupB.Options[0].Unit);

        Assert.Equal(indicatorGroups[1].Indicators[1].Id, groupB.Options[1].Value);
        Assert.Equal(indicatorGroups[1].Indicators[1].DecimalPlaces, groupB.Options[1].DecimalPlaces);
        Assert.Equal(indicatorGroups[1].Indicators[1].Label, groupB.Options[1].Label);
        Assert.Equal(indicatorGroups[1].Indicators[1].Name, groupB.Options[1].Name);
        Assert.Equal(indicatorGroups[1].Indicators[1].Unit, groupB.Options[1].Unit);
    }

    [Fact]
    public void BuildIndicatorGroups_OrdersByLabel()
    {
        var indicatorGroups = new List<IndicatorGroup>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group c",
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator i"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator g"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator h"
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Group a",
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator c"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator a"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator b"
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
                        Label = "Indicator f"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator d"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator e"
                    }
                }
            }
        };

        var result = (IDictionary<string, IndicatorGroupMetaViewModel>) BuildIndicatorGroups(indicatorGroups);

        Assert.Equal(3, result.Count);

        // Verify the indicator groups are sorted by label
        var groupA = Assert.Contains("GroupA", result);
        var groupB = Assert.Contains("GroupB", result);
        var groupC = Assert.Contains("GroupC", result);

        Assert.Equal(0, groupA.Order);
        Assert.Equal(1, groupB.Order);
        Assert.Equal(2, groupC.Order);

        // Verify indicator group A's indicators are sorted by label
        Assert.Equal(3, groupA.Options.Count);
        Assert.Equal("Indicator a", groupA.Options[0].Label);
        Assert.Equal("Indicator b", groupA.Options[1].Label);
        Assert.Equal("Indicator c", groupA.Options[2].Label);

        // Verify indicator group B's indicators are sorted by label
        Assert.Equal(3, groupB.Options.Count);
        Assert.Equal("Indicator d", groupB.Options[0].Label);
        Assert.Equal("Indicator e", groupB.Options[1].Label);
        Assert.Equal("Indicator f", groupB.Options[2].Label);

        // Verify indicator group C's indicators are sorted by label
        Assert.Equal(3, groupC.Options.Count);
        Assert.Equal("Indicator g", groupC.Options[0].Label);
        Assert.Equal("Indicator h", groupC.Options[1].Label);
        Assert.Equal("Indicator i", groupC.Options[2].Label);
    }

    [Fact]
    public void BuildIndicatorGroups_OrdersBySequence()
    {
        var indicatorGroups = new List<IndicatorGroup>
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
                        Label = "Indicator a"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator b"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator c"
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
                        Label = "Indicator d"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator e"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator f"
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
                        Label = "Indicator g"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator h"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator i"
                    }
                }
            }
        };

        // Define a sequence to sort each of the indicator groups and their indicators into the following order.
        // Include some random elements not in the input list since an ordering configuration contains all
        // groups / indicators belonging to the Subject, whereas the input list is usually filtered:
        // - Group c
        //   - Indicator i
        //   - Indicator g
        //   - Indicator h
        // - Group a
        //   - Indicator c
        //   - Indicator a
        //   - Indicator b
        // - Group b
        //   - Indicator f
        //   - Indicator d
        //   - Indicator e
        var ordering = new List<IndicatorGroupSequenceEntry>
        {
            // A group that does not exist in the input list
            new(
                Guid.NewGuid(),
                new List<Guid>
                {
                    Guid.NewGuid()
                }
            ),
            // Group c
            new(
                indicatorGroups[2].Id,
                new List<Guid>
                {
                    // Indicator i, Indicator g, Indicator h
                    indicatorGroups[2].Indicators[2].Id,
                    indicatorGroups[2].Indicators[0].Id,
                    indicatorGroups[2].Indicators[1].Id
                }
            ),
            // Group a
            new(
                indicatorGroups[0].Id,
                new List<Guid>
                {
                    //Indicator c, Indicator a, Indicator b
                    indicatorGroups[0].Indicators[2].Id,
                    indicatorGroups[0].Indicators[0].Id,
                    indicatorGroups[0].Indicators[1].Id
                }
            ),
            // Group b
            new(
                indicatorGroups[1].Id,
                new List<Guid>
                {
                    // Indicator f, Indicator d, Indicator e
                    indicatorGroups[1].Indicators[2].Id,
                    indicatorGroups[1].Indicators[0].Id,
                    indicatorGroups[1].Indicators[1].Id
                }
            ),
            // Another group that does not exist in the input list
            new(
                Guid.NewGuid(),
                new List<Guid>
                {
                    Guid.NewGuid()
                }
            )
        };

        var result =
            (IDictionary<string, IndicatorGroupMetaViewModel>) BuildIndicatorGroups(indicatorGroups, ordering);

        Assert.Equal(3, result.Count);

        // Verify the indicator groups are sorted by the defined sequence
        var groupA = Assert.Contains("GroupA", result);
        var groupB = Assert.Contains("GroupB", result);
        var groupC = Assert.Contains("GroupC", result);

        Assert.Equal(0, groupC.Order);
        Assert.Equal(1, groupA.Order);
        Assert.Equal(2, groupB.Order);

        // Verify indicator group A's indicators are sorted by the defined sequence
        Assert.Equal(3, groupA.Options.Count);
        Assert.Equal("Indicator c", groupA.Options[0].Label);
        Assert.Equal("Indicator a", groupA.Options[1].Label);
        Assert.Equal("Indicator b", groupA.Options[2].Label);

        // Verify indicator group B's indicators are sorted by the defined sequence
        Assert.Equal(3, groupB.Options.Count);
        Assert.Equal("Indicator f", groupB.Options[0].Label);
        Assert.Equal("Indicator d", groupB.Options[1].Label);
        Assert.Equal("Indicator e", groupB.Options[2].Label);

        // Verify indicator group C's indicators are sorted by the defined sequence
        Assert.Equal(3, groupC.Options.Count);
        Assert.Equal("Indicator i", groupC.Options[0].Label);
        Assert.Equal("Indicator g", groupC.Options[1].Label);
        Assert.Equal("Indicator h", groupC.Options[2].Label);
    }

    [Fact]
    public void BuildIndicators_NoIndicators()
    {
        Assert.Empty(BuildIndicators(new List<Indicator>()));
    }

    [Fact]
    public void BuildIndicators_BuildsViewModel()
    {
        var indicators = new List<Indicator>
        {
            new()
            {
                Id = Guid.NewGuid(),
                DecimalPlaces = 1,
                Label = "Indicator a",
                Name = "indicator_a",
                Unit = Unit.Percent
            },
            new()
            {
                Id = Guid.NewGuid(),
                DecimalPlaces = 2,
                Label = "Indicator b",
                Name = "indicator_b",
                Unit = Unit.Pound
            }
        };

        var result = BuildIndicators(indicators);

        Assert.Equal(2, result.Count);

        Assert.Equal(indicators[0].Id, result[0].Value);
        Assert.Equal(indicators[0].DecimalPlaces, result[0].DecimalPlaces);
        Assert.Equal(indicators[0].Label, result[0].Label);
        Assert.Equal(indicators[0].Name, result[0].Name);
        Assert.Equal(indicators[0].Unit, result[0].Unit);

        Assert.Equal(indicators[1].Id, result[1].Value);
        Assert.Equal(indicators[1].DecimalPlaces, result[1].DecimalPlaces);
        Assert.Equal(indicators[1].Label, result[1].Label);
        Assert.Equal(indicators[1].Name, result[1].Name);
        Assert.Equal(indicators[1].Unit, result[1].Unit);
    }

    [Fact]
    public void BuildIndicators_OrdersByLabel()
    {
        var indicators = new List<Indicator>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Indicator c"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Indicator a"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Indicator b"
            }
        };

        var result = BuildIndicators(indicators);

        Assert.Equal(3, result.Count);

        // Verify the indicators are sorted by label
        Assert.Equal("Indicator a", result[0].Label);
        Assert.Equal("Indicator b", result[1].Label);
        Assert.Equal("Indicator c", result[2].Label);
    }

    [Fact]
    public void BuildIndicators_OrdersBySequence()
    {
        var indicators = new List<Indicator>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Indicator a"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Indicator b"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Indicator c"
            }
        };

        // Define a sequence to sort each of the indicators.
        // Include some random elements not in the input list since an ordering configuration contains all
        // indicators belonging to the Subject, whereas the input list is usually filtered:
        var ordering = new List<Guid>
        {
            Guid.NewGuid(),
            indicators[2].Id, // Indicator c
            indicators[0].Id, // Indicator a
            indicators[1].Id, // Indicator b
            Guid.NewGuid()
        };

        var result = BuildIndicators(indicators, ordering);

        Assert.Equal(3, result.Count);

        // Verify the indicators are sorted by the defined sequence
        Assert.Equal("Indicator c", result[0].Label);
        Assert.Equal("Indicator a", result[1].Label);
        Assert.Equal("Indicator b", result[2].Label);
    }
}
