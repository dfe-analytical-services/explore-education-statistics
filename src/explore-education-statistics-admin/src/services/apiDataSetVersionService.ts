import client from '@admin/services/utils/service';
import { ApiDataSet } from '@admin/services/apiDataSetService';

const apiDataSetVersionService = {
  createVersion(data: {
    dataSetId: string;
    releaseFileId: string;
  }): Promise<ApiDataSet> {
    return client.post('/public-data/data-set-versions', data);
  },
  deleteVersion(versionId: string): Promise<void> {
    return client.delete(`/public-data/data-set-versions/${versionId}`);
  },
} as const;

export default apiDataSetVersionService;
