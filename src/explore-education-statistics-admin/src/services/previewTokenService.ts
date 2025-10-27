import client from '@admin/services/utils/service';
import { utcToZonedTime } from 'date-fns-tz';

export interface PreviewToken {
  id: string;
  label: string;
  status: 'Expired' | 'Active' | 'Pending';
  createdByEmail: string;
  activates: string;
  expires: string;
  updated: string;
}

const previewTokenService = {
  createPreviewToken(data: {
    dataSetVersionId: string;
    label: string;
    activates?: Date | null;
    expires?: Date | null;
  }): Promise<PreviewToken> {
    return client.post('/public-data/preview-tokens', {
      ...data,
      activates:
        data.activates && utcToZonedTime(data.activates, 'Europe/London'),
      expires: data.expires && utcToZonedTime(data.expires, 'Europe/London'),
    });
  },
  getPreviewToken(previewTokenId: string): Promise<PreviewToken> {
    return client.get(`/public-data/preview-tokens/${previewTokenId}`);
  },
  listPreviewTokens(dataSetVersionId: string): Promise<PreviewToken[]> {
    return client.get(`/public-data/preview-tokens`, {
      params: { dataSetVersionId },
    });
  },
  revokePreviewToken(previewTokenId: string): Promise<PreviewToken> {
    return client.post(`/public-data/preview-tokens/${previewTokenId}/revoke`);
  },
} as const;

export default previewTokenService;
