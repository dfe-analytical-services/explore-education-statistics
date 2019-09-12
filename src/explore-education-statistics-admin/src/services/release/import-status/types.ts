export type ImportStatusCode =
  | 'COMPLETE'
  | 'RUNNING_PHASE_1'
  | 'RUNNING_PHASE_2'
  | 'QUEUED'
  | 'FAILED';

export interface ImportStatus {
  Status: ImportStatusCode;
  PercentageComplete?: string;
  Errors?: string[];
}

export default {};
