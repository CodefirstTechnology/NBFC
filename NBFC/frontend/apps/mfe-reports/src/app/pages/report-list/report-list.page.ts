import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '@patsanstha/auth';
import {
  ReportSnapshotSummary,
  ReportType,
  ReportsApiService,
  extractApiErrorMessage,
  reportStatusLabel,
  reportTypeLabel,
} from '@patsanstha/reports-data-access';
import { PatsButtonComponent, PatsTableComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-report-list-page',
  standalone: true,
  imports: [FormsModule, HasPermissionDirective, PatsButtonComponent, PatsTableComponent],
  template: `
    <section class="page">
      <header class="page__header">
        <div>
          <h1>Reports <span class="page__subtitle">/ अहवाल</span></h1>
          <p>View generated report snapshots.</p>
        </div>
        <pats-button
          *patsHasPermission="'reports.export'"
          icon="summarize"
          (clicked)="generateReport()">
          Generate Report
        </pats-button>
      </header>

      <div class="page__toolbar">
        <select
          class="page__filter"
          [ngModel]="typeFilter()"
          (ngModelChange)="onTypeFilterChange($event)">
          <option [ngValue]="null">All Types</option>
          <option [ngValue]="0">Branch Summary</option>
          <option [ngValue]="1">Loan Portfolio</option>
          <option [ngValue]="2">Collections Daily</option>
          <option [ngValue]="3">NPA Summary</option>
        </select>
      </div>

      @if (errorMessage()) {
        <p class="page__error">{{ errorMessage() }}</p>
      }

      <pats-table
        [columns]="columns"
        [rows]="tableRows()"
        [loading]="loading()"
        [rowClickable]="true"
        emptyMessage="No reports found."
        (rowClicked)="openReport($event)" />

      <footer class="page__pagination">
        <span>{{ totalCount() }} reports</span>
        <div class="page__pagination-actions">
          <pats-button variant="ghost" size="sm" [disabled]="page() <= 1 || loading()" (clicked)="goToPage(page() - 1)">Previous</pats-button>
          <span>Page {{ page() }}</span>
          <pats-button variant="ghost" size="sm" [disabled]="!hasNextPage() || loading()" (clicked)="goToPage(page() + 1)">Next</pats-button>
        </div>
      </footer>
    </section>
  `,
  styles: [
    `
      .page { display: flex; flex-direction: column; gap: 24px; }
      .page__header { display: flex; align-items: flex-start; justify-content: space-between; gap: 16px; }
      .page__header h1 { margin: 0; font-family: var(--pats-font-display); font-size: 32px; color: var(--pats-color-primary); }
      .page__subtitle { font-size: 18px; color: var(--pats-color-text-secondary); font-weight: 500; }
      .page__header p { margin: 8px 0 0; color: var(--pats-color-text-secondary); }
      .page__toolbar { display: flex; flex-wrap: wrap; gap: 12px; }
      .page__filter { min-height: 44px; padding: 0 16px; border: 1px solid var(--pats-color-border-subtle); border-radius: var(--pats-radius-md); background: var(--pats-color-surface-container-lowest); font-size: 14px; }
      .page__error { color: var(--pats-color-error); }
      .page__pagination { display: flex; align-items: center; justify-content: space-between; color: var(--pats-color-text-secondary); font-size: 14px; }
      .page__pagination-actions { display: flex; align-items: center; gap: 12px; }
    `,
  ],
})
export class ReportListPageComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly reportsApi = inject(ReportsApiService);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly reports = signal<ReportSnapshotSummary[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly typeFilter = signal<ReportType | null>(null);

  readonly columns = [
    { key: 'title', header: 'Title' },
    { key: 'typeLabel', header: 'Type' },
    { key: 'statusLabel', header: 'Status' },
    { key: 'generatedAt', header: 'Generated' },
  ];

  readonly tableRows = computed(() =>
    this.reports().map((report) => ({
      ...report,
      typeLabel: reportTypeLabel(report.reportType),
      statusLabel: reportStatusLabel(report.status),
    }))
  );

  readonly hasNextPage = computed(() => this.page() * this.pageSize() < this.totalCount());

  ngOnInit(): void {
    void this.loadReports();
  }

  onTypeFilterChange(value: ReportType | null): void {
    this.typeFilter.set(value);
    this.page.set(1);
    void this.loadReports();
  }

  goToPage(nextPage: number): void {
    this.page.set(nextPage);
    void this.loadReports();
  }

  openReport(row: ReportSnapshotSummary): void {
    void this.router.navigate(['/reports', row.id]);
  }

  generateReport(): void {
    void this.router.navigate(['/reports/generate']);
  }

  private async loadReports(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);
    try {
      const response = await this.reportsApi.list({
        page: this.page(),
        pageSize: this.pageSize(),
        reportType: this.typeFilter() ?? undefined,
      });
      this.reports.set(response.items);
      this.totalCount.set(response.totalCount);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to load reports.'));
    } finally {
      this.loading.set(false);
    }
  }
}
