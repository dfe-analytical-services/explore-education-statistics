import { ContactDetails, SaveContact } from '@admin/services/contactService';
import { ExternalMethodology } from '@admin/services/dashboardService';
import {
  LegacyRelease,
  UpdateLegacyRelease,
} from '@admin/services/legacyReleaseService';
import { BasicMethodology } from '@admin/services/methodologyService';
import { IdTitlePair } from '@admin/services/types/common';
import client from '@admin/services/utils/service';
import { OmitStrict } from '@common/types';

export interface BasicPublicationDetails {
  id: string;
  title: string;
  contact?: ContactDetails;
  methodology?: BasicMethodology;
  externalMethodology?: ExternalMethodology;
  legacyReleases: LegacyRelease[];
  themeId: string;
  topicId: string;
}

export interface PublicationMethodologyDetails {
  selectedMethodologyId?: string;
  externalMethodology?: ExternalMethodology;
}

export interface SavePublicationRequest {
  title: string;
  contact: SaveContact;
  selectedMethodologyId?: string;
  externalMethodology?: ExternalMethodology;
  topicId: string;
}

export type CreatePublicationRequest = SavePublicationRequest;
export type UpdatePublicationRequest = SavePublicationRequest;

export type UpdatePublicationLegacyRelease = Partial<
  OmitStrict<UpdateLegacyRelease, 'publicationId'>
>;

const publicationService = {
  createPublication(
    publication: CreatePublicationRequest,
  ): Promise<BasicPublicationDetails> {
    return client.post('/publications', publication);
  },

  updatePublication(
    publicationId: string,
    publication: UpdatePublicationRequest,
  ): Promise<BasicPublicationDetails> {
    return client.put(`/publications/${publicationId}`, publication);
  },

  getPublication(publicationId: string): Promise<BasicPublicationDetails> {
    return client.get<BasicPublicationDetails>(
      `/publications/${publicationId}`,
    );
  },

  getPublicationReleaseTemplate: (
    publicationId: string,
  ): Promise<IdTitlePair | undefined> => {
    return client.get(`/publications/${publicationId}/releases/template`);
  },

  updatePublicationMethodology({
    publicationId,
    selectedMethodologyId: methodologyId,
    externalMethodology,
  }: PublicationMethodologyDetails & { publicationId: string }) {
    return client.put(`/publications/${publicationId}/methodology`, {
      methodologyId,
      externalMethodology,
    });
  },

  partialUpdateLegacyReleases(
    publicationId: string,
    legacyReleases: UpdatePublicationLegacyRelease[],
  ): Promise<LegacyRelease> {
    return client.patch(
      `/publications/${publicationId}/legacy-releases`,
      legacyReleases,
    );
  },
};

export default publicationService;
