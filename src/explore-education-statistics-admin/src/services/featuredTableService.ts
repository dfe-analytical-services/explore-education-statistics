import client from '@admin/services/utils/service';

export interface FeaturedTable {
  id: string;
  name: string;
  description: string;
  dataBlockId: string;
  dataBlockParentId: string;
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
    releaseVersionId: string,
    dataBlockId: string,
  ): Promise<FeaturedTable> {
    return client.get(
      `/releases/${releaseVersionId}/featured-tables/${dataBlockId}`,
    );
  },
  listFeaturedTables(releaseVersionId: string): Promise<FeaturedTable[]> {
    return client.get(`/releases/${releaseVersionId}/featured-tables`);
  },
  createFeaturedTable(
    releaseVersionId: string,
    featuredTable: FeaturedTableCreateRequest,
  ): Promise<FeaturedTable> {
    return client.post(
      `/releases/${releaseVersionId}/featured-tables`,
      featuredTable,
    );
  },
  updateFeaturedTable(
    releaseVersionId: string,
    dataBlockId: string,
    featuredTable: FeaturedTableUpdateRequest,
  ): Promise<FeaturedTable> {
    return client.post(
      `/releases/${releaseVersionId}/featured-tables/${dataBlockId}`,
      featuredTable,
    );
  },
  deleteFeaturedTable(
    releaseVersionId: string,
    dataBlockId: string,
  ): Promise<void> {
    return client.delete(
      `/releases/${releaseVersionId}/featured-tables/${dataBlockId}`,
    );
  },
  reorderFeaturedTables(
    releaseVersionId: string,
    newOrder: string[],
  ): Promise<FeaturedTable[]> {
    return client.put(
      `releases/${releaseVersionId}/featured-tables/order`,
      newOrder,
    );
  },
};

export default featuredTableService;
