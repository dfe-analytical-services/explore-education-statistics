import client from '@admin/services/util/service';
import { ImportStatus } from './types';

export interface ImportStatusService {
  getImportStatus: (
    releaseId: string,
    dataFileId: string,
  ) => Promise<ImportStatus>;
}

interface StringifiedImportStatus extends Omit<ImportStatus, 'errors'> {
  errors?: string;
}

const service: ImportStatusService = {
  getImportStatus(releaseId: string, dataFileName: string) {
    return client
      .get<StringifiedImportStatus>(
        `/release/${releaseId}/data/${dataFileName}/import/status`,
      )
      .then(importStatus => {
        return {
          ...importStatus,
          errors: JSON.parse(importStatus.errors || '[]'),
        };
      });
  },
};

export default service;
