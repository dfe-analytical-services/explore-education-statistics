import { Dictionary } from '@common/types';
import { TableData } from '@common/services/tableBuilderService';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
} from '@frontend/modules/table-tool/components/types/filters';
import TimePeriod from '@frontend/modules/table-tool/components/types/TimePeriod';
import { TableHeadersFormValues } from '../TableHeadersForm';

export interface FullTableSubjectMeta {
  indicators: Indicator[];
  filters: Dictionary<CategoryFilter[]>;
  timePeriods: TimePeriod[];
  publicationName: string;
  subjectName: string;
  locations: LocationFilter[];
  footnotes?: TableData['footnotes'];
}

export default interface FullTable /* ™ */ {
  title: string;
  subjectMeta: FullTableSubjectMeta;
  results: TableData['result'];
  configurations: {
    tableHeadersConfig?: TableHeadersFormValues;
  };
}
