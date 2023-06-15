import notificationApi from '@frontend/services/clients/notificationApi';

export interface SubscriptionData {
  email: string;
  slug: string;
  title: string;
}

type SubscriptionQuery = SubscriptionData & {
  id: string;
};

const notificationService = {
  subscribeToPublication(query: SubscriptionQuery): Promise<SubscriptionData> {
    return notificationApi.post('/publication/subscribe', query);
  },
};

export default notificationService;
