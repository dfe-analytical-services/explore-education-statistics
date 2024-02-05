using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public class TimeIdentifierUtilsTests
{
    private static readonly HashSet<TimeIdentifier> ReleaseOnlyEnums = new()
    {
        TimeIdentifier.AcademicYearNationalTutoringProgramme
    };

    public class DataEnumsTests : TimeIdentifierUtilsTests
    {
        [Fact]
        public void DoesNotContainReleaseOnlyEnums()
        {
            var diff = EnumUtil.GetEnums<TimeIdentifier>()
                .Except(TimeIdentifierUtils.DataEnums)
                .ToHashSet();

            Assert.Equal(diff, ReleaseOnlyEnums);
        }
    }

    public class DataCodesTests : TimeIdentifierUtilsTests
    {
        [Fact]
        public void DoesNotContainReleaseOnlyCodes()
        {
            var diff = EnumUtil.GetEnumValuesSet<TimeIdentifier>()
                .Except(TimeIdentifierUtils.DataCodes)
                .ToHashSet();

            Assert.Equal(diff, ReleaseOnlyEnums.Select(e => e.GetEnumValue()).ToHashSet());
        }
    }

    public class DataLabelsTests : TimeIdentifierUtilsTests
    {
        [Fact]
        public void DoesNotContainReleaseOnlyLabels()
        {
            var diff = EnumUtil.GetEnumLabelsSet<TimeIdentifier>()
                .Except(TimeIdentifierUtils.DataLabels)
                .ToHashSet();

            Assert.Equal(diff, ReleaseOnlyEnums.Select(e => e.GetEnumLabel()).ToHashSet());
        }
    }
}
