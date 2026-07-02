export interface ServiceRequest {
  categoryId: number;
  title: string;
  description?: string;
  priceType: string;
  price: number;
  estimatedDurationMinutes: number;
  materialsIncluded?: string;
  availableCities?: string;
  tags?: string;
  images: string[];
}

export interface ServiceResponse {
  id: number;
  workerProfileId: number;
  workerName: string;
  categoryId: number;
  categoryName: string;
  title: string;
  description?: string;
  priceType: string;
  price: number;
  estimatedDurationMinutes: number;
  materialsIncluded?: string;
  availableCities?: string;
  tags?: string;
  isActive: boolean;
  images: string[];
  createdAt: string;
}
