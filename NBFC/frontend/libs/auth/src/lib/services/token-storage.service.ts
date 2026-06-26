import { Injectable } from '@angular/core';
import { TokenPair, AuthUser } from '../models/auth.models';

const ACCESS_TOKEN_KEY = 'patsanstha.accessToken';
const REFRESH_TOKEN_KEY = 'patsanstha.refreshToken';
const ACCESS_EXPIRES_KEY = 'patsanstha.accessExpiresAt';
const USER_KEY = 'patsanstha.user';

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  getAccessToken(): string | null {
    return sessionStorage.getItem(ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  getAccessExpiresAt(): string | null {
    return sessionStorage.getItem(ACCESS_EXPIRES_KEY);
  }

  getUser(): AuthUser | null {
    const raw = sessionStorage.getItem(USER_KEY);
    return raw ? (JSON.parse(raw) as AuthUser) : null;
  }

  storeTokens(tokens: TokenPair, user?: AuthUser | null): void {
    sessionStorage.setItem(ACCESS_TOKEN_KEY, tokens.accessToken);
    sessionStorage.setItem(ACCESS_EXPIRES_KEY, tokens.accessTokenExpiresAt);
    localStorage.setItem(REFRESH_TOKEN_KEY, tokens.refreshToken);

    if (user) {
      sessionStorage.setItem(USER_KEY, JSON.stringify(user));
    }
  }

  storeUser(user: AuthUser): void {
    sessionStorage.setItem(USER_KEY, JSON.stringify(user));
  }

  clear(): void {
    sessionStorage.removeItem(ACCESS_TOKEN_KEY);
    sessionStorage.removeItem(ACCESS_EXPIRES_KEY);
    sessionStorage.removeItem(USER_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
  }

  isAccessTokenExpired(): boolean {
    const expiresAt = this.getAccessExpiresAt();
    if (!expiresAt) {
      return true;
    }

    return new Date(expiresAt).getTime() <= Date.now() + 30_000;
  }
}
