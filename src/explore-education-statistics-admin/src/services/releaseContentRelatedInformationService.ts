import client from '@admin/services/utils/service';
import { BasicLink, Release } from '@common/services/publicationService';

const releaseContentRelatedInformationService = {
  getAll: (
    releaseVersionId: string,
  ): Promise<Release['relatedInformation']> => {
    return client.get(
      `/release/${releaseVersionId}/content/related-information`,
    );
  },
  create: (
    releaseVersionId: string,
    link: Omit<BasicLink, 'id'>,
  ): Promise<BasicLink[]> => {
    return client.post(
      `/release/${releaseVersionId}/content/related-information`,
      link,
    );
  },
  update: (
    releaseVersionId: string,
    link: { id: string; description: string; url: string },
  ): Promise<BasicLink[]> => {
    return client.put(
      `/release/${releaseVersionId}/content/related-information/${link.id}`,
      link,
    );
  },
  delete: (releaseVersionId: string, linkId: string): Promise<BasicLink[]> => {
    return client.delete(
      `/release/${releaseVersionId}/content/related-information/${linkId}`,
    );
  },
};

export default releaseContentRelatedInformationService;
