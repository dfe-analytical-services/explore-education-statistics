import { Table, Chart, Summary } from '@common/services/publicationService';
import { DataBlockRequest } from '@common/services/dataBlockService';

// eslint-disable-next-line import/prefer-default-export
export interface DataBlock {
  id?: string;

  heading?: string;
  customFootnotes?:string;
  name?:string;
  source?:string;

  dataBlockRequest?: DataBlockRequest;
  charts?: Chart[];
  tables?: Table[];
  summary?: Summary;
}
