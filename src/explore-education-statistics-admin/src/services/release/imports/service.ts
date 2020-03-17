import client from '@admin/services/util/service';
import { ErrorObj, ImportStatus } from './types';

interface StringifiedImportStatus extends Omit<ImportStatus, 'errors'> {
  errors?: string;
}

const service = {
  getImportStatus(releaseId: string, dataFileName: string) {
    return client
      .get<StringifiedImportStatus>(
        `/release/${releaseId}/data/${dataFileName}/import/status`,
      )
      .then(importStatus => {
        return {
          ...importStatus,
          errors: JSON.parse(importStatus.errors || '[]').map(
            ({ Message }: ErrorObj) => Message,
          ),
        };
      });
  },
};

export default service;
