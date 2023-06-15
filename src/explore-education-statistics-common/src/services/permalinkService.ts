import { dataApi } from '@common/services/api';
import { TableDataQuery } from '@common/services/tableBuilderService';
import { ConfiguredTable } from '@common/services/types/table';
import deduplicatePermalinkLocations from '@common/services/util/permalinkServiceUtils';

export type Permalink = ConfiguredTable & {
  created: string;
  status?:
    | 'Current'
    | 'SubjectRemoved'
    | 'SubjectReplacedOrRemoved'
    | 'NotForLatestRelease'
    | 'PublicationSuperseded';
};

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

interface CreatePermalink {
  query: TableDataQuery;
  configuration: {
    tableHeaders: UnmappedTableHeadersConfig;
  };
}

const permalinkService = {
  createPermalink(
    query: CreatePermalink,
    releaseId?: string,
  ): Promise<Permalink> {
    if (releaseId) {
      return dataApi.post(`/permalink/release/${releaseId}`, query);
    }
    return dataApi.post(`/permalink`, query);
  },
  async getPermalink(id: string): Promise<Permalink> {
    return deduplicatePermalinkLocations(await dataApi.get(`/permalink/${id}`));
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
