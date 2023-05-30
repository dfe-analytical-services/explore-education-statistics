import { TableJson } from '@common/modules/table-tool/utils/mapTableToJson';
import { dataApi } from '@common/services/api';
import { TableDataQuery } from '@common/services/tableBuilderService';
import { Footnote } from '@common/services/types/footnotes';
import { UnmappedTableHeadersConfig } from '@common/services/permalinkService';

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
  query: TableDataQuery;
  configuration: {
    tableHeaders: UnmappedTableHeadersConfig;
  };
}

const permalinkSnapshotService = {
  createPermalink(query: CreatePermalink): Promise<PermalinkSnapshot> {
    return dataApi.post(`/permalink-snapshot`, query);
  },
  async getPermalink(id: string): Promise<PermalinkSnapshot> {
    return dataApi.get(`/permalink-snapshot/${id}`, {
      headers: {
        Accept: 'application/json',
      },
    });
  },
  async getPermalinkCsv(id: string): Promise<Blob> {
    return dataApi.get(`/permalink-snapshot/${id}`, {
      headers: {
        Accept: 'text/csv',
      },
      responseType: 'blob',
    });
  },
};

export default permalinkSnapshotService;
