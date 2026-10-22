#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.ViewModels;

public class DataReplacementPlanViewModelTests
{
    public class ToSummaryTests
    {
        [Fact]
        public void ValidDataBlock_RetainsIndicatorsAndLocations()
        {
            var indicatorGroups = new Dictionary<Guid, IndicatorGroupReplacementViewModel>
            {
                {
                    Guid.NewGuid(),
                    new IndicatorGroupReplacementViewModel(
                        id: Guid.NewGuid(),
                        label: "Indicator group",
                        indicators:
                        [
                            new IndicatorReplacementViewModel(Guid.NewGuid(), "Indicator", Guid.NewGuid(), "indicator"),
                        ]
                    )
                },
            };
            var locations = new Dictionary<string, LocationReplacementViewModel>
            {
                {
                    "Country",
                    new LocationReplacementViewModel(
                        label: "Country",
                        locationAttributes:
                        [
                            new LocationAttributeReplacementViewModel(
                                Guid.NewGuid(),
                                "E92000001",
                                "England",
                                Guid.NewGuid()
                            ),
                        ]
                    )
                },
            };
            var filters = new Dictionary<Guid, FilterReplacementViewModel>
            {
                {
                    Guid.NewGuid(),
                    new FilterReplacementViewModel(Guid.NewGuid(), Guid.NewGuid(), "Filter", "filter", [])
                },
            };
            var timePeriods = new TimePeriodRangeReplacementViewModel(
                start: new TimePeriodReplacementViewModel(true, TimeIdentifier.AcademicYear, 2020),
                end: new TimePeriodReplacementViewModel(true, TimeIdentifier.AcademicYear, 2021)
            );

            var dataBlock = new DataBlockReplacementPlanViewModel(
                Guid.NewGuid(),
                "Data block",
                filters,
                indicatorGroups,
                locations,
                timePeriods
            );

            Assert.True(dataBlock.Valid);

            var summary = dataBlock.ToSummary();

            Assert.Equal(dataBlock.Id, summary.Id);
            Assert.Equal(dataBlock.Name, summary.Name);
            Assert.Same(indicatorGroups, summary.IndicatorGroups);
            Assert.Same(locations, summary.Locations);
            Assert.Empty(summary.Filters);
            Assert.Null(summary.TimePeriods);
        }

        [Fact]
        public void InvalidDataBlock_ReturnsFullDetails()
        {
            var dataBlock = new DataBlockReplacementPlanViewModel(
                Guid.NewGuid(),
                "Data block",
                indicators: new Dictionary<Guid, IndicatorGroupReplacementViewModel>
                {
                    {
                        Guid.NewGuid(),
                        new IndicatorGroupReplacementViewModel(
                            id: Guid.NewGuid(),
                            label: "Indicator group",
                            indicators:
                            [
                                new IndicatorReplacementViewModel(Guid.NewGuid(), "Indicator", null, "indicator"),
                            ]
                        )
                    },
                }
            );

            Assert.False(dataBlock.Valid);
            Assert.Same(dataBlock, dataBlock.ToSummary());
        }

        [Fact]
        public void ValidFootnote_RetainsIndicators()
        {
            var indicatorGroups = new Dictionary<Guid, IndicatorGroupReplacementViewModel>
            {
                {
                    Guid.NewGuid(),
                    new IndicatorGroupReplacementViewModel(
                        id: Guid.NewGuid(),
                        label: "Indicator group",
                        indicators:
                        [
                            new IndicatorReplacementViewModel(Guid.NewGuid(), "Indicator", Guid.NewGuid(), "indicator"),
                        ]
                    )
                },
            };

            var footnote = new FootnoteReplacementPlanViewModel(
                Guid.NewGuid(),
                "Footnote",
                filters: [new FootnoteFilterReplacementViewModel(Guid.NewGuid(), "Filter", Guid.NewGuid())],
                indicatorGroups: indicatorGroups
            );

            Assert.True(footnote.Valid);

            var summary = footnote.ToSummary();

            Assert.Equal(footnote.Id, summary.Id);
            Assert.Equal(footnote.Content, summary.Content);
            Assert.Same(indicatorGroups, summary.IndicatorGroups);
            Assert.Empty(summary.Filters);
        }
    }
}
