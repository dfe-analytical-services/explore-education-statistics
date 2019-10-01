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

const transformResponse: AxiosTransformer[] = [
  data => {
    let parsed;
    try {
      parsed = JSON.parse(data, (key, value) => {
        try {
          const className: string = value['_class'];

          if (className) {
            const filterClass = classMap[className];
            if (filterClass) {
              // @ts-ignore
              value = new filterClass(...JSON.parse(value['_construct']));
            }
          }
        } catch (_) {
          //
        }

        return value;
      });
    } catch (_) {
      // nothing
    }

    return parsed;
  },
];

const service: DataBlockService = {
  async getDataBlocks(releaseId: string) {
    return client.get<DataBlock[]>(`/release/${releaseId}/datablocks`, {
      transformResponse,
    });
  },

  async postDataBlock(releaseId: string, dataBlock: DataBlock) {
    return client.post<DataBlock>(
      `/release/${releaseId}/datablocks`,
      dataBlock,
      { transformResponse },
    );
  },
};

export default service;
