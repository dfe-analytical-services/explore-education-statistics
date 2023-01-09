namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    /// <summary>
    /// Stages that are allowed to be retried.
    /// </summary>
    /// <remarks>
    /// <para>The following are not allowed for these reasons:</para>
    /// <list type="bullet">
    /// <item>
    /// <term>Content</term>
    /// <description>Content alone cannot be generated without publishing it to avoid corrupting the staging directory See <see cref="ContentAndPublishing"/>.</description>
    /// </item>
    /// <item>
    /// <term>Files</term>
    /// <description>Files changing implies there are statistics data and content (download) changes so it's recommended to publish the Release with the full workflow.</description>
    /// </item>
    /// <item>
    /// <term>Publishing</term>
    /// <description>Publishing alone could have unintended consequences if there is more than one Release staged.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public enum RetryStage
    {
        ContentAndPublishing,
        StatisticsData
    }
}