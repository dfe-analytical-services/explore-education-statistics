using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Functions;

public class OrderingCsvStage2Scenario : IProcessorStage2TestScenario
{
    private readonly Guid _subjectId;

    public OrderingCsvStage2Scenario(Guid? subjectId = null)
    {
        _subjectId = subjectId ?? Guid.NewGuid();
    }

    public string GetFilenameUnderTest()
    {
        return "ordering-test-4.csv";
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
}

public interface IProcessorStage2TestScenario
{
    string GetFilenameUnderTest();

    Guid GetSubjectId();

    List<Filter> GetExpectedFilters();

    List<IndicatorGroup> GetExpectedIndicatorGroups();

    List<Location> GetExpectedLocations();
}