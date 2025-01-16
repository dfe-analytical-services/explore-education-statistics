import client from '@admin/services/utils/service';
import { ReleaseType } from '@common/services/types/releaseType';
import { Release } from './releaseVersionService';

export interface CreateReleaseRequest {
  publicationId: string;
  templateReleaseId?: string;
  year: number;
  timePeriodCoverage: {
    value: string;
  };
  type: ReleaseType;
}

const releaseVersionService = {
  createRelease(createRequest: CreateReleaseRequest): Promise<Release> {
    return client.post(`/releases`, createRequest);
  },
};

export default releaseVersionService;
