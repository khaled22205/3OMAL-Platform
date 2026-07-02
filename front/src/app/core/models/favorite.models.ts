export interface AddFavoriteRequest {
  workerProfileId?: number;
  workerServiceId?: number;
}

export interface FavoriteResponse {
  id: number;
  workerProfileId?: number;
  workerName?: string;
  workerPhoto?: string;
  workerRating?: number;
  workerServiceId?: number;
  serviceName?: string;
  servicePrice?: number;
  createdAt: string;
}
