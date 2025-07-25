import client from '@admin/services/utils/service';
import { ReleaseType } from '@common/services/types/releaseType';
import { ReleaseVersion } from './releaseVersionService';

export interface CreateReleaseRequest {
  publicationId: string;
  templateReleaseId?: string;
  year: number;
  timePeriodCoverage: {
    value: string;
  };
  type: ReleaseType;
  label?: string;
  publishingOrganisations?: string[];
}

export interface UpdateReleaseRequest {
  label?: string;
}

export interface Release {
  id: string;
  publicationId: string;
  slug: string;
  timePeriodCoverage: string;
  year: number;
  label?: string;
  title: string;
}

const releaseService = {
  createRelease(createRequest: CreateReleaseRequest): Promise<ReleaseVersion> {
    return client.post(`/releases`, createRequest);
  },

  updateRelease(
    releaseId: string,
    updateRequest: UpdateReleaseRequest,
  ): Promise<Release> {
    return client.patch(`/releases/${releaseId}`, updateRequest);
  },
};

export default releaseService;
