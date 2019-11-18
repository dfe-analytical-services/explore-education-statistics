import client from '@admin/services/util/service';
import { Release } from '@common/services/publicationService';
import { ManageContentPageViewModel } from './types';

export interface ReleaseContentService {
  getContent: (releaseId: string) => Promise<ManageContentPageViewModel>;
}

const service: ReleaseContentService = {
  getContent(releaseId: string): Promise<ManageContentPageViewModel> {
    return client.get<ManageContentPageViewModel>(
      `/release/${releaseId}/content`,
    );
  },
};

export default service;
