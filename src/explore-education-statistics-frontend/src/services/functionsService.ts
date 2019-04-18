import { functionApi } from '@common/services/api';

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
    return functionApi.post('/publication/subscribe', query);
  },
};
