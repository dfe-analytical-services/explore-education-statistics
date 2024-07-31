import client from '@admin/services/utils/service';
import { DataSetDataGuidance } from '@common/services/releaseDataGuidanceService';

export interface ReleaseDataGuidance {
  id: string;
  content: string;
  dataSets: DataSetDataGuidance[];
}

const releaseDataGuidanceService = {
  getDataGuidance(releaseVersionId: string): Promise<ReleaseDataGuidance> {
    return client.get(`/release/${releaseVersionId}/data-guidance`);
  },
  updateDataGuidance(
    releaseVersionId: string,
    data: {
      content: string;
      dataSets: {
        fileId: string;
        content: string;
      }[];
    },
  ): Promise<ReleaseDataGuidance> {
    return client.patch(`/release/${releaseVersionId}/data-guidance`, data);
  },
};

export default releaseDataGuidanceService;
