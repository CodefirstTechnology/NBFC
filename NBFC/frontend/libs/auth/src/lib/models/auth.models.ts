export interface AuthUser {
  userId: string;
  email: string;
  fullName: string;
  tenantId: string;
  branchId: string | null;
  roles: string[];
  permissions: string[];
}

export interface TokenPair {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
}

export interface AuthLoginResponse {
  tokens?: TokenPair;
  requiresTwoFactor?: boolean;
  twoFactorChallengeToken?: string;
  twoFactorChallengeExpiresAt?: string;
}

export interface ApiProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  type?: string;
}
