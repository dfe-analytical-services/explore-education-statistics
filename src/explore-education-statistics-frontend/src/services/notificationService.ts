import notificationApi from '@frontend/services/clients/notificationApi';

export interface SubscriptionData {
  email: string;
  slug: string;
  title: string;
}

export default {
  subscribeToPublication(query: {
    email: string;
    id: string;
    slug: string;
    title: string;
  }): Promise<SubscriptionData> {
    return notificationApi.post('/publication/subscribe', query);
  },
};
