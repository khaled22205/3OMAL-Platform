export interface WrappedResponse<T> {
  success: boolean;
  data: T;
  message: string | null;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface ApiError {
  success: false;
  message: string;
  errors?: string[];
}
