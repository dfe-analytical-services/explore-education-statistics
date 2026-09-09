import client from '@admin/services/utils/service';

export interface FeaturedTable {
  id: string;
  name: string;
  description: string;
  dataBlockVersionId: string;
  dataBlockId: string;
  order: number;
}

export interface FeaturedTableBasic {
  name: string;
  description: string;
}

export type FeaturedTableCreateRequest = FeaturedTableBasic & {
  dataBlockVersionId: string;
};

type FeaturedTableUpdateRequest = FeaturedTableBasic;

const featuredTableService = {
  getFeaturedTable(
    releaseId: string,
    dataBlockVersionId: string,
  ): Promise<FeaturedTable> {
    return client.get(
      `/releases/${releaseId}/featured-tables/${dataBlockVersionId}`,
    );
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
    dataBlockVersionId: string,
    featuredTable: FeaturedTableUpdateRequest,
  ): Promise<FeaturedTable> {
    return client.post(
      `/releases/${releaseId}/featured-tables/${dataBlockVersionId}`,
      featuredTable,
    );
  },
  deleteFeaturedTable(
    releaseId: string,
    dataBlockVersionId: string,
  ): Promise<void> {
    return client.delete(
      `/releases/${releaseId}/featured-tables/${dataBlockVersionId}`,
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
