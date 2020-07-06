import { dataApi } from '@common/services/api';
import { ConfiguredTable } from '@common/services/types/table';

export type FastTrackTable = ConfiguredTable;

const fastTrackService = {
  getFastTrackTable(id: string): Promise<FastTrackTable> {
    return dataApi.get(`/fasttrack/${id}`);
  },
};

export default fastTrackService;
