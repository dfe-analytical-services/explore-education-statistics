import { utcToZonedTime } from 'date-fns-tz';
import client from '@admin/services/utils/service';
import previewTokenService from '../previewTokenService';

jest.mock('@admin/services/utils/service');
jest.mock('date-fns-tz');

const mockClient = client as jest.Mocked<typeof client>;
const mockUtcToZonedTime = utcToZonedTime as jest.MockedFunction<
  typeof utcToZonedTime
>;

describe('previewTokenService', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('createPreviewToken', () => {
    test('should convert UTC dates to Europe/London timezone', async () => {
      const utcActivatesDate = new Date('2025-10-15T09:00:00Z');
      const utcExpiresDate = new Date('2025-10-16T09:00:00Z');
      const londonActivatesDate = new Date('2025-10-15T10:00:00');
      const londonExpiresDate = new Date('2025-10-16T10:00:00');
      mockUtcToZonedTime
        .mockReturnValueOnce(londonActivatesDate)
        .mockReturnValueOnce(londonExpiresDate);
      const data = {
        dataSetVersionId: '456',
        label: 'Test Token',
        activates: utcActivatesDate,
        expires: utcExpiresDate,
      };

      await previewTokenService.createPreviewToken(data);

      expect(mockUtcToZonedTime).toHaveBeenNthCalledWith(
        1,
        utcActivatesDate,
        'Europe/London',
      );
      expect(mockUtcToZonedTime).toHaveBeenNthCalledWith(
        2,
        utcExpiresDate,
        'Europe/London',
      );
      expect(mockUtcToZonedTime).toHaveBeenCalledTimes(2);

      // Payload should use the converted (London/BST) dates we returned from the mock
      expect(mockClient.post).toHaveBeenCalledWith(
        '/public-data/preview-tokens',
        {
          ...data,
          activates: londonActivatesDate,
          expires: londonExpiresDate,
        },
      );
    });

    test('converts America/New_York (EDT) datetimes to Europe/London (BST) before POST', async () => {
      const edtActivates = new Date('2025-10-15T10:00:00-04:00');
      const edtExpires = new Date('2025-10-16T10:00:00-04:00');

      const bstActivates = new Date('2025-10-15T15:00:00+01:00');
      const bstExpires = new Date('2025-10-16T15:00:00+01:00');

      mockUtcToZonedTime
        .mockReturnValueOnce(bstActivates)
        .mockReturnValueOnce(bstExpires);

      const data = {
        dataSetVersionId: '789',
        label: 'American Token',
        activates: edtActivates,
        expires: edtExpires,
      };

      await previewTokenService.createPreviewToken(data);

      expect(mockUtcToZonedTime).toHaveBeenNthCalledWith(
        1,
        edtActivates,
        'Europe/London',
      );
      expect(mockUtcToZonedTime).toHaveBeenNthCalledWith(
        2,
        edtExpires,
        'Europe/London',
      );
      expect(mockUtcToZonedTime).toHaveBeenCalledTimes(2);

      // Payload should use the converted (London/BST) dates we returned from the mock
      expect(mockClient.post).toHaveBeenCalledTimes(1);
      expect(mockClient.post).toHaveBeenCalledWith(
        '/public-data/preview-tokens',
        {
          ...data,
          activates: bstActivates,
          expires: bstExpires,
        },
      );
    });

    test('should handle null dates without conversion', async () => {
      const data = {
        dataSetVersionId: '456',
        label: 'Test Token',
        activates: null,
        expires: null,
      };

      await previewTokenService.createPreviewToken(data);

      expect(mockUtcToZonedTime).not.toHaveBeenCalled();
      expect(mockClient.post).toHaveBeenCalledWith(
        '/public-data/preview-tokens',
        {
          dataSetVersionId: '456',
          label: 'Test Token',
          activates: null,
          expires: null,
        },
      );
    });
  });
});
