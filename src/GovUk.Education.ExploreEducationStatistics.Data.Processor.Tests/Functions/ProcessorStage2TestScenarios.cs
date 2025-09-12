#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Functions;

public abstract class ScenarioWithFilterGrouping() : IProcessorStage2TestScenario
{
    private readonly Guid _subjectId;

    protected ScenarioWithFilterGrouping(Guid? subjectId = null) : this()
    {
        _subjectId = subjectId ?? Guid.NewGuid();
    }

    public abstract string GetFilenameUnderTest();

    public Guid GetSubjectId()
    {
        return _subjectId;
    }

    public List<Filter> GetExpectedFilters()
    {
        return ListOf(
            new Filter
            {
                Label = "Filter one",
                Name = "filter_one",
                GroupCsvColumn = null,
                FilterGroups = ListOf(
                    new FilterGroup
                    {
                        Label = "Default",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "Total"
                            })
                    }),
                SubjectId = _subjectId
            },
            new Filter
            {
                Label = "Filter two",
                Name = "filter_two",
                GroupCsvColumn = "filter_two_group",
                FilterGroups = ListOf(
                    new FilterGroup
                    {
                        Label = "One group",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "One"
                            })
                    },
                    new FilterGroup
                    {
                        Label = "Two group",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "Two"
                            })
                    }
                ),
                SubjectId = _subjectId
            },
            new Filter
            {
                Label = "Filter three",
                Name = "filter_three",
                GroupCsvColumn = null,
                FilterGroups = ListOf(
                    new FilterGroup
                    {
                        Label = "Default",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "Total"
                            })
                    }
                ),
                SubjectId = _subjectId
            },
            new Filter
            {
                Label = "Filter four",
                Name = "filter_four",
                GroupCsvColumn = "filter_four_group",
                FilterGroups = ListOf(
                    new FilterGroup
                    {
                        Label = "One group",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "One"
                            },
                            new FilterItem
                            {
                                Label = "Two"
                            })
                    },
                    new FilterGroup
                    {
                        Label = "Two group",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "One"
                            },
                            new FilterItem
                            {
                                Label = "Two"
                            })
                    }
                ),
                SubjectId = _subjectId
            });
    }

    public List<IndicatorGroup> GetExpectedIndicatorGroups()
    {
        return ListOf(
            new IndicatorGroup
            {
                Label = "Default",
                Indicators = ListOf(
                    new Indicator
                    {
                        Label = "Indicator one"
                    }),
                SubjectId = _subjectId
            });
    }

    public List<Location> GetExpectedLocations()
    {
        return ListOf(
            new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = new Country("E92000001", "England"),
                LocalAuthority = new LocalAuthority("E08000025", "330", "Birmingham")
            },
            new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = new Country("E92000001", "England"),
                LocalAuthority = new LocalAuthority("E08000016", "370", "Barnsley")
            },
            new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = new Country("E92000001", "England"),
                LocalAuthority = new LocalAuthority("E09000011", "203", "Greenwich")
            },
            new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = new Country("E92000001", "England"),
                LocalAuthority = new LocalAuthority("E09000007", "202", "Camden")
            });
    }

    public List<AutoSelectFilterValue> GetAutoSelectFilterValues() =>
    [
        new AutoSelectFilterValue("Filter one", "Total"),
        new AutoSelectFilterValue("Filter three", "Total"),
    ];
}

public class OrderingCsvStage2Scenario(Guid? subjectId = null) : ScenarioWithFilterGrouping(subjectId)
{
    public override string GetFilenameUnderTest()
    {
        return "ordering-test-4.csv";
    }
}

public class AdditionalFiltersAndIndicatorsScenario : IProcessorStage2TestScenario
{
    private readonly Guid _subjectId;

    public AdditionalFiltersAndIndicatorsScenario(Guid? subjectId = null)
    {
        _subjectId = subjectId ?? Guid.NewGuid();
    }

    public string GetFilenameUnderTest()
    {
        return "additional-filters-and-indicators.csv";
    }

