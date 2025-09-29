import client from '@admin/services/utils/service';

export interface PreviewToken {
  id: string;
  label: string;
  status: 'Expired' | 'Active';
  createdByEmail: string;
  created: string;
  expires: string;
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
