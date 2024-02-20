import client from '@admin/services/utils/service';

export interface LegacyRelease {
  id: string;
  description: string;
  url: string;
}

export interface CombinedRelease extends LegacyRelease {
  order: number;
  isLegacy: boolean;
  isDraft: boolean;
  isAmendment: boolean;
}

export interface CreateLegacyRelease {
  description: string;
  url: string;
  publicationId: string;
}

export interface UpdateLegacyRelease {
  description: string;
  url: string;
  publicationId: string;
}

export interface UpdateCombinedRelease {
  id: string;
  order: number;
  isLegacy: boolean;
  isAmendment: boolean;
}

const legacyReleaseService = {
  getLegacyRelease(id: string): Promise<LegacyRelease> {
    return client.get(`/legacy-releases/${id}`);
  },
  listLegacyReleases(publicationId: string): Promise<LegacyRelease[]> {
    return client.get(`/publications/${publicationId}/legacy-releases`);
  },
  listCombinedReleases(publicationId: string): Promise<CombinedRelease[]> {
    return client.get(`/publications/${publicationId}/combined-releases`);
  },
  createLegacyRelease(data: CreateLegacyRelease): Promise<LegacyRelease> {
    return client.post('/legacy-releases', data);
  },
  updateLegacyRelease(
    id: string,
    data: UpdateLegacyRelease,
  ): Promise<LegacyRelease> {
    return client.put(`/legacy-releases/${id}`, data);
  },
  deleteLegacyRelease(id: string): Promise<void> {
    return client.delete(`/legacy-releases/${id}`);
  },
};

export default legacyReleaseService;
