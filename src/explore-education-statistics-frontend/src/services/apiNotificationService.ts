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
    id: string,
    token: string,
  ): Promise<ApiSubscription | undefined> {
    return notificationApi.post<ApiSubscription>(
      `public-api/${id}/verify-subscription/${token}`,
    );
  },
  confirmUnsubscription(id: string, token: string): Promise<ApiSubscription> {
    return notificationApi.delete(`public-api/${id}/unsubscribe/${token}`);
  },
};
export default apiNotificationService;
