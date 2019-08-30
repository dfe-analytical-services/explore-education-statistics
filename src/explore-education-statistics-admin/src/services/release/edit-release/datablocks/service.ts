import client from '@admin/services/util/service';
import { DataBlock } from '@admin/services/release/edit-release/datablocks/types';

export interface DataBlockService {
  getDataBlocks: (releaseId: string) => Promise<DataBlock[]>;
}

const service: DataBlockService = {
  async getDataBlocks(releaseId: string) {
    return client.get<DataBlock[]>(`/release/${releaseId}/datablocks`);
  },
};

export default service;
