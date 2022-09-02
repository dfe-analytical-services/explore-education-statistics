import {
  LegacyRelease,
  UpdateLegacyRelease,
} from '@admin/services/legacyReleaseService';
import {
  MethodologyVersion,
  MethodologyVersionSummary,
} from '@admin/services/methodologyService';
import { Release, ReleaseSummary } from '@admin/services/releaseService';
import { IdTitlePair } from '@admin/services/types/common';
import client from '@admin/services/utils/service';
import { OmitStrict } from '@common/types';
import { PublicationSummary } from '@common/services/publicationService';
import { PaginatedList } from '@common/services/types/pagination';

export interface PublicationContactDetails {
  id: string;
  contactName: string;
  contactTelNo: string;
  teamEmail: string;
  teamName: string;
}

export interface SavePublicationContact {
  contactName: string;
  contactTelNo: string;
  teamEmail: string;
  teamName: string;
}

export interface ExternalMethodology {
  title: string;
  url: string;
}

export interface MyPublication {
  id: string;
  title: string;
  summary: string;
  releases: Release[];
  methodologies: MethodologyVersionSummary[];
  externalMethodology?: ExternalMethodology;
  topicId: string;
  themeId: string;
  contact: PublicationContactDetails;
  permissions: {
    canAdoptMethodologies: boolean;
    canCreateReleases: boolean;
    canUpdatePublication: boolean;
    canUpdatePublicationTitle: boolean;
    canUpdatePublicationSupersededBy: boolean;
    canCreateMethodologies: boolean;
    canManageExternalMethodology: boolean;
  };
  supersededById?: string;
  isSuperseded?: boolean;
}

export interface BasicPublicationDetails {
  id: string;
  title: string;
  summary: string;
  slug: string;
  contact: PublicationContactDetails;
  releases?: Release[]; // @MarkFix remove
  externalMethodology?: ExternalMethodology; // @MarkFix remove
  themeId: string;
  topicId: string;
}

export interface PublicationMethodologyDetails {
  selectedMethodologyId?: string;
  externalMethodology?: ExternalMethodology;
}

export interface SavePublicationRequest {
  title: string;
  summary: string;
  contact: SavePublicationContact;
  selectedMethodologyId?: string;
  externalMethodology?: ExternalMethodology;
  supersededById?: string;
  topicId: string;
}

export interface ListReleasesParams {
  live?: boolean;
  page?: number;
  pageSize?: number;
  permissions?: boolean;
}

export type CreatePublicationRequest = SavePublicationRequest;
export type UpdatePublicationRequest = SavePublicationRequest;

export type UpdatePublicationLegacyRelease = Partial<
  OmitStrict<UpdateLegacyRelease, 'publicationId'>
>;

const publicationService = {
  getMyPublicationsByTopic(topicId: string): Promise<MyPublication[]> {
    return client.get('/me/publications', {
      params: { topicId },
    });
  },

  getPublicationSummaries(): Promise<PublicationSummary[]> {
    return client.get('/publication-summaries');
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

  getExternalMethodology(
    publicationId: string,
  ): Promise<ExternalMethodology | undefined> {
    // TODO EES-3666 Replace with external methodology request
    return client
      .get<MyPublication>(`/me/publication/${publicationId}`)
      .then(response => response.externalMethodology);
  },

  listReleases<TReleaseSummary extends ReleaseSummary = ReleaseSummary>(
    publicationId: string,
    params?: ListReleasesParams,
  ): Promise<PaginatedList<TReleaseSummary>> {
    return client.get<PaginatedList<TReleaseSummary>>(
      `/publication/${publicationId}/releases`,
      { params },
    );
  },

  getPublicationReleaseTemplate: (
    publicationId: string,
  ): Promise<IdTitlePair | undefined> => {
    return client.get(`/publications/${publicationId}/releases/template`);
  },

  getAdoptableMethodologies(
    publicationId: string,
  ): Promise<MethodologyVersion[]> {
    return client.get<MethodologyVersion[]>(
      `/publication/${publicationId}/adoptable-methodologies`,
    );
  },

  adoptMethodology(
    publicationId: string,
    methodologyId: string,
  ): Promise<MethodologyVersion> {
    return client.put(
      `/publication/${publicationId}/methodology/${methodologyId}`,
    );
  },

  dropMethodology(
    publicationId: string,
    methodologyId: string,
  ): Promise<MethodologyVersion> {
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