    public Guid GetSubjectId()
    {
        return _subjectId;
    }

    public List<Filter> GetExpectedFilters()
    {
        return ListOf(
            new Filter
            {
                Label = "Filter one",
                Name = "filter_one",
                GroupCsvColumn = null,
                FilterGroups = ListOf(
                    new FilterGroup
                    {
                        Label = "Default",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "Total"
                            })
                    }),
                SubjectId = _subjectId
            },
            new Filter
            {
                Label = "Additional filter",
                Name = "additional_filter",
                FilterGroups = ListOf(
                    new FilterGroup
                    {
                        Label = "Default",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "Not specified"
                            })
                    }
                ),
                SubjectId = _subjectId
            });
    }

    public List<IndicatorGroup> GetExpectedIndicatorGroups()
    {
        return ListOf(
            new IndicatorGroup
            {
                Label = "Default",
                Indicators = ListOf(
                    new Indicator
                    {
                        Label = "Indicator one"
                    },
                    new Indicator
                    {
                        Label = "Additional indicator"
                    }),
                SubjectId = _subjectId
            });
    }

    public List<Location> GetExpectedLocations()
    {
        return ListOf(
            new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = new Country("E92000001", "England"),
                LocalAuthority = new LocalAuthority("E08000025", "330", "Birmingham")
            },
            new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = new Country("E92000001", "England"),
                LocalAuthority = new LocalAuthority("E08000016", "370", "Barnsley")
            },
            new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = new Country("E92000001", "England"),
                LocalAuthority = new LocalAuthority("E09000011", "203", "Greenwich")
            },
            new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = new Country("E92000001", "England"),
                LocalAuthority = new LocalAuthority("E09000007", "202", "Camden")
            });
    }

    public List<AutoSelectFilterValue> GetAutoSelectFilterValues() =>
        [new AutoSelectFilterValue("Filter one", "Total")];
}

public class SpecialFilterItemsScenario : IProcessorStage2TestScenario
{
    private readonly Guid _subjectId;

    public SpecialFilterItemsScenario(Guid? subjectId = null)
    {
        _subjectId = subjectId ?? Guid.NewGuid();
    }

    public string GetFilenameUnderTest()
    {
        return "small-csv-with-special-data.csv";
    }

    public Guid GetSubjectId()
    {
        return _subjectId;
    }

    public List<Filter> GetExpectedFilters()
    {
        return ListOf(
            new Filter
            {
                Label = "Filter one",
                Name = "filter_one",
                GroupCsvColumn = null,
                FilterGroups = ListOf(
                    new FilterGroup
                    {
                        Label = "Default",
                        FilterItems = ListOf(
                            // Despite various differences in case and additional whitespace,
                            // only one "Value 1" Filter Item should be created for them all.
                            new FilterItem
                            {
                                Label = "Value 1"
                            },
                            // Some values for "Filter one" are blank. They will be assigned the
                            // special "Not specified" value.
                            new FilterItem
                            {
                                Label = "Not specified"
                            })
                    }),
                SubjectId = _subjectId
            });
    }

    public List<IndicatorGroup> GetExpectedIndicatorGroups()
    {
        return ListOf(
            new IndicatorGroup
            {
                Label = "Default",
                Indicators = ListOf(
                    new Indicator
                    {
                        Label = "Indicator one"
                    }),
                SubjectId = _subjectId
            });
    }

    public List<Location> GetExpectedLocations()
    {
        return ListOf(
            new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = new Country("E92000001", "England"),
                LocalAuthority = new LocalAuthority("E08000025", "330", "Birmingham")
            },
            new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = new Country("E92000001", "England"),
                LocalAuthority = new LocalAuthority("E08000016", "370", "Barnsley")
            },
            new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = new Country("E92000001", "England"),
                LocalAuthority = new LocalAuthority("E09000011", "203", "Greenwich")
            });
    }

    public List<AutoSelectFilterValue> GetAutoSelectFilterValues() => [];
}

public class AutoSelectFilterItemCsvStage2Scenario : IProcessorStage2TestScenario
{
    private readonly Guid _subjectId;

