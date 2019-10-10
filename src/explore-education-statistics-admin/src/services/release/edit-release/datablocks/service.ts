/* eslint-disable */
import axios, { AxiosTransformer } from 'axios';

import client from '@admin/services/util/service';
import { DataBlock } from '@admin/services/release/edit-release/datablocks/types';

import {
  CategoryFilter,
  Indicator,
  TimePeriodFilter,
  LocationFilter,
} from '@common/modules/full-table/types/filters';
import { Dictionary } from '@common/types/util';

export interface DataBlockService {
  getDataBlocks: (releaseId: string) => Promise<DataBlock[]>;
  postDataBlock: (
    releaseId: string,
    dataBlock: DataBlock,
  ) => Promise<DataBlock>;
  putDataBlock: (
    dataBlockId: string,
    dataBlock: DataBlock,
  ) => Promise<DataBlock>;
  deleteDataBlock: (id: string) => Promise<void>;
}

type AllowedClasses =
  | typeof CategoryFilter
  | typeof Indicator
  | typeof TimePeriodFilter
  | typeof LocationFilter;

const classMap: Dictionary<AllowedClasses> = {
  CategoryFilter,
  Indicator,
  TimePeriodFilter,
  LocationFilter,
};

const service: DataBlockService = {
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
