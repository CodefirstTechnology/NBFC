import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { AUTH_API_BASE_URL } from '@patsanstha/auth';
import {
  CreateMemberRequest,
  ListMembersParams,
  MemberDetail,
  PagedMembersResponse,
} from './member.models';

/**
 * Typed Members API client.
 * Replace with OpenAPI-generated client in CI when backend spec is aggregated.
 */
@Injectable({ providedIn: 'root' })
export class MemberApiService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(AUTH_API_BASE_URL);

  private get membersUrl(): string {
    return `${this.apiBaseUrl}/members`;
  }

  list(params: ListMembersParams = {}): Promise<PagedMembersResponse> {
    let httpParams = new HttpParams()
      .set('page', String(params.page ?? 1))
      .set('pageSize', String(params.pageSize ?? 20));

    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }

    if (params.status !== undefined) {
      httpParams = httpParams.set('status', String(params.status));
    }

    if (params.branchId) {
      httpParams = httpParams.set('branchId', params.branchId);
    }

    return firstValueFrom(
      this.http.get<PagedMembersResponse>(this.membersUrl, { params: httpParams })
    );
  }

  getById(id: string): Promise<MemberDetail> {
    return firstValueFrom(this.http.get<MemberDetail>(`${this.membersUrl}/${id}`));
  }

  create(request: CreateMemberRequest): Promise<MemberDetail> {
    return firstValueFrom(
      this.http.post<MemberDetail>(this.membersUrl, request, {
        headers: {
          'Idempotency-Key': crypto.randomUUID(),
        },
      })
    );
  }
}

export function extractApiErrorMessage(error: unknown, fallback: string): string {
  if (typeof error === 'object' && error !== null && 'error' in error) {
    const problem = (error as { error?: { detail?: string; title?: string } }).error;
    return problem?.detail ?? problem?.title ?? fallback;
  }

  return fallback;
}

export function memberStatusLabel(status: number): string {
  return ['Pending', 'Active', 'Inactive', 'Suspended', 'Closed'][status] ?? 'Unknown';
}

export function memberStatusVariant(
  status: number
): 'active' | 'inactive' | 'pending' | 'warning' | 'error' {
  switch (status) {
    case 1:
      return 'active';
    case 0:
      return 'pending';
    case 2:
      return 'inactive';
    case 3:
      return 'warning';
    case 4:
      return 'error';
    default:
      return 'inactive';
  }
}