    public AutoSelectFilterItemCsvStage2Scenario(Guid? subjectId = null)
    {
        _subjectId = subjectId ?? Guid.NewGuid();
    }

    public string GetFilenameUnderTest()
    {
        return "auto-select-filter-items.csv";
    }

    public Guid GetSubjectId()
    {
        return _subjectId;
    }

    public List<Filter> GetExpectedFilters()
    {
        return ListOf(
            new Filter
            {
                Label = "Filter with Total",
                Name = "filter_with_total",
                GroupCsvColumn = null,
                AutoSelectFilterItemLabel = "Total",
                // AutoSelectFilterItemId should also be set, but we cannot know what it is here
                FilterGroups = ListOf(
                    new FilterGroup
                    {
                        Label = "Default",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "OneOne"
                            },
                            new FilterItem
                            {
                                Label = "Total"
                            },
                            new FilterItem
                            {
                                Label = "OneThree"
                            })
                    }),
                SubjectId = _subjectId
            },
            new Filter
            {
                Label = "Filter with default TwoTwo",
                Name = "filter_with_default",
                GroupCsvColumn = null,
                AutoSelectFilterItemLabel = "TwoTwo",
                // AutoSelectFilterItemId should also be set, but we cannot know what it is here
                FilterGroups = ListOf(
                    new FilterGroup
                    {
                        Label = "Default",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "TwoOne"
                            },
                            new FilterItem
                            {
                                Label = "TwoTwo"
                            },
                            new FilterItem
                            {
                                Label = "TwoThree"
                            })
                    }
                ),
                SubjectId = _subjectId
            },
            new Filter
            {
                Label = "Filter with no default",
                Name = "filter_no_default",
                GroupCsvColumn = null,
                FilterGroups = ListOf(
                    new FilterGroup
                    {
                        Label = "Default",
                        FilterItems = ListOf(
                            new FilterItem
                            {
                                Label = "ThreeOne"
                            },
                            new FilterItem
                            {
                                Label = "ThreeTwo"
                            },
                            new FilterItem
                            {
                                Label = "ThreeThree"
                            })
                    }
                ),
                SubjectId = _subjectId
            });
    }

    public List<IndicatorGroup> GetExpectedIndicatorGroups()
    {
        return ListOf(
            new IndicatorGroup
            {
                Label = "Default",
                Indicators = ListOf(
                    new Indicator
                    {
                        Label = "Indicator one"
                    }),
                SubjectId = _subjectId
            });
    }

    public List<Location> GetExpectedLocations()
    {
        return ListOf(
            new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = new Country("E92000001", "England"),
                LocalAuthority = new LocalAuthority("E08000025", "330", "Birmingham")
            },
            new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = new Country("E92000001", "England"),
                LocalAuthority = new LocalAuthority("E08000016", "370", "Barnsley")
            });
    }

    public List<AutoSelectFilterValue> GetAutoSelectFilterValues() =>
    [
        new AutoSelectFilterValue("Filter with Total", "Total"),
        new AutoSelectFilterValue("Filter with default TwoTwo", "TwoTwo"),
    ];
}

public record AutoSelectFilterValue(string FilterLabel, string AutoSelectFilterItemLabel);

public class GroupingFiltersAsRowsInMetaFileScenario(Guid? subjectId = null) : ScenarioWithFilterGrouping(subjectId)
{
    public override string GetFilenameUnderTest()
    { 
        // This file contains two rows "filter_four_group" and
        // "filter_two_group" which are not treated as filters
        // by the test data set up by OrderingCsvStage2Scenario.
        return "grouping_filters_as_rows.csv";
    }
}

public interface IProcessorStage2TestScenario
{
    string GetFilenameUnderTest();

    Guid GetSubjectId();

    List<Filter> GetExpectedFilters();

    List<IndicatorGroup> GetExpectedIndicatorGroups();

    List<Location> GetExpectedLocations();

    List<AutoSelectFilterValue> GetAutoSelectFilterValues();
}
