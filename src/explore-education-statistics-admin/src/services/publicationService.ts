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

export interface ContactPermissions {
  canUpdatePublication: boolean;
}

export interface Contact {
  contactName: string;
  contactTelNo: string;
  teamEmail: string;
  teamName: string;
  permissions?: ContactPermissions;
}

export interface ContactSave {
  contactName: string;
  contactTelNo: string;
  teamEmail: string;
  teamName: string;
}

export interface ExternalMethodology {
  title: string;
  url: string;
}

export interface ExternalMethodologySaveRequest {
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
  contact: Contact;
  permissions: PublicationPermissions;
  supersededById?: string;
  isSuperseded?: boolean;
}

export interface PublicationPermissions {
  canAdoptMethodologies: boolean;
  canCreateReleases: boolean;
  canUpdatePublication: boolean;
  canUpdatePublicationTitle: boolean;
  canUpdatePublicationSupersededBy: boolean;
  canCreateMethodologies: boolean;
  canManageExternalMethodology: boolean;
}

export interface PublicationWithPermissions extends Publication {
  permissions: PublicationPermissions;
}

export interface Publication {
  id: string;
  title: string;
  summary: string;
  slug: string;
  theme: IdTitlePair;
  topic: IdTitlePair;
  supersededById?: string;
  isSuperseded?: boolean;
  permissions?: PublicationPermissions;
}

export interface PublicationMethodologyDetails {
  selectedMethodologyId?: string;
  externalMethodology?: ExternalMethodology;
}

export interface PublicationSaveRequest {
  title: string;
  summary: string;
  supersededById?: string;
  topicId: string;
}

export interface PublicationCreateRequest {
  title: string;
  summary: string;
  contact: ContactSave;
  supersededById?: string;
  topicId: string;
}

export interface ListReleasesParams {
  live?: boolean;
  page?: number;
  pageSize?: number;
  permissions?: boolean;
}

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
    publication: PublicationCreateRequest,
  ): Promise<Publication> {
    return client.post('/publications', publication);
  },

  updatePublication(
    publicationId: string,
    publication: PublicationSaveRequest,
  ): Promise<Publication> {
    return client.put(`/publications/${publicationId}`, publication);
  },

  getPublication(
    publicationId: string,
    permissions = false,
  ): Promise<Publication> {
    return client.get<Publication>(`/publications/${publicationId}`, {
      params: { permissions },
    });
  },

  getMyPublication(publicationId: string): Promise<MyPublication> {
    return client.get<MyPublication>(`/me/publication/${publicationId}`);
  },

  getExternalMethodology(
    publicationId: string,
  ): Promise<ExternalMethodology | undefined> {
    return client
      .get<ExternalMethodology>(
        `/publication/${publicationId}/external-methodology`,
      )
      .catch(err => {
        if (err.response.status !== 404) {
          throw err;
        }
        return undefined;
      });
  },

  updateExternalMethodology(
    publicationId: string,
    updatedExternalMethodology: ExternalMethodologySaveRequest,
  ): Promise<ExternalMethodology> {
    return client.put<ExternalMethodology>(
      `/publication/${publicationId}/external-methodology`,
      updatedExternalMethodology,
    );
  },

  removeExternalMethodology(publicationId: string): Promise<boolean> {
    return client.delete(`/publication/${publicationId}/external-methodology`);
  },

  getContact(publicationId: string, permissions = false): Promise<Contact> {
    return client.get(`publication/${publicationId}/contact`, {
      params: { permissions },
    });
  },

  updateContact(
    publicationId: string,
    updatedContact: ContactSave,
  ): Promise<Contact> {
    return client.put(`publication/${publicationId}/contact`, updatedContact);
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
