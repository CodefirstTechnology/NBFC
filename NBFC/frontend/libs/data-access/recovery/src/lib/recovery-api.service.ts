import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { AUTH_API_BASE_URL } from '@patsanstha/auth';
import {
  CreateRecoveryCaseRequest,
  ListRecoveryCasesParams,
  PagedRecoveryCasesResponse,
  RecoveryCaseDetail,
  UpdateRecoveryCaseRequest,
} from './recovery.models';

@Injectable({ providedIn: 'root' })
export class RecoveryApiService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(AUTH_API_BASE_URL);

  private get recoveryUrl(): string {
    return `${this.apiBaseUrl}/recovery`;
  }

  list(params: ListRecoveryCasesParams = {}): Promise<PagedRecoveryCasesResponse> {
    let httpParams = new HttpParams()
      .set('page', String(params.page ?? 1))
      .set('pageSize', String(params.pageSize ?? 20));

    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.status !== undefined) httpParams = httpParams.set('status', String(params.status));
    if (params.branchId) httpParams = httpParams.set('branchId', params.branchId);
    if (params.memberId) httpParams = httpParams.set('memberId', params.memberId);
    if (params.assignedToUserId) {
      httpParams = httpParams.set('assignedToUserId', params.assignedToUserId);
    }

    return firstValueFrom(
      this.http.get<PagedRecoveryCasesResponse>(this.recoveryUrl, { params: httpParams })
    );
  }

  getById(id: string): Promise<RecoveryCaseDetail> {
    return firstValueFrom(this.http.get<RecoveryCaseDetail>(`${this.recoveryUrl}/${id}`));
  }

  create(request: CreateRecoveryCaseRequest): Promise<RecoveryCaseDetail> {
    return firstValueFrom(this.http.post<RecoveryCaseDetail>(this.recoveryUrl, request));
  }

  update(id: string, request: UpdateRecoveryCaseRequest): Promise<RecoveryCaseDetail> {
    return firstValueFrom(
      this.http.put<RecoveryCaseDetail>(`${this.recoveryUrl}/${id}`, request)
    );
  }
}
