import { dataApi } from '@common/services/api';
import { TableDataQuery } from '@common/services/tableBuilderService';
import { ConfiguredTable } from '@common/services/types/table';

export type FastTrackTable = ConfiguredTable & {
  query: TableDataQuery & {
    publicationId: string;
  };
  releaseId: string;
  releaseSlug: string;
};

const fastTrackService = {
  getFastTrackTable(id: string): Promise<FastTrackTable> {
    return dataApi.get(`/fasttrack/${id}`);
  },
};

export default fastTrackService;
