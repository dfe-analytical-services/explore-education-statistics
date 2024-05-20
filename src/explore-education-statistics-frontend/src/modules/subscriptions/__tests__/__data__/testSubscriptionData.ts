import { Subscription } from '@frontend/services/notificationService';

const testActiveSubscription: Subscription = {
  slug: 'test-publication-title',
  title: 'Test Publication Title',
  status: 'Subscribed',
};

export const testPendingSubscription: Subscription = {
  slug: 'test-publication-title',
  title: 'Test Publication Title',
  status: 'SubscriptionPending',
};

export default testActiveSubscription;
