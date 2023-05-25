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

interface KeyStatisticTextSaveRequest {
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
    releaseId: string,
    request: KeyStatisticDataBlockCreateRequest,
  ): Promise<KeyStatisticDataBlock> {
    return client.post(
      `/release/${releaseId}/key-statistic-data-block`,
      request,
    );
  },

  createKeyStatisticText(
    releaseId: string,
    request: KeyStatisticTextCreateRequest,
  ): Promise<KeyStatisticText> {
    return client.post(`/release/${releaseId}/key-statistic-text`, request);
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

  updateKeyStatisticText(
    releaseId: string,
    keyStatisticId: string,
    request: KeyStatisticTextUpdateRequest,
  ): Promise<KeyStatisticText> {
    return client.put(
      `/release/${releaseId}/key-statistic-text/${keyStatisticId}`,
      request,
    );
  },

  deleteKeyStatistic(releaseId: string, keyStatisticId: string): Promise<void> {
    return client.delete(
      `/release/${releaseId}/key-statistic/${keyStatisticId}`,
    );
  },

  reorderKeyStatistics(
    releaseId: string,
    order: string[],
  ): Promise<KeyStatistic[]> {
    return client.put(`/release/${releaseId}/key-statistic/order`, order);
  },
};

export default keyStatisticService;
