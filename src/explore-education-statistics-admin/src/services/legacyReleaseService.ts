import client from '@admin/services/utils/service';

export interface LegacyRelease {
  id: string;
  description: string;
  url: string;
}

export interface ReleaseSeriesItem extends LegacyRelease {
  order: number;
  isLegacy: boolean;
  isDraft: boolean;
  isAmendment: boolean;
  isLatest: boolean;
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

export interface UpdateReleaseSeriesItem {
  id: string;
  order: number;
  isLegacy: boolean;
  isAmendment: boolean;
  isLatest: boolean;
}

const legacyReleaseService = {
  getLegacyRelease(id: string): Promise<LegacyRelease> {
    return client.get(`/legacy-releases/${id}`);
  },
  getReleaseSeriesView(publicationId: string): Promise<ReleaseSeriesItem[]> {
    return client.get(`/publications/${publicationId}/release-series-view`);
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
