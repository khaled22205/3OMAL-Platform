export interface DashboardStatsResponse {
  totalUsers: number;
  totalWorkers: number;
  totalCustomers: number;
  totalBookings: number;
  activeBookings: number;
  completedBookings: number;
  totalRevenue: number;
  totalCommission: number;
  totalCategories: number;
  pendingApprovals: number;
  monthlyBookings: MonthlyStats[];
  topCategories: TopCategoryStats[];
}

export interface MonthlyStats {
  year: number;
  month: number;
  count: number;
}

export interface TopCategoryStats {
  categoryName: string;
  bookingCount: number;
}

export interface UserManagementResponse {
  id: number;
  email: string;
  phoneNumber?: string;
  userName: string;
  roles: string[];
  emailConfirmed: boolean;
  phoneNumberConfirmed: boolean;
  isLockedOut: boolean;
  lockoutEnd?: string;
  createdAt: string;
}

export interface AdminUserUpdateRequest {
  email?: string;
  phoneNumber?: string;
  role?: string;
  lockoutEnabled?: boolean;
}
