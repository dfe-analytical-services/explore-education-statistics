using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Validators;

public static class ValidationMessages
{
    public static readonly LocalizableMessage ApiPendingSubscriptionAlreadyExists = new(
        Code: nameof(ApiPendingSubscriptionAlreadyExists),
        Message: "The user already has a pending subscription for the API data set. They must verify their subscription."
    );

    public static readonly LocalizableMessage ApiVerifiedSubscriptionAlreadyExists = new(
        Code: nameof(ApiVerifiedSubscriptionAlreadyExists),
        Message: "The user is already subscribed to the API data set."
    );

    public static readonly LocalizableMessage ApiPendingSubscriptionAlreadyExpired = new(
        Code: nameof(ApiPendingSubscriptionAlreadyExpired),
        Message: "The unverified subscription has expired. The user must request a new subscription."
    );

    public static readonly LocalizableMessage ApiSubscriptionHasNotBeenVerified = new(
        Code: nameof(ApiSubscriptionHasNotBeenVerified),
        Message: "The subscription has not been verified."
    );

    public static readonly LocalizableMessage AuthorizationTokenInvalid = new(
        Code: nameof(AuthorizationTokenInvalid),
        Message: "The authorization token is invalid."
    );

    public static readonly LocalizableMessage InvalidDataSetVersion = new(
        Code: "InvalidDataSetVersion",
        Message: "The data set version version number supplied is invalid."
    );
}
