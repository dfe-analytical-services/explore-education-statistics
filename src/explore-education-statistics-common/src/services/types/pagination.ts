export interface PaginatedList<T> {
  results: T[];
  paging: Paging;
}

export interface Paging {
  page: number;
  pageSize: number;
  totalResults: number;
  totalPages: number;
}
