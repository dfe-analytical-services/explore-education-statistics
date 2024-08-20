import notificationApi from '@frontend/services/clients/notificationApi';

interface ApiSubscription {
  dataSetId: string;
  dataSetTitle: string;
  status: 'NotSubscribed' | 'SubscriptionPending' | 'Subscribed';
}

interface SubscribeQuery {
  dataSetId: string;
  dataSetTitle: string;
  email: string;
}

const apiNotificationService = {
  requestPendingSubscription(query: SubscribeQuery): Promise<ApiSubscription> {
    return notificationApi.post(
      '/public-api/request-pending-subscription',
      query,
    );
  },
  async confirmPendingSubscription(
    dataSetId: string,
    token: string,
  ): Promise<ApiSubscription> {
    return notificationApi.post<ApiSubscription>(
      `public-api/${dataSetId}/verify-subscription/${token}`,
    );
  },
  confirmUnsubscription(dataSetId: string, token: string): Promise<void> {
    return notificationApi.delete(
      `public-api/${dataSetId}/unsubscribe/${token}`,
    );
  },
};
export default apiNotificationService;
