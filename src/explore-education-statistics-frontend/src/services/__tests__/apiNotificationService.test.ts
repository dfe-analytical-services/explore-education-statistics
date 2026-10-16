import apiNotificationService from '@frontend/services/apiNotificationService';
import notificationApi from '@frontend/services/clients/notificationApi';
import {
  CapturedRequest,
  captureRequests,
} from '@frontend/services/__tests__/utils/captureRequests';

jest.mock('@frontend/services/clients/notificationApi', () => {
  const Client = jest.requireActual('@common/services/api/Client').default;

  return {
    __esModule: true,
    default: new Client({ baseURL: 'http://notifier/api' }),
  };
});

describe('apiNotificationService', () => {
  const token = 'test-token';
  let requests: CapturedRequest[];
  let release: () => void;

  beforeEach(() => {
    ({ requests, release } = captureRequests(notificationApi));
  });

  afterEach(() => {
    release();
  });

  describe('confirmPendingSubscription', () => {
    test('sends the token as a query string parameter', async () => {
      await apiNotificationService.confirmPendingSubscription(
        'data-set-id',
        token,
      );

      expect(requests).toHaveLength(1);

      const [request] = requests;

      expect(request.method).toBe('post');
      expect(request.url.pathname).toBe(
        '/api/public-api/data-set-id/verify-subscription',
      );
      expect(request.url.searchParams.get('token')).toBe(token);
      expect(request.data).toBeUndefined();
    });
  });

  describe('confirmUnsubscription', () => {
    test('sends the token as a query string parameter', async () => {
      await apiNotificationService.confirmUnsubscription('data-set-id', token);

      expect(requests).toHaveLength(1);

      const [request] = requests;

      expect(request.method).toBe('delete');
      expect(request.url.pathname).toBe(
        '/api/public-api/data-set-id/unsubscribe',
      );
      expect(request.url.searchParams.get('token')).toBe(token);
      expect(request.data).toBeUndefined();
    });
  });
});
