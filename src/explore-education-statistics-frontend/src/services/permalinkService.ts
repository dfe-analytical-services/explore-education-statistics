import { dataApi } from '@common/services/api';
import { Dictionary } from '@common/types';
import { TableData } from '@common/services/tableBuilderService';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
} from '@frontend/modules/table-tool/components/types/filters';
import TimePeriod from '@frontend/modules/table-tool/components/types/TimePeriod';
import { TableHeadersFormValues } from '@frontend/modules/table-tool/components/TableHeadersForm';

export interface SubjectMeta {
  indicators: Indicator[];
  filters: Dictionary<CategoryFilter[]>;
  timePeriods: TimePeriod[];
  publicationName: string;
  subjectName: string;
  locations: LocationFilter[];
  footnotes?: TableData['footnotes'];
}

export interface FullTable /* ™ */ {
  title: string;
  subjectMeta: SubjectMeta;
  results: TableData['result'];
  configurations: {
    tableHeadersConfig?: TableHeadersFormValues;
  };
}

export interface Permalink {
  id: string;
  title: string;
  created: string;
  fullTable: FullTable;
}

export default {
  getPermalink(publicationSlug: string): Promise<Permalink> {
    return dataApi.get(`Permalink/${publicationSlug}`);
  },
};
