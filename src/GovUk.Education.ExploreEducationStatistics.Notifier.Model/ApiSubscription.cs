using System.Runtime.Serialization;
using Azure;
using Azure.Data.Tables;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model;

public class ApiSubscription : ITableEntity
{
    /// <summary>
    /// This should be set to the API <b>Data Set ID</b> that the user is subscribing to.
    /// <br/><br/>
    /// <inheritdoc/>
    /// </summary>
    public required string PartitionKey { get; set; }
    /// <summary>
    /// This should be set to the <b>email address</b> of the user subscribing.
    /// <br/><br/>
    /// <inheritdoc/>
    /// </summary>
    public required string RowKey { get; set; }
    public required string DataSetTitle { get; set; }
    public required ApiSubscriptionStatus Status { get; set; }
    /// <summary>
    /// This is only relevant to subscriptions which are <b>pending</b> and yet to be verified.
    /// It is to be set to <b>null</b> otherwise.
    /// </summary>
    public DateTimeOffset? Expiry { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    [IgnoreDataMember]
    public bool HasExpired => Expiry <= DateTimeOffset.UtcNow;
}
