import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { AUTH_API_BASE_URL } from '@patsanstha/auth';
import {
  CollectionReceiptDetail,
  CreateCollectionReceiptRequest,
  ListCollectionsParams,
  PagedCollectionsResponse,
} from './collection.models';

@Injectable({ providedIn: 'root' })
export class CollectionApiService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(AUTH_API_BASE_URL);

  private get collectionsUrl(): string {
    return `${this.apiBaseUrl}/collections`;
  }

  list(params: ListCollectionsParams = {}): Promise<PagedCollectionsResponse> {
    let httpParams = new HttpParams()
      .set('page', String(params.page ?? 1))
      .set('pageSize', String(params.pageSize ?? 20));

    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.branchId) httpParams = httpParams.set('branchId', params.branchId);
    if (params.memberId) httpParams = httpParams.set('memberId', params.memberId);
    if (params.loanNumber) httpParams = httpParams.set('loanNumber', params.loanNumber);

    return firstValueFrom(
      this.http.get<PagedCollectionsResponse>(this.collectionsUrl, { params: httpParams })
    );
  }

  getById(id: string): Promise<CollectionReceiptDetail> {
    return firstValueFrom(this.http.get<CollectionReceiptDetail>(`${this.collectionsUrl}/${id}`));
  }

  create(request: CreateCollectionReceiptRequest): Promise<CollectionReceiptDetail> {
    return firstValueFrom(
      this.http.post<CollectionReceiptDetail>(this.collectionsUrl, request, {
        headers: { 'Idempotency-Key': crypto.randomUUID() },
      })
    );
  }

  reverse(id: string): Promise<CollectionReceiptDetail> {
    return firstValueFrom(
      this.http.put<CollectionReceiptDetail>(`${this.collectionsUrl}/${id}/reverse`, {})
    );
  }
}
