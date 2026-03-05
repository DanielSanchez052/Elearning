export interface ApiError {
  error: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface UploadResultDto {
  url: string;
  fileName: string;
  fileSizeBytes: number;
  contentType: string;
}