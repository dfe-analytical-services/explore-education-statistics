import client from '@admin/services/util/service';
import { DataBlock } from '@common/services/dataBlockService';

import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/full-table/types/filters';

const service = {
  async getDataBlocks(releaseId: string) {
    return client.get<DataBlock[]>(`/release/${releaseId}/datablocks`, {});
  },

  async postDataBlock(releaseId: string, dataBlock: DataBlock) {
    return client.post<DataBlock>(
      `/release/${releaseId}/datablocks`,
      dataBlock,
    );
  },

  async putDataBlock(dataBlockId: string, dataBlock: DataBlock) {
    return client.put<DataBlock>(`/datablocks/${dataBlockId}`, dataBlock);
  },

  async deleteDataBlock(id: string) {
    return client.delete(`/datablocks/${id}`);
  },
};

export default service;
