import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { AUTH_API_BASE_URL } from '@patsanstha/auth';
import {
  CreateLoanApplicationRequest,
  ListLoansParams,
  LoanApplicationDetail,
  LoanProductApiDto,
  PagedLoansResponse,
} from './loan.models';

@Injectable({ providedIn: 'root' })
export class LoanApiService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(AUTH_API_BASE_URL);

  private get loansUrl(): string {
    return `${this.apiBaseUrl}/loans`;
  }

  list(params: ListLoansParams = {}): Promise<PagedLoansResponse> {
    let httpParams = new HttpParams()
      .set('page', String(params.page ?? 1))
      .set('pageSize', String(params.pageSize ?? 20));

    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.productType !== undefined) httpParams = httpParams.set('productType', String(params.productType));
    if (params.status !== undefined) httpParams = httpParams.set('status', String(params.status));
    if (params.branchId) httpParams = httpParams.set('branchId', params.branchId);
    if (params.memberId) httpParams = httpParams.set('memberId', params.memberId);

    return firstValueFrom(this.http.get<PagedLoansResponse>(this.loansUrl, { params: httpParams }));
  }

  getById(id: string): Promise<LoanApplicationDetail> {
    return firstValueFrom(this.http.get<LoanApplicationDetail>(`${this.loansUrl}/${id}`));
  }

  getProducts(): Promise<LoanProductApiDto[]> {
    return firstValueFrom(this.http.get<LoanProductApiDto[]>(`${this.loansUrl}/products`));
  }

  create(request: CreateLoanApplicationRequest): Promise<LoanApplicationDetail> {
    return firstValueFrom(
      this.http.post<LoanApplicationDetail>(this.loansUrl, request, {
        headers: { 'Idempotency-Key': crypto.randomUUID() },
      })
    );
  }

  approve(id: string, approvedAmount: number): Promise<LoanApplicationDetail> {
    return firstValueFrom(
      this.http.put<LoanApplicationDetail>(`${this.loansUrl}/${id}/approve`, { approvedAmount })
    );
  }

  reject(id: string, reason: string): Promise<LoanApplicationDetail> {
    return firstValueFrom(
      this.http.put<LoanApplicationDetail>(`${this.loansUrl}/${id}/reject`, { reason })
    );
  }

  disburse(id: string): Promise<LoanApplicationDetail> {
    return firstValueFrom(this.http.put<LoanApplicationDetail>(`${this.loansUrl}/${id}/disburse`, {}));
  }
}
