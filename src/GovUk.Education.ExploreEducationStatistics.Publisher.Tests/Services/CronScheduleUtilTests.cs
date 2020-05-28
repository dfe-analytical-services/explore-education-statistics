using System;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Services.CronScheduleUtil;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class CronScheduleUtilTests
    {
        [Fact]
        public void GetReleaseViewModel()
        {
            Environment.SetEnvironmentVariable("PublishReleaseContentCronSchedule", "0 30 9 * * *");

            var nineThirty = new TimeSpan(9, 30, 0);
            var expected = DateTime.UtcNow.TimeOfDay > nineThirty
                ? DateTime.Today.AddDays(1).Add(nineThirty)
                : DateTime.Today.Add(nineThirty);

            Assert.Equal(expected, GetNextScheduledPublishingTime());
        }
    }
}