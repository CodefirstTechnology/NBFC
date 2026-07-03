import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import {
  AuthLoginResponse,
  AuthUser,
  TokenPair,
  ApiProblemDetails,
} from '../models/auth.models';
import { TokenStorageService } from './token-storage.service';
import { AUTH_API_BASE_URL } from '../tokens';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly apiBaseUrl = inject(AUTH_API_BASE_URL);

  private refreshInFlight: Promise<boolean> | null = null;

  private readonly userSignal = signal<AuthUser | null>(this.tokenStorage.getUser());

  readonly user = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.tokenStorage.getAccessToken());
  readonly permissions = computed(() => this.userSignal()?.permissions ?? []);

  async login(email: string, password: string): Promise<AuthLoginResponse> {
    const response = await firstValueFrom(
      this.http.post<AuthLoginResponse>(`${this.apiBaseUrl}/auth/login`, { email, password })
    );

    if (response.tokens) {
      await this.completeLogin(response.tokens);
    }

    return response;
  }

  async completeLogin(tokens: TokenPair): Promise<AuthUser> {
    this.tokenStorage.storeTokens(tokens);
    const user = await this.fetchCurrentUser();
    this.userSignal.set(user);
    return user;
  }

  async refreshSession(): Promise<boolean> {
    if (this.refreshInFlight) {
      return this.refreshInFlight;
    }

    this.refreshInFlight = this.refreshSessionInternal();
    try {
      return await this.refreshInFlight;
    } finally {
      this.refreshInFlight = null;
    }
  }

  private async refreshSessionInternal(): Promise<boolean> {
    const refreshToken = this.tokenStorage.getRefreshToken();
    if (!refreshToken) {
      return false;
    }

    try {
      const response = await firstValueFrom(
        this.http.post<TokenPair>(`${this.apiBaseUrl}/auth/refresh`, { refreshToken })
      );
      await this.completeLogin(response);
      return true;
    } catch {
      this.logout();
      return false;
    }
  }

  async fetchCurrentUser(): Promise<AuthUser> {
    const user = await firstValueFrom(
      this.http.get<AuthUser>(`${this.apiBaseUrl}/auth/me`)
    );
    this.tokenStorage.storeUser(user);
    this.userSignal.set(user);
    return user;
  }

  hasPermission(permission: string): boolean {
    return this.permissions().includes(permission);
  }

  hasAnyPermission(required: string[]): boolean {
    const granted = this.permissions();
    return required.some((permission) => granted.includes(permission));
  }

  async logout(): Promise<void> {
    const refreshToken = this.tokenStorage.getRefreshToken();

    if (refreshToken) {
      try {
        await firstValueFrom(
          this.http.post(`${this.apiBaseUrl}/auth/logout`, { refreshToken })
        );
      } catch {
        // Best-effort server logout; local session is always cleared.
      }
    }

    this.tokenStorage.clear();
    this.userSignal.set(null);
  }

  extractProblemMessage(error: unknown, fallback = 'Request failed.'): string {
    if (typeof error === 'object' && error !== null && 'error' in error) {
      const problem = (error as { error?: ApiProblemDetails }).error;
      return problem?.detail ?? problem?.title ?? fallback;
    }

    return fallback;
  }
}
