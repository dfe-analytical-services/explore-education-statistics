import { dataApi } from '@common/services/api';
import { Dictionary } from '@common/types';
import {
  TableData,
  GroupedFilterOptions,
  TableDataQuery,
} from '@frontend/services/tableBuilderService';
import TimePeriod from '@common/services/types/TimePeriod';
import {
  Indicator,
  LocationFilter,
} from '@frontend/modules/table-tool/components/types/filters';
import { TableHeadersFormValues } from '@frontend/modules/table-tool/components/TableHeadersForm';

export interface FullTableMeta {
  publicationName: string;
  subjectId: string;
  subjectName: string;
  locations: LocationFilter[];
  timePeriodRange: TimePeriod[];
  filters: Dictionary<{
    legend: string;
    hint?: string;
    options: GroupedFilterOptions;
    totalValue?: string;
  }>;
  indicators: Indicator[];
  footnotes: {
    id: number;
    label: string;
  }[];
}

interface PermalinkCreate extends TableDataQuery {
  configurations: {
    tableHeadersConfig: TableHeadersFormValues;
  };
}

export interface FullTable extends TableData {
  title?: string;
}

export interface Permalink {
  id: string;
  title: string;
  created: string;
  fullTable: FullTable;
  configuration: {
    tableHeadersConfig?: TableHeadersFormValues;
  };
}

export default {
  getPermalink(publicationSlug: string): Promise<Permalink> {
    return dataApi.get(`Permalink/${publicationSlug}`);
  },
  createTablePermalink(
    query: PermalinkCreate,
  ): Promise<{
    id: string;
    title: string;
    created: string;
    url: string;
  }> {
    return dataApi.post('/permalink', query);
  },
};
