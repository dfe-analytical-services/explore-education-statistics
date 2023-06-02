import client from '@admin/services/utils/service';

export interface FeaturedTable {
  id: string;
  name: string;
  description: string;
  dataBlockId: string;
  order: number;
}

export interface FeaturedTableBasic {
  name: string;
  description: string;
}

export type FeaturedTableCreateRequest = FeaturedTableBasic & {
  dataBlockId: string;
};

type FeaturedTableUpdateRequest = FeaturedTableBasic;

const featuredTableService = {
  getFeaturedTable(
    releaseId: string,
    dataBlockId: string,
  ): Promise<FeaturedTable> {
    return client.get(`/releases/${releaseId}/featured-tables/${dataBlockId}`);
  },
  listFeaturedTables(releaseId: string): Promise<FeaturedTable[]> {
    return client.get(`/releases/${releaseId}/featured-tables`);
  },
  createFeaturedTable(
    releaseId: string,
    featuredTable: FeaturedTableCreateRequest,
  ): Promise<FeaturedTable> {
    return client.post(`/releases/${releaseId}/featured-tables`, featuredTable);
  },
  updateFeaturedTable(
    releaseId: string,
    dataBlockId: string,
    featuredTable: FeaturedTableUpdateRequest,
  ): Promise<FeaturedTable> {
    return client.post(
      `/releases/${releaseId}/featured-tables/${dataBlockId}`,
      featuredTable,
    );
  },
  deleteFeaturedTable(releaseId: string, dataBlockId: string): Promise<void> {
    return client.delete(
      `/releases/${releaseId}/featured-tables/${dataBlockId}`,
    );
  },
  reorderFeaturedTables(
    releaseId: string,
    newOrder: string[],
  ): Promise<FeaturedTable[]> {
    return client.put(`releases/${releaseId}/featured-tables/order`, newOrder);
  },
};

export default featuredTableService;
