import client from '@admin/services/utils/service';

export interface ImportStatus {
  subjectTitle: string;
  subjectId: string;
  publicationId: string;
  releaseId: string;
  dataFileName: string;
  metaFileName: string;
  numberOfRows: number;
  status: number;
  stagePercentageComplete: number;
}

const importStatusService = {
  getAllIncompleteImports(): Promise<ImportStatus[]> {
    return client.get(`/imports/incomplete`);
  },
};

export default importStatusService;
