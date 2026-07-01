export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phoneNumber: string;
  userType: 'Customer' | 'Worker';
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface UserInfo {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
  roles: string[];
}

export interface AuthResponse {
  success: boolean;
  message: string | null;
  errors: string[] | null;
  accessToken: string | null;
  refreshToken: string | null;
  expiresAt: string;
  user: UserInfo | null;
}

export interface ApiError {
  success: false;
  message: string;
  errors?: string[];
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}
