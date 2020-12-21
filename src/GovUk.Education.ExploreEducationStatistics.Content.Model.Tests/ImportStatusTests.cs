using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.IStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class ImportStatusTests
    {
        private static readonly List<IStatus> ExpectedFinishedStatuses = new List<IStatus>
        {
            COMPLETE,
            FAILED,
            NOT_FOUND,
            CANCELLED
        };

        private static readonly List<IStatus> ExpectedAbortingStatuses = new List<IStatus>
        {
            CANCELLING,
        };

        [Fact]
        public void FinishedAndAbortingStatuses()
        {
            EnumUtil.GetEnumValues<IStatus>().ForEach(status =>
            {
                var importStatus = new ImportStatus
                {
                    Status = status
                };

                var expectingToBeFinished = ExpectedFinishedStatuses.Contains(status);
                var expectingToBeAborting = ExpectedAbortingStatuses.Contains(status);

                Assert.Equal(expectingToBeFinished, importStatus.IsFinished());
                Assert.Equal(expectingToBeAborting, importStatus.IsAborting());
                Assert.Equal(expectingToBeFinished || expectingToBeAborting, importStatus.IsFinishedOrAborting());
            });
        }
        
        [Fact]
        public void CancellingFinishingStatus()
        {
            var importStatus = new ImportStatus
            {
                Status = CANCELLING
            };
            
            Assert.Equal(CANCELLED, importStatus.GetFinishingStateOfAbortProcess());
        }

        [Fact]
        public void NoOtherAbortingFinishStates()
        {
            EnumUtil.GetEnumValues<IStatus>().ForEach(status =>
            {
                var importStatus = new ImportStatus
                {
                    Status = status
                };

                if (status != CANCELLING)
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => importStatus.GetFinishingStateOfAbortProcess());
                }    
            });
        }
    }
}