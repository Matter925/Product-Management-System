export interface IPaginationResult<T> {
  data: T[] | null;
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
}

export interface IMetaData {
  TotalCount: number;
  PageSize: number;
  CurrentPage: number;
  TotalPages: number;
}