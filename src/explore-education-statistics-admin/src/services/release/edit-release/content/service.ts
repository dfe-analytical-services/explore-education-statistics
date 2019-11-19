import client from '@admin/services/util/service';
import { Release } from '@common/services/publicationService';
import { ManageContentPageViewModel } from './types';

export interface ReleaseContentService {
  getRelease: (releaseId: string) => Promise<Release>;
  getContent: (releaseId: string) => Promise<ManageContentPageViewModel>;
}

const service: ReleaseContentService = {
  getRelease(releaseId: string): Promise<Release> {
    return client.get<Release>(`/releases/${releaseId}`);
  },
  getContent(releaseId: string): Promise<ManageContentPageViewModel> {
    return client.get<ManageContentPageViewModel>(
      `/release/${releaseId}/content`,
    );
  },
};

export default service;
