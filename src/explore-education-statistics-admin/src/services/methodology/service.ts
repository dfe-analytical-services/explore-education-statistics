import client from '@admin/services/util/service';
import { MethodologyStatus } from '@admin/services/methodology/types';

export interface MethodologyService {
  getMethodologies(): Promise<MethodologyStatus[]>;
}

const service: MethodologyService = {
  getMethodologies(): Promise<MethodologyStatus[]> {
    return client.get<MethodologyStatus[]>('/bau/methodology');
  },
};

export default service;
