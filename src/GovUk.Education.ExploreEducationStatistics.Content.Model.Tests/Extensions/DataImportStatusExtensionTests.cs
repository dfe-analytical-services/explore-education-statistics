using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Extensions
{
    public class DataImportStatusExtensionTests
    {
        private static readonly List<DataImportStatus> ExpectedFinishedStatuses = new List<DataImportStatus>
        {
            COMPLETE,
            FAILED,
            NOT_FOUND,
            CANCELLED
        };

        private static readonly List<DataImportStatus> ExpectedAbortingStatuses = new List<DataImportStatus>
        {
            CANCELLING
        };

        [Fact]
        public void FinishedAndAbortingStatuses()
        {
            EnumUtil.GetEnumValues<DataImportStatus>().ForEach(importStatus =>
            {
                var expectingToBeFinished = ExpectedFinishedStatuses.Contains(importStatus);
                var expectingToBeAborting = ExpectedAbortingStatuses.Contains(importStatus);

                Assert.Equal(expectingToBeFinished, importStatus.IsFinished());
                Assert.Equal(expectingToBeAborting, importStatus.IsAborting());
                Assert.Equal(expectingToBeFinished || expectingToBeAborting, importStatus.IsFinishedOrAborting());
            });
        }

        [Fact]
        public void CancellingFinishingStatus()
        {
            Assert.Equal(CANCELLED, CANCELLING.GetFinishingStateOfAbortProcess());
        }

        [Fact]
        public void NoOtherAbortingFinishStates()
        {
            EnumUtil.GetEnumValues<DataImportStatus>().ForEach(status =>
            {
                if (status != CANCELLING)
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => status.GetFinishingStateOfAbortProcess());
                }
            });
        }
    }
}
