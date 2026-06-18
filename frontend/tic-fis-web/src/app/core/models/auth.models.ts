export interface LoginRequest {
  email: string;
  password: string;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface DecodedToken {
  sub: string;
  email: string;
  roles: string | string[];
  exp: number;
}
