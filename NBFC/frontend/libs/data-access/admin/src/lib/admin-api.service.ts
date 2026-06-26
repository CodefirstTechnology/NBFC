import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { AUTH_API_BASE_URL } from '@patsanstha/auth';
import {
  AssignRolesRequest,
  CreateUserRequest,
  ListUsersParams,
  PagedUsersResponse,
  RoleSummary,
  UpdateUserRequest,
  UserSummary,
} from './admin.models';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(AUTH_API_BASE_URL);

  private get adminUrl(): string {
    return `${this.apiBaseUrl}/admin`;
  }

  listUsers(params: ListUsersParams = {}): Promise<PagedUsersResponse> {
    let httpParams = new HttpParams()
      .set('page', String(params.page ?? 1))
      .set('pageSize', String(params.pageSize ?? 20));

    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.isActive !== undefined) httpParams = httpParams.set('isActive', String(params.isActive));

    return firstValueFrom(this.http.get<PagedUsersResponse>(`${this.adminUrl}/users`, { params: httpParams }));
  }

  createUser(request: CreateUserRequest): Promise<UserSummary> {
    return firstValueFrom(this.http.post<UserSummary>(`${this.adminUrl}/users`, request));
  }

  updateUser(userId: string, request: UpdateUserRequest): Promise<UserSummary> {
    return firstValueFrom(this.http.put<UserSummary>(`${this.adminUrl}/users/${userId}`, request));
  }

  assignRoles(userId: string, request: AssignRolesRequest): Promise<UserSummary> {
    return firstValueFrom(this.http.put<UserSummary>(`${this.adminUrl}/users/${userId}/roles`, request));
  }

  getRoles(): Promise<RoleSummary[]> {
    return firstValueFrom(this.http.get<RoleSummary[]>(`${this.adminUrl}/roles`));
  }

  getPermissions(): Promise<string[]> {
    return firstValueFrom(this.http.get<string[]>(`${this.adminUrl}/permissions`));
  }
}
