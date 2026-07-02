export interface CategoryRequest {
  name: string;
  description?: string;
  icon?: string;
  banner?: string;
  seoUrl?: string;
  parentCategoryId?: number;
  sortOrder: number;
  isActive: boolean;
}

export interface CategoryResponse {
  id: number;
  name: string;
  description?: string;
  icon?: string;
  banner?: string;
  seoUrl?: string;
  parentCategoryId?: number;
  parentCategoryName?: string;
  sortOrder: number;
  isActive: boolean;
  servicesCount: number;
  subCategories: CategoryResponse[];
}

export interface CategoryTreeResponse {
  id: number;
  name: string;
  seoUrl?: string;
  sortOrder: number;
  children: CategoryTreeResponse[];
}
