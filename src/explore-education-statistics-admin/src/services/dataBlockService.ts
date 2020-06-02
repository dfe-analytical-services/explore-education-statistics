import client from '@admin/services/utils/service';
import { DataBlock } from '@common/services/types/blocks';
import { OmitStrict } from '@common/types';

export type ReleaseDataBlock = OmitStrict<DataBlock, 'order' | 'type'>;

export type UpdateReleaseDataBlock = ReleaseDataBlock;
export type CreateReleaseDataBlock = OmitStrict<
  DataBlock,
  'id' | 'order' | 'type'
>;

export interface DependentDataBlock {
  name: string;
  contentSectionHeading?: string;
  infographicFilenames: string[];
}

export interface DeleteDataBlockPlan {
  dependentDataBlocks: DependentDataBlock[];
}

const dataBlockService = {
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

  async deleteDataBlock(releaseId: string, id: string) {
    return client.delete(`/release/${releaseId}/datablocks/${id}`);
  },

  async getDeleteBlockPlan(releaseId: string, id: string) {
    return client.get<DeleteDataBlockPlan>(
      `/release/${releaseId}/datablocks/${id}/delete-plan`,
      {},
    );
  },
};

export default dataBlockService;
