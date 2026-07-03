import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { AUTH_API_BASE_URL } from '@patsanstha/auth';
import {
  ExecutiveDashboard,
  GenerateReportRequest,
  ListReportSnapshotsParams,
  PagedReportSnapshotsResponse,
  ReportSnapshotDetail,
} from './reports.models';

@Injectable({ providedIn: 'root' })
export class ReportsApiService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(AUTH_API_BASE_URL);

  private get reportsUrl(): string {
    return `${this.apiBaseUrl}/reports`;
  }

  private get dashboardUrl(): string {
    return `${this.apiBaseUrl}/dashboard`;
  }

  getExecutiveDashboard(branchId?: string): Promise<ExecutiveDashboard> {
    let params = new HttpParams();
    if (branchId) {
      params = params.set('branchId', branchId);
    }

    return firstValueFrom(this.http.get<ExecutiveDashboard>(`${this.dashboardUrl}/executive`, { params }));
  }

  list(params: ListReportSnapshotsParams = {}): Promise<PagedReportSnapshotsResponse> {
    let httpParams = new HttpParams()
      .set('page', String(params.page ?? 1))
      .set('pageSize', String(params.pageSize ?? 20));

    if (params.reportType !== undefined) httpParams = httpParams.set('reportType', String(params.reportType));
    if (params.status !== undefined) httpParams = httpParams.set('status', String(params.status));

    return firstValueFrom(this.http.get<PagedReportSnapshotsResponse>(this.reportsUrl, { params: httpParams }));
  }

  getById(id: string): Promise<ReportSnapshotDetail> {
    return firstValueFrom(this.http.get<ReportSnapshotDetail>(`${this.reportsUrl}/${id}`));
  }

  generate(request: GenerateReportRequest): Promise<ReportSnapshotDetail> {
    return firstValueFrom(
      this.http.post<ReportSnapshotDetail>(`${this.reportsUrl}/generate`, {
        reportType: request.reportType,
        title: request.title,
        parametersJson: request.parametersJson ?? '{}',
      })
    );
  }
}
