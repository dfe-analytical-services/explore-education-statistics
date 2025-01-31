import notificationApi from '@frontend/services/clients/notificationApi';

export interface Subscription {
  slug: string;
  title: string;
  status: 'NotSubscribed' | 'SubscriptionPending' | 'Subscribed';
}

type SubscribeQuery = {
  email: string;
  slug: string;
  title: string;
  id: string;
};

const notificationService = {
  requestPendingSubscription(query: SubscribeQuery): Promise<Subscription> {
    return notificationApi.post(
      '/publication/request-pending-subscription',
      query,
    );
  },
  async confirmPendingSubscription(
    id: string,
    token: string,
  ): Promise<Subscription | undefined> {
    return notificationApi
      .get<Subscription>(`publication/${id}/verify-subscription/${token}`)
      .catch(() => {
        return undefined;
      });
  },
  confirmUnsubscription(id: string, token: string): Promise<Subscription> {
    return notificationApi.get(`publication/${id}/unsubscribe/${token}`);
  },
};
export default notificationService;
