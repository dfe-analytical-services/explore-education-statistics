import client from '@admin/services/utils/service';
import { PaginatedList } from '@common/services/types/pagination';

export interface PreviewToken {
  id: string;
  label: string;
  status: 'Expired' | 'Active';
  createdByEmail: string;
  created: string;
  expiry: string;
  updated: string;
}

const previewTokenService = {
  createPreviewToken(data: {
    dataSetVersionId: string;
    label: string;
  }): Promise<PreviewToken> {
    return client.post('/public-data/preview-tokens', data);
  },
  getPreviewToken(previewTokenId: string): Promise<PreviewToken> {
    return client.get(`/public-data/preview-tokens/${previewTokenId}`);
  },
  async listPreviewTokens(dataSetVersionId: string): Promise<PreviewToken[]> {
    const { results } = await client.get<PaginatedList<PreviewToken>>(
      '/public-data/preview-tokens',
      {
        params: {
          dataSetVersionId,
          pageSize: 100,
        },
      },
    );

    return results;
  },
  revokePreviewToken(previewTokenId: string): Promise<PreviewToken> {
    return client.post(`/public-data/preview-tokens/${previewTokenId}/revoke`);
  },
} as const;

export default previewTokenService;
