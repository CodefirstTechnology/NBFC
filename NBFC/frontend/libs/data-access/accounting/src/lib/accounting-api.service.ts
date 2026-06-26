import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { AUTH_API_BASE_URL } from '@patsanstha/auth';
import {
  CreateJournalEntryRequest,
  JournalEntryDetail,
  ListJournalEntriesParams,
  PagedJournalEntriesResponse,
} from './accounting.models';

@Injectable({ providedIn: 'root' })
export class AccountingApiService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(AUTH_API_BASE_URL);

  private get accountingUrl(): string {
    return `${this.apiBaseUrl}/accounting`;
  }

  list(params: ListJournalEntriesParams = {}): Promise<PagedJournalEntriesResponse> {
    let httpParams = new HttpParams()
      .set('page', String(params.page ?? 1))
      .set('pageSize', String(params.pageSize ?? 20));

    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.status !== undefined) httpParams = httpParams.set('status', String(params.status));
    if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
    if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);

    return firstValueFrom(this.http.get<PagedJournalEntriesResponse>(this.accountingUrl, { params: httpParams }));
  }

  getById(id: string): Promise<JournalEntryDetail> {
    return firstValueFrom(this.http.get<JournalEntryDetail>(`${this.accountingUrl}/${id}`));
  }

  create(request: CreateJournalEntryRequest): Promise<JournalEntryDetail> {
    return firstValueFrom(
      this.http.post<JournalEntryDetail>(this.accountingUrl, request, {
        headers: { 'Idempotency-Key': crypto.randomUUID() },
      })
    );
  }

  post(id: string): Promise<JournalEntryDetail> {
    return firstValueFrom(this.http.put<JournalEntryDetail>(`${this.accountingUrl}/${id}/post`, {}));
  }
}
