export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  userName?: string;
}

export interface LoginRequest {
  emailOrUserName: string;
  password: string;
}

export interface RefreshRequest {
  refreshToken: string;
}
