import client from '@admin/services/utils/service';

export interface LegacyRelease {
  id: string;
  description: string;
  url: string;
  order: number;
}

export interface CreateLegacyRelease {
  description: string;
  url: string;
  publicationId: string;
}

export interface UpdateLegacyRelease extends CreateLegacyRelease {
  order: number;
}

const legacyReleaseService = {
  getLegacyRelease(id: string): Promise<LegacyRelease> {
    return client.get(`/legacy-releases/${id}`);
  },
  listLegacyReleases(publicationId: string): Promise<LegacyRelease[]> {
    return client.get(`/publications/${publicationId}/legacy-releases`);
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
