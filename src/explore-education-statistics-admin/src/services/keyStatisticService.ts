import client from '@admin/services/utils/service';
import {
  KeyStatistic,
  KeyStatisticDataBlock,
  KeyStatisticText,
} from '@common/services/publicationService';

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

export interface KeyStatisticTextSaveRequest {
  title: string;
  statistic: string;
  trend?: string;
  guidanceTitle?: string;
  guidanceText?: string;
}

export type KeyStatisticTextCreateRequest = KeyStatisticTextSaveRequest;
export type KeyStatisticTextUpdateRequest = KeyStatisticTextSaveRequest;

const keyStatisticService = {
  createKeyStatisticDataBlock(
    releaseVersionId: string,
    request: KeyStatisticDataBlockCreateRequest,
  ): Promise<KeyStatisticDataBlock> {
    return client.post(
      `/release/${releaseVersionId}/key-statistic-data-block`,
      request,
    );
  },

  createKeyStatisticText(
    releaseVersionId: string,
    request: KeyStatisticTextCreateRequest,
  ): Promise<KeyStatisticText> {
    return client.post(
      `/release/${releaseVersionId}/key-statistic-text`,
      request,
    );
  },

  updateKeyStatisticDataBlock(
    releaseVersionId: string,
    keyStatisticId: string,
    request: KeyStatisticDataBlockUpdateRequest,
  ): Promise<KeyStatisticDataBlock> {
    return client.put(
      `/release/${releaseVersionId}/key-statistic-data-block/${keyStatisticId}`,
      request,
    );
  },

  updateKeyStatisticText(
    releaseVersionId: string,
    keyStatisticId: string,
    request: KeyStatisticTextUpdateRequest,
  ): Promise<KeyStatisticText> {
    return client.put(
      `/release/${releaseVersionId}/key-statistic-text/${keyStatisticId}`,
      request,
    );
  },

  deleteKeyStatistic(
    releaseVersionId: string,
    keyStatisticId: string,
  ): Promise<void> {
    return client.delete(
      `/release/${releaseVersionId}/key-statistic/${keyStatisticId}`,
    );
  },

  reorderKeyStatistics(
    releaseVersionId: string,
    order: string[],
  ): Promise<KeyStatistic[]> {
    return client.put(
      `/release/${releaseVersionId}/key-statistic/order`,
      order,
    );
  },
};

export default keyStatisticService;
