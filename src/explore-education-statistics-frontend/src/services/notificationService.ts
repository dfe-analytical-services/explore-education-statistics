import notificationApi from '@frontend/services/clients/notificationApi';

export interface SubscriptionData {
  email: string;
  slug: string;
  title: string;
}

type SubscribeQuery = SubscriptionData & {
  id: string;
};

const notificationService = {
  // Actually "begin a pending subscription and request a subscription verification email"
  subscribeToPublication(query: SubscribeQuery): Promise<SubscriptionData> {
    return notificationApi.post('/publication/subscribe', query);
  },
  // Actually confirms the pending subscription
  confirmPendingSubscription(
    id: string,
    token: string,
  ): Promise<SubscriptionData> {
    return notificationApi.get(
      `publication/${id}/verify-subscription-actual/${token}`,
    );
  },
  confirmUnsubscription(id: string, token: string): Promise<SubscriptionData> {
    return notificationApi.get(`publication/${id}/unsubscribe-actual/${token}`);
  },
};
export default notificationService;
