using System;
using NCrontab;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public static class CronScheduleUtil
    {
        public static DateTime GetNextScheduledPublishingTime()
        {
            var publishReleasesCronSchedule = Environment.GetEnvironmentVariable("PublishReleaseContentCronSchedule");
            return TryParseCronSchedule(publishReleasesCronSchedule, out var cronSchedule)
                ? cronSchedule.GetNextOccurrence(DateTime.UtcNow)
                : DateTime.UtcNow;
        }

        private static bool TryParseCronSchedule(string cronExpression, out CrontabSchedule cronSchedule)
        {
            // ReSharper disable once IdentifierTypo
            cronSchedule = CrontabSchedule.TryParse(cronExpression, new CrontabSchedule.ParseOptions
            {
                IncludingSeconds = CronExpressionHasSeconds(cronExpression)
            });
            return cronSchedule != null;
        }

        private static bool CronExpressionHasSeconds(string cronExpression)
        {
            return cronExpression.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Length != 5;
        }
    }
}