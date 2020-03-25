export type ImportStatusCode =
  | 'COMPLETE'
  | 'QUEUED'
  | 'RUNNING_PHASE_1'
  | 'RUNNING_PHASE_2'
  | 'RUNNING_PHASE_3'
  | 'NOT_FOUND'
  | 'FAILED';

export interface ErrorObj {
  Message: string;
}

export interface ImportStatus {
  status: ImportStatusCode;
  percentageComplete?: string;
  errors?: ErrorObj[];
  numberOfRows: number;
}

export default {};
