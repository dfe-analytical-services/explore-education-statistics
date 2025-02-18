import client from '@admin/services/utils/service';
import { ReleaseType } from '@common/services/types/releaseType';
import { ReleaseVersion } from './releaseVersionService';

export interface CreateReleaseVersionRequest {
  publicationId: string;
  templateReleaseId?: string;
  year: number;
  timePeriodCoverage: {
    value: string;
  };
  type: ReleaseType;
  label?: string;
}

const releaseService = {
  createReleaseVersion(createRequest: CreateReleaseVersionRequest): Promise<Release> {
    return client.post(`/releases`, createRequest);
  },
};

export default releaseService;
