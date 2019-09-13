import client from '@admin/services/util/service';

import { ImportStatus } from './types';

export interface ImportStatusService {
  getImportStatus: (
    releaseId: string,
    dataFileId: string,
  ) => Promise<ImportStatus>;
}

const service: ImportStatusService = {
  getImportStatus(releaseId: string, dataFileName: string) {
    return client.get<ImportStatus>(
      `/release/${releaseId}/data/${dataFileName}/import/status`,
    );
  },
};

export default service;
