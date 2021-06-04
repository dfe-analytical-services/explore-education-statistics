import client from '@admin/services/utils/service';
import { DataBlock } from '@common/services/types/blocks';
import { OmitStrict } from '@common/types';

export type ReleaseDataBlock = OmitStrict<DataBlock, 'order' | 'type'>;

export interface ReleaseDataBlockSummary {
  id: string;
  name: string;
  created?: string;
  highlightName?: string;
  highlightDescription?: string;
  heading: string;
  source: string;
  chartsCount: number;
  contentSectionId?: string;
}

export type UpdateReleaseDataBlock = ReleaseDataBlock;
export type CreateReleaseDataBlock = OmitStrict<
  DataBlock,
  'id' | 'order' | 'type'
>;

export interface DependentDataBlock {
  name: string;
  contentSectionHeading?: string;
  infographicFilesInfo: InfographicFileInfo[];
}

export interface InfographicFileInfo {
  id: string;
  filename: string;
}

export interface DeleteDataBlockPlan {
  dependentDataBlocks: DependentDataBlock[];
}

const dataBlockService = {
  listDataBlocks(releaseId: string): Promise<ReleaseDataBlockSummary[]> {
    return client.get(`/releases/${releaseId}/data-blocks`);
  },

  getDataBlock(dataBlockId: string): Promise<ReleaseDataBlock> {
    return client.get<ReleaseDataBlock>(`/data-blocks/${dataBlockId}`);
  },

  createDataBlock(
    releaseId: string,
    dataBlock: CreateReleaseDataBlock,
  ): Promise<ReleaseDataBlock> {
    return client.post(`/releases/${releaseId}/data-blocks`, dataBlock);
  },

  updateDataBlock(
    dataBlockId: string,
    dataBlock: UpdateReleaseDataBlock,
  ): Promise<ReleaseDataBlock> {
    return client.put<ReleaseDataBlock>(
      `/data-blocks/${dataBlockId}`,
      dataBlock,
    );
  },

  deleteDataBlock(releaseId: string, id: string): Promise<void> {
    return client.delete(`/releases/${releaseId}/data-blocks/${id}`);
  },

  getDeleteBlockPlan(
    releaseId: string,
    id: string,
  ): Promise<DeleteDataBlockPlan> {
    return client.get(
      `/releases/${releaseId}/data-blocks/${id}/delete-plan`,
      {},
    );
  },
};

export default dataBlockService;
