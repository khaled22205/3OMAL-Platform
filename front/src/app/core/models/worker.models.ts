export interface WorkerProfileRequest {
  photo?: string;
  coverPhoto?: string;
  biography?: string;
  yearsOfExperience: number;
  skills?: string;
  serviceAreas?: string;
  hourlyRate: number;
  startingPrice: number;
  minimumJobValue?: number;
}

export interface WorkerProfileResponse {
  id: number;
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  photo?: string;
  coverPhoto?: string;
  biography?: string;
  yearsOfExperience: number;
  skills?: string;
  serviceAreas?: string;
  hourlyRate: number;
  startingPrice: number;
  completedJobs: number;
  averageRating: number;
  isAvailable: boolean;
  isVerified: boolean;
  availability: WorkerAvailabilityResponse[];
  portfolio: WorkerPortfolioResponse[];
}

export interface WorkerAvailabilityRequest {
  dayOfWeek: string;
  startTime: string;
  endTime: string;
  isAvailable: boolean;
}

export interface WorkerAvailabilityResponse {
  id: number;
  dayOfWeek: string;
  startTime: string;
  endTime: string;
  isAvailable: boolean;
}

export interface WorkerPortfolioResponse {
  id: number;
  mediaType: string;
  mediaUrl: string;
  title?: string;
}

export interface WorkerPortfolioRequest {
  mediaType: string;
  mediaUrl: string;
  title?: string;
}

export interface WorkerSearchRequest {
  searchTerm?: string;
  categoryId?: number;
  minRating?: number;
  maxPrice?: number;
  city?: string;
  area?: string;
  minExperience?: number;
  availableNow?: boolean;
  sortBy?: string;
  page: number;
  pageSize: number;
}

export interface WorkerSummaryResponse {
  id: number;
  firstName: string;
  lastName: string;
  photo?: string;
  biography?: string;
  yearsOfExperience: number;
  startingPrice: number;
  averageRating: number;
  completedJobs: number;
  isAvailable: boolean;
  isVerified: boolean;
  serviceAreas?: string;
  categories: string[];
}

export interface WorkerStatusRequest {
  isAvailable: boolean;
}
