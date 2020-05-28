import client from '@admin/services/utils/service';
import { Overwrite } from '@common/types';

export type ImportStatusCode =
  | 'COMPLETE'
  | 'QUEUED'
  | 'UPLOADING'
  | 'RUNNING_PHASE_1'
  | 'RUNNING_PHASE_2'
  | 'RUNNING_PHASE_3'
  | 'NOT_FOUND'
  | 'FAILED';

interface Errors {
  Message: string;
}

export interface ImportStatus {
  status: ImportStatusCode;
  percentageComplete?: string;
  errors?: Errors[];
  numberOfRows: number;
}

const importService = {
  getImportStatus(releaseId: string, dataFileName: string) {
    return client
      .get<
        Overwrite<
          ImportStatus,
          {
            errors?: string;
          }
        >
      >(`/release/${releaseId}/data/${dataFileName}/import/status`)
      .then(importStatus => {
        return {
          ...importStatus,
          errors: JSON.parse(importStatus.errors || '[]').map(
            ({ Message }: Errors) => Message,
          ),
        };
      });
  },
};

export default importService;
