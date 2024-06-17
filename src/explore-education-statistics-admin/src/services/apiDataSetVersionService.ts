import client from '@admin/services/utils/service';

const apiDataSetVersionService = {
  deleteVersion(versionId: string): Promise<void> {
    return client.delete(`/public-data/data-set-versions/${versionId}`);
  },
} as const;

export default apiDataSetVersionService;
