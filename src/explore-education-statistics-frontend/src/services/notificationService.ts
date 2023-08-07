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
  subscribeToPublication(query: SubscribeQuery): Promise<SubscriptionData> {
    return notificationApi.post('/publication/subscribe', query);
  },
};
export default notificationService;
