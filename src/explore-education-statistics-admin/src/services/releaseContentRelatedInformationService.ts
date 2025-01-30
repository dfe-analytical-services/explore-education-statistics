import client from '@admin/services/utils/service';
import { BasicLink, ReleaseVersion } from '@common/services/publicationService';

const releaseContentRelatedInformationService = {
  getAll: (
    releaseId: string,
  ): Promise<ReleaseVersion['relatedInformation']> => {
    return client.get(`/release/${releaseId}/content/related-information`);
  },
  update: (
    releaseId: string,
    links: (BasicLink | Omit<BasicLink, 'id'>)[],
  ): Promise<BasicLink[]> => {
    return client.put(
      `/release/${releaseId}/content/related-information`,
      links,
    );
  },
};

export default releaseContentRelatedInformationService;
