import { PreReleaseSummaryViewModel } from '@admin/services/pre-release/types';
import client from '@admin/services/util/service';

const preReleaseService = {
  getPreReleaseSummary(releaseId: string): Promise<PreReleaseSummaryViewModel> {
    return client.get<PreReleaseSummaryViewModel>(
      `/release/${releaseId}/prerelease`,
    );
  },
};

export default preReleaseService;
