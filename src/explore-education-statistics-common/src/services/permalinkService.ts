import { TableJson } from '@common/modules/table-tool/utils/mapTableToJson';
import { dataApi } from '@common/services/api';
import { TableDataQuery } from '@common/services/tableBuilderService';
import { Footnote } from '@common/services/types/footnotes';

export type TableHeader =
  | {
      type: 'TimePeriod' | 'Indicator' | 'Filter';
      value: string;
    }
  | {
      type: 'Location';
      value: string;
      level: string;
    };

export interface UnmappedTableHeadersConfig {
  columnGroups: TableHeader[][];
  columns: TableHeader[];
  rowGroups: TableHeader[][];
  rows: TableHeader[];
}

export interface PermalinkSnapshot {
  created: string;
  dataSetTitle: string;
  id: string;
  publicationTitle: string;
  status:
    | 'Current'
    | 'SubjectRemoved'
    | 'SubjectReplacedOrRemoved'
    | 'NotForLatestRelease'
    | 'PublicationSuperseded';
  table: {
    caption: string;
    footnotes: Footnote[];
    json: TableJson;
  };
}

interface CreatePermalink {
  releaseId?: string;
  query: TableDataQuery;
  configuration: {
    tableHeaders: UnmappedTableHeadersConfig;
  };
}

const permalinkService = {
  createPermalink(permalink: CreatePermalink): Promise<PermalinkSnapshot> {
    return dataApi.post('/permalink', permalink);
  },
  async getPermalink(id: string): Promise<PermalinkSnapshot> {
    return dataApi.get(`/permalink/${id}`, {
      headers: {
        Accept: 'application/json',
      },
    });
  },
  async getPermalinkCsv(id: string): Promise<Blob> {
    return dataApi.get(`/permalink/${id}`, {
      headers: {
        Accept: 'text/csv',
      },
      responseType: 'blob',
    });
  },
};

export default permalinkService;
