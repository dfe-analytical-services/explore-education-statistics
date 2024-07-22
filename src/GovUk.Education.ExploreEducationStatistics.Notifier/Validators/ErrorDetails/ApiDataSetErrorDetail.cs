using System;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Validators.ErrorDetails;

public record ApiSubscriptionErrorDetail(Guid DataSetId, string Email);
