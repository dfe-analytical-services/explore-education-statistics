import client from '@admin/services/utils/service';
import { SubjectMetaGuidance } from '@common/services/releaseMetaGuidanceService';

export interface ReleaseMetaGuidance {
  id: string;
  content: string;
  subjects: SubjectMetaGuidance[];
}

const releaseMetaGuidanceService = {
  getMetaGuidance(releaseId: string): Promise<ReleaseMetaGuidance> {
    return client.get(`/release/${releaseId}/meta-guidance`);
  },
  updateMetaGuidance(
    releaseId: string,
    data: {
      content: string;
      subjects: {
        id: string;
        content: string;
      }[];
    },
  ): Promise<ReleaseMetaGuidance> {
    return client.patch(`/release/${releaseId}/meta-guidance`, data);
  },
};

export default releaseMetaGuidanceService;
