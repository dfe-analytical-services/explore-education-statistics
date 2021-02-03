using System.Collections.Generic;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ImportStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class ImportTests
    {
        private static readonly List<ImportStatus> StatusesWithZeroProgress = new List<ImportStatus>
        {
            CANCELLING,
            QUEUED,
            PROCESSING_ARCHIVE_FILE,
            FAILED,
            NOT_FOUND
        };

        private const int StagePercentageComplete = 50;

        [Fact]
        public void PercentageComplete_Stage1()
        {
            var import = new Import
            {
                StagePercentageComplete = StagePercentageComplete,
                Status = STAGE_1
            };

            Assert.Equal(StagePercentageComplete * 0.1, import.PercentageComplete());
        }

        [Fact]
        public void PercentageComplete_Stage2()
        {
            var import = new Import
            {
                StagePercentageComplete = StagePercentageComplete,
                Status = STAGE_2
            };

            Assert.Equal(100 * 0.1 + StagePercentageComplete * 0.1, import.PercentageComplete());
        }

        [Fact]
        public void PercentageComplete_Stage3()
        {
            var import = new Import
            {
                StagePercentageComplete = StagePercentageComplete,
                Status = STAGE_3
            };

            Assert.Equal(100 * 0.1 + 100 * 0.1 + StagePercentageComplete * 0.1, import.PercentageComplete());
        }

        [Fact]
        public void PercentageComplete_Stage4()
        {
            var import = new Import
            {
                StagePercentageComplete = StagePercentageComplete,
                Status = STAGE_4
            };

            Assert.Equal(100 * 0.1 + 100 * 0.1 + 100 * 0.1 + StagePercentageComplete * 0.7,
                import.PercentageComplete());
        }

        [Fact]
        public void PercentageComplete_Cancelled()
        {
            var import = new Import
            {
                StagePercentageComplete = StagePercentageComplete,
                Status = CANCELLED
            };

            Assert.Equal(100, import.PercentageComplete());
        }

        [Fact]
        public void PercentageComplete_Complete()
        {
            var import = new Import
            {
                StagePercentageComplete = StagePercentageComplete,
                Status = COMPLETE
            };

            Assert.Equal(100, import.PercentageComplete());
        }

        [Fact]
        public void PercentageComplete_StatusesWhichShouldHaveZeroProgress()
        {
            StatusesWithZeroProgress.ForEach(status =>
            {
                var import = new Import
                {
                    StagePercentageComplete = StagePercentageComplete,
                    Status = status
                };

                Assert.Equal(0, import.PercentageComplete());
            });
        }
    }
}