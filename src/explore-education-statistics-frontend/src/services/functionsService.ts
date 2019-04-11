import { functionApi } from './api';

export interface SubscriptionData {
  email: string;
  slug: string;
  title: string;
}

export default {
  subscribeToPublication(query: {
    email: string;
    slug: string;
    title: string;
  }): Promise<SubscriptionData> {
    return functionApi.post('/publication/subscribe', query);
  },
};
