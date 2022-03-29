import {
  LegacyRelease,
  UpdateLegacyRelease,
} from '@admin/services/legacyReleaseService';
import {
  BasicMethodologyVersion,
  MyMethodologyVersion,
} from '@admin/services/methodologyService';
import {
  MyRelease,
  Release,
  ReleaseSummary,
} from '@admin/services/releaseService';
import { IdTitlePair } from '@admin/services/types/common';
import client from '@admin/services/utils/service';
import { OmitStrict } from '@common/types';

export interface PublicationContactDetails {
  id: string;
  contactName: string;
  contactTelNo: string;
  teamEmail: string;
  teamName?: string;
}

export interface SavePublicationContact {
  contactName: string;
  contactTelNo: string;
  teamEmail: string;
  teamName?: string;
}

export interface ExternalMethodology {
  title: string;
  url: string;
}

export interface MyPublication {
  id: string;
  title: string;
  releases: MyRelease[];
  methodologies: MyPublicationMethodology[];
  externalMethodology?: ExternalMethodology;
  legacyReleases: LegacyRelease[];
  topicId: string;
  themeId: string;
  contact?: PublicationContactDetails;
  permissions: {
    canAdoptMethodologies: boolean;
    canCreateReleases: boolean;
    canUpdatePublication: boolean;
    canUpdatePublicationTitle: boolean;
    canCreateMethodologies: boolean;
    canManageExternalMethodology: boolean;
  };
}

export interface MyPublicationMethodology {
  owner: boolean;
  methodology: MyMethodologyVersion;
  permissions: {
    canDropMethodology: boolean;
  };
}

export interface BasicPublicationDetails {
  id: string;
  title: string;
  slug: string;
  contact?: PublicationContactDetails;
  releases?: Release[];
  methodologies?: BasicMethodologyVersion[];
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
  contact: SavePublicationContact;
  selectedMethodologyId?: string;
  externalMethodology?: ExternalMethodology;
  supersededById?: string;
  topicId: string;
}

export type CreatePublicationRequest = SavePublicationRequest;
export type UpdatePublicationRequest = SavePublicationRequest;

export type UpdatePublicationLegacyRelease = Partial<
  OmitStrict<UpdateLegacyRelease, 'publicationId'>
>;

const publicationService = {
  getMyPublicationsByTopic(topicId: string): Promise<MyPublication[]> {
    return client.get<MyPublication[]>('/me/publications', {
      params: { topicId },
    });
  },
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

  getMyPublication(publicationId: string): Promise<MyPublication> {
    return client.get<MyPublication>(`/me/publication/${publicationId}`);
  },

  getReleases(publicationId: string): Promise<ReleaseSummary[]> {
    return client.get<ReleaseSummary[]>(
      `/publication/${publicationId}/releases`,
    );
  },

  getPublicationReleaseTemplate: (
    publicationId: string,
  ): Promise<IdTitlePair | undefined> => {
    return client.get(`/publications/${publicationId}/releases/template`);
  },

  getAdoptableMethodologies(
    publicationId: string,
  ): Promise<BasicMethodologyVersion[]> {
    return client.get<BasicMethodologyVersion[]>(
      `/publication/${publicationId}/adoptable-methodologies`,
    );
  },

  adoptMethodology(
    publicationId: string,
    methodologyId: string,
  ): Promise<BasicMethodologyVersion> {
    return client.put(
      `/publication/${publicationId}/methodology/${methodologyId}`,
    );
  },

  dropMethodology(
    publicationId: string,
    methodologyId: string,
  ): Promise<BasicMethodologyVersion> {
    return client.delete(
      `/publication/${publicationId}/methodology/${methodologyId}`,
    );
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
