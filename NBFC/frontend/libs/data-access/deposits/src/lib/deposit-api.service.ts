import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { AUTH_API_BASE_URL } from '@patsanstha/auth';
import {
  CreateDepositAccountRequest,
  DepositAccountDetail,
  ListDepositsParams,
  PagedDepositsResponse,
  UpdateDepositAccountRequest,
} from './deposit.models';

@Injectable({ providedIn: 'root' })
export class DepositApiService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(AUTH_API_BASE_URL);

  private get depositsUrl(): string {
    return `${this.apiBaseUrl}/deposits`;
  }

  list(params: ListDepositsParams = {}): Promise<PagedDepositsResponse> {
    let httpParams = new HttpParams()
      .set('page', String(params.page ?? 1))
      .set('pageSize', String(params.pageSize ?? 20));

    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }

    if (params.productType !== undefined) {
      httpParams = httpParams.set('productType', String(params.productType));
    }

    if (params.status !== undefined) {
      httpParams = httpParams.set('status', String(params.status));
    }

    if (params.branchId) {
      httpParams = httpParams.set('branchId', params.branchId);
    }

    if (params.memberId) {
      httpParams = httpParams.set('memberId', params.memberId);
    }

    return firstValueFrom(
      this.http.get<PagedDepositsResponse>(this.depositsUrl, { params: httpParams })
    );
  }

  getById(id: string): Promise<DepositAccountDetail> {
    return firstValueFrom(this.http.get<DepositAccountDetail>(`${this.depositsUrl}/${id}`));
  }

  create(request: CreateDepositAccountRequest): Promise<DepositAccountDetail> {
    return firstValueFrom(
      this.http.post<DepositAccountDetail>(this.depositsUrl, request, {
        headers: {
          'Idempotency-Key': crypto.randomUUID(),
        },
      })
    );
  }

  update(id: string, request: UpdateDepositAccountRequest): Promise<DepositAccountDetail> {
    return firstValueFrom(
      this.http.put<DepositAccountDetail>(`${this.depositsUrl}/${id}`, request)
    );
  }
}
