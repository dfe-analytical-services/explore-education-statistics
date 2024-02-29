import client from '@admin/services/utils/service';

export interface LegacyRelease {
  id: string;
  description: string;
  url: string;
}

export interface ReleaseSeriesItem {
  id: string;
  isLegacyLink: boolean;
  description: string;

  publicationSlug?: string;
  releaseSlug?: string;

  legacyLinkUrl?: string;
}

export interface CreateLegacyRelease { // @MarkFix remove
  description: string;
  url: string;
  publicationId: string;
}

export interface UpdateLegacyRelease { // @MarkFix remove
  description: string;
  url: string;
  publicationId: string;
}

export interface UpdateReleaseSeriesItem {
  // @MarkFix rename to match backend's ReleaseSeriesItemUpdateRequest
  releaseParentId?: string;

  legacyLinkDescription?: string;
  legacyLinkUrl?: string;
}

const legacyReleaseService = {
  getLegacyRelease(id: string): Promise<LegacyRelease> {
    return client.get(`/legacy-releases/${id}`);
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
