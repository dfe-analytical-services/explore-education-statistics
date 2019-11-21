import client from '@admin/services/util/service';
import { IdTitlePair } from '@admin/services/common/types';

export interface MethodologyService {
  getMethodologies(): Promise<IdTitlePair[]>;
}

const service: MethodologyService = {
  getMethodologies(): Promise<IdTitlePair[]> {
    return client.get<IdTitlePair[]>('/bau/methodology');
  },
};

export default service;
