import notificationService from '@frontend/services/notificationService';
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

describe('notificationService', () => {
  const token = 'header.payload.signature';
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
      await notificationService.confirmPendingSubscription(
        'publication-id',
        token,
      );

      expect(requests).toHaveLength(1);

      const [request] = requests;

      expect(request.method).toBe('get');
      expect(request.url.pathname).toBe(
        '/api/publication/publication-id/verify-subscription',
      );
      expect(request.url.searchParams.get('token')).toBe(token);
    });
  });

  describe('confirmUnsubscription', () => {
    test('sends the token as a query string parameter', async () => {
      await notificationService.confirmUnsubscription('publication-id', token);

      expect(requests).toHaveLength(1);

      const [request] = requests;

      expect(request.method).toBe('get');
      expect(request.url.pathname).toBe(
        '/api/publication/publication-id/unsubscribe',
      );
      expect(request.url.searchParams.get('token')).toBe(token);
    });
  });
});
