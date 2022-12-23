import client from '@admin/services/utils/service';
import { KeyStatisticDataBlock } from '@common/services/publicationService';

export interface KeyStatisticDataBlockCreateRequest {
  dataBlockId: string;
  trend?: string;
  guidanceTitle?: string;
  guidanceText?: string;
}

export interface KeyStatisticDataBlockUpdateRequest {
  trend?: string;
  guidanceTitle?: string;
  guidanceText?: string;
}

const keyStatisticService = {
  createKeyStatisticDataBlock(
    releaseId: string,
    request: KeyStatisticDataBlockCreateRequest,
  ): Promise<KeyStatisticDataBlock> {
    return client.post(
      `/release/${releaseId}/key-statistic-data-block`,
      request,
    );
  },

  updateKeyStatisticDataBlock(
    releaseId: string,
    keyStatisticId: string,
    request: KeyStatisticDataBlockUpdateRequest,
  ): Promise<KeyStatisticDataBlock> {
    return client.put(
      `/release/${releaseId}/key-statistic-data-block/${keyStatisticId}`,
      request,
    );
  },

  deleteKeyStatistic(releaseId: string, keyStatisticId: string): Promise<void> {
    return client.delete(
      `/release/${releaseId}/key-statistic/${keyStatisticId}`,
    );
  },
};

export default keyStatisticService;
