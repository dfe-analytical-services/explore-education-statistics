import { functionApi } from './api';

export interface SubscriptionData {
  publicationId: string;
  email: string;
  result: {

  }[];
}

export default {
  subscribe(query: {
    email: string;
    publicationId: string;
  }): Promise<SubscriptionData> {
    return functionApi.post('/publication/subscribe', query);
  },
};
