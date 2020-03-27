import client from '@admin/services/util/service';
import { DataBlock } from '@common/services/types/blocks';
import { OmitStrict } from '@common/types';

export type ReleaseDataBlock = OmitStrict<DataBlock, 'order' | 'type'>;
export type UpdateReleaseDataBlock = ReleaseDataBlock;
export type CreateReleaseDataBlock = OmitStrict<
  DataBlock,
  'id' | 'order' | 'type'
>;

const service = {
  async getDataBlocks(releaseId: string) {
    return client.get<ReleaseDataBlock[]>(
      `/release/${releaseId}/datablocks`,
      {},
    );
  },

  async postDataBlock(releaseId: string, dataBlock: CreateReleaseDataBlock) {
    return client.post<ReleaseDataBlock>(
      `/release/${releaseId}/datablocks`,
      dataBlock,
    );
  },

  async putDataBlock(dataBlockId: string, dataBlock: UpdateReleaseDataBlock) {
    return client.put<ReleaseDataBlock>(
      `/datablocks/${dataBlockId}`,
      dataBlock,
    );
  },

  async deleteDataBlock(id: string) {
    return client.delete(`/datablocks/${id}`);
  },
};

export default service;
