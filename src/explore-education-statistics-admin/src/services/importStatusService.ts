import client from '@admin/services/utils/service';

export interface ImportStatus {
  subjectTitle: string;
  subjectId: string;
  publicationId: string;
  publicationTitle: string;
  releaseId: string;
  releaseTitle: string;
  dataFileName: string;
  totalRows?: number;
  batches: number;
  status: number;
  stagePercentageComplete: number;
  percentageComplete: number;
}

const importStatusService = {
  getAllIncompleteImports(): Promise<ImportStatus[]> {
    return client.get(`/imports/incomplete`);
  },
};

export default importStatusService;
