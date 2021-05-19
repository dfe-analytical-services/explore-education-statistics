import { dataApi } from '@common/services/api';
import { TableDataQuery } from '@common/services/tableBuilderService';
import { ConfiguredTable } from '@common/services/types/table';

export type FastTrackTable = ConfiguredTable & {
  query: TableDataQuery & {
    publicationId: string;
  };
};

export type FastTrackTableAndReleaseMeta = FastTrackTable & {
  releaseId: string;
  releaseSlug: string;
  latestData: boolean;
  latestReleaseTitle: string;
};

const fastTrackService = {
  getFastTrackTableAndReleaseMeta(
    id: string,
  ): Promise<FastTrackTableAndReleaseMeta> {
    return dataApi.get(`/fasttrack/${id}`);
  },
};

export default fastTrackService;
