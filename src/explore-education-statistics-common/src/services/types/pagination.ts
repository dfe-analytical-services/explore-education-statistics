export interface PaginatedList<T> {
  results: T[];
  paging: Pagination;
}

export interface Pagination {
  page: number;
  pageSize: number;
  totalResults: number;
  totalPages: number;
}
