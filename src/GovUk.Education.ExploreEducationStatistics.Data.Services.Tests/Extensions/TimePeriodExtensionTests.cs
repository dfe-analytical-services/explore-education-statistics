#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Extensions;

public class TimePeriodExtensionTests
{
    [Theory]
    [InlineData("2020_AY", 2020, AcademicYear)]
    [InlineData("2020_CY", 2020, CalendarYear)]
    [InlineData("2022_P2", 2022, FinancialYearPart2)]
    [InlineData("2022_AYQ4", 2022, AcademicYearQ4)]
    public void GetTimePeriodTuple(string timePeriod, int expectedYear, TimeIdentifier expectedTimeIdentifier)
    {
        var observation = new ObservationViewModel
        {
            TimePeriod = timePeriod
        };

        var tuple = observation.GetTimePeriodTuple();

        Assert.Equal(expectedYear, tuple.Year);
        Assert.Equal(expectedTimeIdentifier, tuple.TimeIdentifier);
    }

    [Theory]
    [InlineData("2020AY")]
    [InlineData("2020_AY_")]
    [InlineData("2020__AY")]
    [InlineData("AY_2020")]
    [InlineData("Invalid_AY")]
    [InlineData("2020_Invalid")]
    public void GetTimePeriodTuple_InvalidTimePeriodThrows(string timePeriod)
    {
        var observation = new ObservationViewModel
        {
            TimePeriod = timePeriod
        };

        Assert.Throws<ArgumentException>(() => observation.GetTimePeriodTuple());
    }
}
