export type ImportStatusCode =
  | 'COMPLETE'
  | 'RUNNING_PHASE_1'
  | 'RUNNING_PHASE_2'
  | 'QUEUED'
  | 'FAILED';

export interface ImportStatus {
  status: ImportStatusCode;
  percentageComplete?: string;
  errors?: string[];
}

export default {};
