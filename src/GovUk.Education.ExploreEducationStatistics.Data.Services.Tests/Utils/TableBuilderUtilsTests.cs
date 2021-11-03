#nullable enable
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Utils.TableBuilderUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Utils
{
    public class TableBuilderUtilsTests
    {
        [Fact]
        public void MaximumTableCellCount_NoFilters_SingleIndicatorLocationAndTimePeriod()
        {
            /*
             * 2011  Bury  cell1
             */
            Assert.Equal(
                1,
                MaximumTableCellCount(
                    countOfIndicators: 1,
                    countOfLocations: 1,
                    countOfTimePeriods: 1));
        }

        [Fact]
        public void MaximumTableCellCount_NoFilters_MultipleIndicators()
        {
            /*
             * 2011  Bury  cell1  cell2  cell 3
             */
            Assert.Equal(
                3,
                MaximumTableCellCount(
                    countOfIndicators: 3,
                    countOfLocations: 1,
                    countOfTimePeriods: 1));
        }

        [Fact]
        public void MaximumTableCellCount_NoFilters_MultipleLocations()
        {
            /*
             * 2011  Bury  cell1
             * 2011  Kent  cell2
             * 2011  York  cell3
             */
            Assert.Equal(
                3,
                MaximumTableCellCount(
                    countOfIndicators: 1,
                    countOfLocations: 3,
                    countOfTimePeriods: 1));
        }

        [Fact]
        public void MaximumTableCellCount_NoFilters_MultipleTimePeriods()
        {
            /*
             * 2011  Bury  cell1
             * 2012  Bury  cell2
             * 2013  Bury  cell3
             */
            Assert.Equal(
                3,
                MaximumTableCellCount(
                    countOfIndicators: 1,
                    countOfLocations: 1,
                    countOfTimePeriods: 3));
        }

        [Fact]
        public void MaximumTableCellCount_NoFilters_MultipleIndicatorsLocationsAndTimePeriods()
        {
            /*
             * 2011  Bury   cell1   cell2
             * 2011  York   cell3   cell4
             * 2012  Bury   cell5   cell6
             * 2012  York   cell7   cell8
             * 2013  Bury   cell9  cell10
             * 2013  York  cell11  cell12
             */
            Assert.Equal(
                12,
                MaximumTableCellCount(
                    countOfIndicators: 2,
                    countOfLocations: 2,
                    countOfTimePeriods: 3));
        }

        [Fact]
        public void MaximumTableCellCount_SingleFilterWithSingleFilterItem()
        {
            /*
             * 2011  Bury  Year1   cell1   cell2
             * 2011  York  Year1   cell3   cell4
             * 2012  Bury  Year1   cell5   cell6
             * 2012  York  Year1   cell7   cell8
             * 2013  Bury  Year1   cell9  cell10
             * 2013  York  Year1  cell11  cell12
             */
            Assert.Equal(
                12,
                MaximumTableCellCount(
                    countOfIndicators: 2,
                    countOfLocations: 2,
                    countOfTimePeriods: 3,
                    countsOfFilterItemsByFilter: ListOf(1)));
        }

        [Fact]
        public void MaximumTableCellCount_MultipleFiltersWithSingleFilterItems()
        {
            /*
             * 2011  Bury  Year1  Male   cell1   cell2
             * 2011  York  Year1  Male   cell3   cell4
             * 2012  Bury  Year1  Male   cell5   cell6
             * 2012  York  Year1  Male   cell7   cell8
             * 2013  Bury  Year1  Male   cell9  cell10
             * 2013  York  Year1  Male  cell11  cell12
             */
            Assert.Equal(
                12,
                MaximumTableCellCount(
                    countOfIndicators: 2,
                    countOfLocations: 2,
                    countOfTimePeriods: 3,
                    countsOfFilterItemsByFilter: ListOf(1, 1)));
        }

        [Fact]
        public void MaximumTableCellCount_SingleFilterWithMultipleFilterItems()
        {
            /*
             * 2011  Bury  Year1   cell1   cell2
             * 2011  York  Year1   cell3   cell4
             * 2012  Bury  Year1   cell5   cell6
             * 2012  York  Year1   cell7   cell8
             * 2013  Bury  Year1   cell9  cell10
             * 2013  York  Year1  cell11  cell12
             * 2011  Bury  Year2  cell13  cell14
             * 2011  York  Year2  cell15  cell16
             * 2012  Bury  Year2  cell17  cell18
             * 2012  York  Year2  cell19  cell20
             * 2013  Bury  Year2  cell21  cell22
             * 2013  York  Year2  cell23  cell24
             * 2011  Bury  Year3  cell25  cell26
             * 2011  York  Year3  cell27  cell28
             * 2012  Bury  Year3  cell29  cell30
             * 2012  York  Year3  cell31  cell32
             * 2013  Bury  Year3  cell33  cell34
             * 2013  York  Year3  cell35  cell36
             */
            Assert.Equal(
                36,
                MaximumTableCellCount(
                    countOfIndicators: 2,
                    countOfLocations: 2,
                    countOfTimePeriods: 3,
                    countsOfFilterItemsByFilter: ListOf(3)));
        }

        [Fact]
        public void MaximumTableCellCount_MultipleFiltersWithMultipleFilterItems()
        {
            /*
             * 2011  Bury  Year1    Male   cell1   cell2
             * 2011  York  Year1    Male   cell3   cell4
             * 2012  Bury  Year1    Male   cell5   cell6
             * 2012  York  Year1    Male   cell7   cell8
             * 2013  Bury  Year1    Male   cell9  cell10
             * 2013  York  Year1    Male  cell11  cell12
             * 2011  Bury  Year2    Male  cell13  cell14
             * 2011  York  Year2    Male  cell15  cell16
             * 2012  Bury  Year2    Male  cell17  cell18
             * 2012  York  Year2    Male  cell19  cell20
             * 2013  Bury  Year2    Male  cell21  cell22
             * 2013  York  Year2    Male  cell23  cell24
             * 2011  Bury  Year3    Male  cell25  cell26
             * 2011  York  Year3    Male  cell27  cell28
             * 2012  Bury  Year3    Male  cell29  cell30
             * 2012  York  Year3    Male  cell31  cell32
             * 2013  Bury  Year3    Male  cell33  cell34
             * 2013  York  Year3    Male  cell35  cell36
             * 2011  Bury  Year1  Female  cell37  cell38
             * 2011  York  Year1  Female  cell39  cell40
             * 2012  Bury  Year1  Female  cell41  cell42
             * 2012  York  Year1  Female  cell43  cell44
             * 2013  Bury  Year1  Female  cell45  cell46
             * 2013  York  Year1  Female  cell47  cell48
             * 2011  Bury  Year2  Female  cell49  cell50
             * 2011  York  Year2  Female  cell51  cell52
             * 2012  Bury  Year2  Female  cell53  cell54
             * 2012  York  Year2  Female  cell55  cell56
             * 2013  Bury  Year2  Female  cell57  cell58
             * 2013  York  Year2  Female  cell59  cell60
             * 2011  Bury  Year3  Female  cell61  cell62
             * 2011  York  Year3  Female  cell63  cell64
             * 2012  Bury  Year3  Female  cell65  cell66
             * 2012  York  Year3  Female  cell67  cell68
             * 2013  Bury  Year3  Female  cell69  cell70
             * 2013  York  Year3  Female  cell71  cell72
             */
            Assert.Equal(
                72,
                MaximumTableCellCount(
                    countOfIndicators: 2,
                    countOfLocations: 2,
                    countOfTimePeriods: 3,
                    countsOfFilterItemsByFilter: ListOf(3, 2)));
        }
    }
}
