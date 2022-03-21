import client from '@admin/services/utils/service';
import { SubjectDataGuidance } from '@common/services/releaseDataGuidanceService';

export interface ReleaseDataGuidance {
  id: string;
  content: string;
  subjects: SubjectDataGuidance[];
}

const releaseDataGuidanceService = {
  getDataGuidance(releaseId: string): Promise<ReleaseDataGuidance> {
    return client.get(`/release/${releaseId}/data-guidance`);
  },
  updateDataGuidance(
    releaseId: string,
    data: {
      content: string;
      subjects: {
        id: string;
        content: string;
      }[];
    },
  ): Promise<ReleaseDataGuidance> {
    return client.patch(`/release/${releaseId}/data-guidance`, data);
  },
};

export default releaseDataGuidanceService;
