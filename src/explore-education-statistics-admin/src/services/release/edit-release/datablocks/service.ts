import client from '@admin/services/util/service';
import { DataBlock } from '@admin/services/release/edit-release/datablocks/types';

export interface DataBlockService {
  getDataBlocks: (releaseId: string) => Promise<DataBlock[]>;
  postDataBlock: (releaseId: string, dataBlock: DataBlock) => Promise<DataBlock>;
}

const service: DataBlockService = {
  async getDataBlocks(releaseId: string) {
    return client.get<DataBlock[]>(`/release/${releaseId}/datablocks`);
  },

  async postDataBlock(releaseId: string,  dataBlock: DataBlock) {
    return client.post<DataBlock>(`/release/${releaseId}/datablocks`, dataBlock);
  }
};

export default service;
