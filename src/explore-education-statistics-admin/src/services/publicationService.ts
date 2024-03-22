import { MethodologyVersion } from '@admin/services/methodologyService';
import { ReleaseSummary } from '@admin/services/releaseService';
import { IdTitlePair } from '@admin/services/types/common';
import client from '@admin/services/utils/service';
import { PublicationSummary } from '@common/services/publicationService';
import { PaginatedList } from '@common/services/types/pagination';
import { UserPublicationRole } from '@admin/services/userService';
import { isAxiosError } from 'axios';

export interface Contact {
  contactName: string;
  contactTelNo: string;
  teamEmail: string;
  teamName: string;
}

export interface ContactSave {
  contactName: string;
  contactTelNo?: string;
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

export interface PublicationPermissions {
  canUpdatePublication: boolean;
  canUpdatePublicationSummary: boolean;
  canCreateReleases: boolean;
  canAdoptMethodologies: boolean;
  canCreateMethodologies: boolean;
  canManageExternalMethodology: boolean;
  canManageReleaseSeries: boolean;
  canUpdateContact: boolean;
  canUpdateContributorReleaseRole: boolean;
  canViewReleaseTeamAccess: boolean;
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

export interface ReleaseSeriesLegacyLinkAddRequest {
  description: string;
  url: string;
}

export interface ReleaseSeriesItemUpdateRequest {
  id: string;

  releaseId?: string;

  legacyLinkDescription?: string;
  legacyLinkUrl?: string;
}

export interface ListReleasesParams {
  live?: boolean;
  page?: number;
  pageSize?: number;
  includePermissions?: boolean;
}

export interface ReleaseSeriesTableEntry {
  id: string;
  isLegacyLink: boolean;
  description: string;

  releaseId?: string;
  releaseSlug?: string;
  isLatest?: boolean;
  isPublished?: boolean;

  legacyLinkUrl?: string;
}

const publicationService = {
  listPublications(topicId?: string): Promise<Publication[]> {
    return client.get('/publications', {
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

  getPublication<TPublication extends Publication = Publication>(
    publicationId: string,
    includePermissions = false,
  ): Promise<TPublication> {
    return client.get<TPublication>(`/publications/${publicationId}`, {
      params: { includePermissions },
    });
  },

  getExternalMethodology(
    publicationId: string,
  ): Promise<ExternalMethodology | undefined> {
    return client
      .get<ExternalMethodology>(
        `/publication/${publicationId}/external-methodology`,
      )
      .catch(err => {
        if (isAxiosError(err) && err?.response?.status !== 404) {
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

  getContact<TContact extends Contact = Contact>(
    publicationId: string,
  ): Promise<TContact> {
    return client.get<TContact>(`publication/${publicationId}/contact`);
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

  getReleaseSeries(publicationId: string): Promise<ReleaseSeriesTableEntry[]> {
    return client.get(`/publications/${publicationId}/release-series`);
  },

  addReleaseSeriesLegacyLink(
    publicationId: string,
    newLegacyLink: ReleaseSeriesLegacyLinkAddRequest,
  ): Promise<ReleaseSeriesTableEntry[]> {
    return client.post(
      `publications/${publicationId}/release-series`,
      newLegacyLink,
    );
  },

  updateReleaseSeries(
    publicationId: string,
    updatedReleaseSeries: ReleaseSeriesItemUpdateRequest[],
  ): Promise<ReleaseSeriesTableEntry[]> {
    return client.put(
      `/publications/${publicationId}/release-series`,
      updatedReleaseSeries,
    );
  },

  listRoles(publicationId: string): Promise<UserPublicationRole[]> {
    return client.get(`/publications/${publicationId}/roles`);
  },
};

export default publicationService;
