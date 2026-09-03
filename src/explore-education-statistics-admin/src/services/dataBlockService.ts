import client from '@admin/services/utils/service';
import { DataBlock } from '@common/services/types/blocks';
import { OmitStrict } from '@common/types';
import { FeaturedTableBasic } from '@admin/services/featuredTableService';

export type ReleaseDataBlock = OmitStrict<DataBlock, 'order' | 'type'>;

export interface ReleaseDataBlockSummary {
  id: string;
  name: string;
  created?: string;
  highlightName?: string;
  highlightDescription?: string;
  heading: string;
  source?: string;
  chartsCount: number;
  inContent: boolean;
}

export type UpdateReleaseDataBlock = ReleaseDataBlock;
export type CreateReleaseDataBlock = OmitStrict<
  DataBlock,
  'id' | 'order' | 'type' | 'dataBlockParentId'
>;

export interface DependentDataBlock {
  name: string;
  contentSectionHeading?: string;
  infographicFilesInfo: InfographicFileInfo[];
  isKeyStatistic: boolean;
  featuredTable?: FeaturedTableBasic;
}

export interface InfographicFileInfo {
  id: string;
  filename: string;
}

export interface DeleteDataBlockPlan {
  dependentDataBlocks: DependentDataBlock[];
}

const dataBlockService = {
  listDataBlocks(releaseVersionId: string): Promise<ReleaseDataBlockSummary[]> {
    return client.get(`/releases/${releaseVersionId}/data-blocks`);
  },

  getDataBlock(dataBlockId: string): Promise<ReleaseDataBlock> {
    return client.get<ReleaseDataBlock>(`/data-blocks/${dataBlockId}`);
  },

  createDataBlock(
    releaseVersionId: string,
    dataBlock: CreateReleaseDataBlock,
  ): Promise<ReleaseDataBlock> {
    return client.post(`/releases/${releaseVersionId}/data-blocks`, dataBlock);
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

  deleteDataBlock(releaseVersionId: string, id: string): Promise<void> {
    return client.delete(`/releases/${releaseVersionId}/data-blocks/${id}`);
  },

  getDeleteBlockPlan(
    releaseVersionId: string,
    id: string,
  ): Promise<DeleteDataBlockPlan> {
    return client.get(
      `/releases/${releaseVersionId}/data-blocks/${id}/delete-plan`,
      {},
    );
  },

  getUnattachedDataBlocks(releaseVersionId: string): Promise<DataBlock[]> {
    return client.get(`/release/${releaseVersionId}/data-blocks/unattached`);
  },
};

export default dataBlockService;
