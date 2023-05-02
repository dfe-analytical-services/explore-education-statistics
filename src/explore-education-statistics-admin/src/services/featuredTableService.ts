import client from '@admin/services/utils/service';

export interface FeaturedTable {
  id: string;
  name: string;
  description: string;
  dataBlockId: string;
  order: number;
}

export interface FeaturedTableCreateRequest {
  name: string;
  description: string;
  dataBlockId: string;
}

export interface FeaturedTableUpdateRequest {
  name: string;
  description: string;
}

const featuredTableService = {
  get(releaseId: string, dataBlockId: string): Promise<FeaturedTable> {
    return client.get(`/releases/${releaseId}/data-blocks/${dataBlockId}`);
  },

  list(releaseId: string): Promise<FeaturedTable[]> {
    return client.get(`/releases/${releaseId}/featured-tables`);
  },

  create(
    releaseId: string,
    featuredTable: FeaturedTableCreateRequest,
  ): Promise<FeaturedTable> {
    return client.post(`/releases/${releaseId}/featured-tables`, featuredTable);
  },

  update(
    releaseId: string,
    id: string,
    featuredTable: FeaturedTableUpdateRequest,
  ): Promise<FeaturedTable> {
    return client.post(
      `/releases/${releaseId}/featured-tables/${id}`,
      featuredTable,
    );
  },

  delete(releaseId: string, id: string): Promise<void> {
    return client.delete(`/releases/${releaseId}/featured-tables/${id}`);
  },

  reorder(releaseId: string, newOrder: string[]): Promise<FeaturedTable[]> {
    return client.put(`releases/${releaseId}/featured-tables/order`, newOrder);
  },
};

export default featuredTableService;
