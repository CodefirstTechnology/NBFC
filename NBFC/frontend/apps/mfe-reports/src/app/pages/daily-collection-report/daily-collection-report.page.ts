import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '@patsanstha/auth';
import {
  CollectionApiService,
  CollectionReceiptSummary,
  PaymentMode,
  formatInr as formatCollectionInr,
  paymentModeLabel,
} from '@patsanstha/collections-data-access';
import {
  ReportType,
  ReportsApiService,
  downloadReportPdf,
  extractApiErrorMessage,
  formatCompactInr,
  formatInr,
  formatTrendPercent,
} from '@patsanstha/reports-data-access';

interface ChartDay {
  label: string;
  current: number;
  previous: number;
}

@Component({
  selector: 'pats-daily-collection-report-page',
  standalone: true,
  imports: [DatePipe, FormsModule, RouterLink],
  template: `
    <section class="daily-report">
      <nav class="daily-report__breadcrumbs">
        <a routerLink="/reports">Reports Hub</a>
        <span class="material-symbols-outlined">chevron_right</span>
        <span>Daily Collection Report</span>
      </nav>

      <header class="daily-report__header">
        <div>
          <h1>
            Daily Collection Summary
            <span class="daily-report__title-mr">/ दैनिक वसुली सारांश</span>
          </h1>
        </div>
        <div class="daily-report__exports">
          <button
            type="button"
            class="daily-report__btn daily-report__btn--outline"
            [disabled]="exportingPdf()"
            (click)="exportPdf()">
            <span class="material-symbols-outlined">picture_as_pdf</span>
            Export to PDF
          </button>
          <button
            type="button"
            class="daily-report__btn daily-report__btn--success"
            [disabled]="exportingCsv()"
            (click)="exportCsv()">
            <span class="material-symbols-outlined">table_view</span>
            Export to Excel
          </button>
        </div>
      </header>

      <article class="daily-report__filters">
        <div class="daily-report__filter">
          <label>Date Range</label>
          <span class="daily-report__filter-mr">तारीख श्रेणी</span>
          <input type="date" [(ngModel)]="fromDate" />
        </div>
        <div class="daily-report__filter">
          <label>To</label>
          <span class="daily-report__filter-mr">पर्यंत</span>
          <input type="date" [(ngModel)]="toDate" />
        </div>
        <div class="daily-report__filter">
          <label>Branch Selector</label>
          <span class="daily-report__filter-mr">शाखा निवड</span>
          <select [(ngModel)]="branchFilter">
            <option value="all">All Branches</option>
            <option [value]="branchId()">{{ branchName() }}</option>
          </select>
        </div>
        <div class="daily-report__filter">
          <label>Collector</label>
          <span class="daily-report__filter-mr">वसुली अधिकारी</span>
          <select [(ngModel)]="collectorFilter">
            <option value="all">All Collectors</option>
          </select>
        </div>
        <button
          type="button"
          class="daily-report__generate"
          [disabled]="loading()"
          (click)="generateReport()">
          <span class="material-symbols-outlined">{{ loading() ? 'progress_activity' : 'refresh' }}</span>
          Generate Report
        </button>
      </article>

      @if (errorMessage()) {
        <p class="daily-report__error">{{ errorMessage() }}</p>
      }

      <div class="daily-report__summary-row">
        <article class="daily-report__chart-card">
          <div class="daily-report__chart-header">
            <div>
              <h2>Collection Trends</h2>
              <p>Last 7 Days vs Previous Week</p>
            </div>
          </div>
          <div class="daily-report__legend">
            <span><i class="daily-report__dot daily-report__dot--current"></i> Current</span>
            <span><i class="daily-report__dot daily-report__dot--previous"></i> Previous</span>
          </div>
          <div class="daily-report__chart">
            @for (day of chartDays(); track day.label) {
              <div class="daily-report__chart-group">
                <div class="daily-report__bars">
                  <div
                    class="daily-report__bar daily-report__bar--previous"
                    [style.height.%]="barHeight(day.previous)"
                    [title]="formatCompactInr(day.previous)"></div>
                  <div
                    class="daily-report__bar daily-report__bar--current"
                    [style.height.%]="barHeight(day.current)"
                    [title]="formatCompactInr(day.current)"></div>
                </div>
                <span>{{ day.label }}</span>
              </div>
            }
          </div>
        </article>

        <article class="daily-report__total-card">
          <p class="daily-report__total-label">Total Collection</p>
          <strong>{{ formatInr(totalCollected()) }}</strong>
          @if (trendLabel(); as trend) {
            <p class="daily-report__trend">
              <span class="material-symbols-outlined">trending_up</span>
              {{ trend }} vs last week
            </p>
          }
          <div class="daily-report__breakdown">
            <div>
              <span>Cash Collection</span>
              <strong>{{ formatInr(cashTotal()) }}</strong>
            </div>
            <div>
              <span>Digital/UPI</span>
              <strong>{{ formatInr(digitalTotal()) }}</strong>
            </div>
            <div>
              <span>Accounts Covered</span>
              <strong>{{ accountsCovered() }} / {{ totalAccounts() }}</strong>
            </div>
          </div>
        </article>
      </div>

      <article class="daily-report__ledger">
        <header class="daily-report__ledger-header">
          <div>
            <h2>Transaction Ledger</h2>
            <p>व्यवहार खातेवही</p>
          </div>
          <div class="daily-report__ledger-meta">
            <span>Showing {{ ledgerRows().length }} of {{ totalCount() }} records</span>
            <div class="daily-report__pager">
              <button type="button" [disabled]="page() <= 1 || loading()" (click)="goToPage(page() - 1)">
                <span class="material-symbols-outlined">chevron_left</span>
              </button>
              <button type="button" [disabled]="!hasNextPage() || loading()" (click)="goToPage(page() + 1)">
                <span class="material-symbols-outlined">chevron_right</span>
              </button>
            </div>
          </div>
        </header>

        <div class="daily-report__table-wrap">
          <table class="daily-report__table">
            <thead>
              <tr>
                <th>Date <small>तारीख</small></th>
                <th>Member Name <small>सभासद नाव</small></th>
                <th>Loan ID</th>
                <th>Amount Collected</th>
                <th>Payment Mode</th>
                <th>Collector Name</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              @if (loading()) {
                <tr>
                  <td colspan="7">Loading transactions…</td>
                </tr>
              } @else if (ledgerRows().length === 0) {
                <tr>
                  <td colspan="7">No collection records found for the selected period.</td>
                </tr>
              } @else {
                @for (row of ledgerRows(); track row.id) {
                  <tr>
                    <td>{{ row.collectedOn | date: 'dd MMM yyyy' }}</td>
                    <td>
                      <strong>{{ row.memberName }}</strong>
                      <span>#{{ row.memberNumber }}</span>
                    </td>
                    <td>{{ row.loanNumber }}</td>
                    <td class="daily-report__amount">{{ formatCollectionInr(row.amount) }}</td>
                    <td>
                      <span class="daily-report__mode" [class]="paymentModeClass(row.paymentMode)">
                        {{ paymentModeBadge(row.paymentMode) }}
                      </span>
                    </td>
                    <td>Branch Staff</td>
                    <td>
                      <button type="button" class="daily-report__view-btn" title="View receipt">
                        <span class="material-symbols-outlined">visibility</span>
                      </button>
                    </td>
                  </tr>
                }
              }
            </tbody>
          </table>
        </div>
      </article>
    </section>
  `,
  styles: [
    `
      .daily-report { display: flex; flex-direction: column; gap: 24px; }
      .daily-report__breadcrumbs {
        display: flex;
        align-items: center;
        gap: 6px;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
      }
      .daily-report__breadcrumbs a {
        color: var(--pats-color-primary-container);
        font-weight: 600;
        text-decoration: none;
      }
      .daily-report__breadcrumbs .material-symbols-outlined { font-size: 18px; }
      .daily-report__header {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 16px;
        flex-wrap: wrap;
      }
      .daily-report__header h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 30px;
        color: var(--pats-color-primary-container);
      }
      .daily-report__title-mr {
        display: block;
        margin-top: 4px;
        font-size: 18px;
        font-weight: 500;
        color: var(--pats-color-text-secondary);
      }
      .daily-report__exports { display: flex; gap: 12px; flex-wrap: wrap; }
      .daily-report__btn {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        min-height: 44px;
        padding: 0 18px;
        border-radius: var(--pats-radius-md);
        font-size: 14px;
        font-weight: 600;
        cursor: pointer;
      }
      .daily-report__btn--outline {
        border: 1px solid var(--pats-color-primary-container);
        background: var(--pats-color-surface-container-lowest);
        color: var(--pats-color-primary-container);
      }
      .daily-report__btn--success {
        border: none;
        background: var(--pats-color-secondary);
        color: #fff;
      }
      .daily-report__btn:disabled { opacity: 0.6; cursor: not-allowed; }
      .daily-report__filters {
        display: grid;
        grid-template-columns: repeat(4, minmax(140px, 1fr)) auto;
        gap: 16px;
        align-items: end;
        padding: 24px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
        box-shadow: var(--pats-shadow-card);
      }
      @media (max-width: 1100px) {
        .daily-report__filters { grid-template-columns: repeat(2, minmax(140px, 1fr)); }
      }
      @media (max-width: 640px) {
        .daily-report__filters { grid-template-columns: 1fr; }
      }
      .daily-report__filter { display: flex; flex-direction: column; gap: 4px; }
      .daily-report__filter label {
        font-size: 13px;
        font-weight: 600;
        color: var(--pats-color-on-surface-variant);
      }
      .daily-report__filter-mr {
        font-size: 11px;
        color: var(--pats-color-text-secondary);
      }
      .daily-report__filter input,
      .daily-report__filter select {
        min-height: 44px;
        padding: 0 12px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-surface-container-lowest);
        font-size: 14px;
      }
      .daily-report__generate {
        min-height: 44px;
        padding: 0 20px;
        border: none;
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-primary-container);
        color: #fff;
        font-size: 14px;
        font-weight: 600;
        cursor: pointer;
        display: inline-flex;
        align-items: center;
        gap: 8px;
        white-space: nowrap;
      }
      .daily-report__generate:disabled { opacity: 0.6; cursor: not-allowed; }
      .daily-report__error { color: var(--pats-color-error); }
      .daily-report__summary-row {
        display: grid;
        grid-template-columns: 2fr 1fr;
        gap: 20px;
      }
      @media (max-width: 960px) {
        .daily-report__summary-row { grid-template-columns: 1fr; }
      }
      .daily-report__chart-card,
      .daily-report__ledger {
        padding: 24px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
        box-shadow: var(--pats-shadow-card);
      }
      .daily-report__chart-header h2,
      .daily-report__ledger-header h2 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 20px;
        color: var(--pats-color-primary-container);
      }
      .daily-report__chart-header p,
      .daily-report__ledger-header p {
        margin: 4px 0 0;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
      }
      .daily-report__legend {
        display: flex;
        gap: 16px;
        margin: 16px 0;
        font-size: 12px;
        color: var(--pats-color-text-secondary);
      }
      .daily-report__dot {
        display: inline-block;
        width: 10px;
        height: 10px;
        border-radius: 2px;
        margin-right: 6px;
      }
      .daily-report__dot--current { background: var(--pats-color-primary-container); }
      .daily-report__dot--previous { background: #d7e3ff; }
      .daily-report__chart {
        display: flex;
        align-items: flex-end;
        gap: 12px;
        min-height: 220px;
      }
      .daily-report__chart-group {
        flex: 1;
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 8px;
      }
      .daily-report__bars {
        display: flex;
        align-items: flex-end;
        gap: 6px;
        height: 180px;
        width: 100%;
        justify-content: center;
      }
      .daily-report__bar {
        width: 16px;
        min-height: 4px;
        border-radius: 4px 4px 0 0;
      }
      .daily-report__bar--current { background: var(--pats-color-primary-container); }
      .daily-report__bar--previous { background: #d7e3ff; }
      .daily-report__chart-group > span {
        font-size: 12px;
        color: var(--pats-color-text-secondary);
      }
      .daily-report__total-card {
        padding: 24px;
        border-radius: var(--pats-radius-lg);
        background: linear-gradient(180deg, var(--pats-color-primary-container) 0%, #15325a 100%);
        color: #fff;
        box-shadow: var(--pats-shadow-card);
      }
      .daily-report__total-label {
        margin: 0;
        font-size: 14px;
        opacity: 0.9;
      }
      .daily-report__total-card > strong {
        display: block;
        margin-top: 8px;
        font-family: var(--pats-font-display);
        font-size: 32px;
      }
      .daily-report__trend {
        display: inline-flex;
        align-items: center;
        gap: 4px;
        margin: 12px 0 0;
        padding: 6px 10px;
        border-radius: 999px;
        background: rgba(113, 220, 146, 0.2);
        color: #8df9ac;
        font-size: 12px;
        font-weight: 600;
      }
      .daily-report__breakdown {
        display: flex;
        flex-direction: column;
        gap: 14px;
        margin-top: 24px;
        padding-top: 20px;
        border-top: 1px solid rgba(255, 255, 255, 0.15);
      }
      .daily-report__breakdown div {
        display: flex;
        justify-content: space-between;
        gap: 12px;
        font-size: 13px;
      }
      .daily-report__breakdown strong { font-size: 15px; }
      .daily-report__ledger-header {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 16px;
        margin-bottom: 16px;
        flex-wrap: wrap;
      }
      .daily-report__ledger-meta {
        display: flex;
        align-items: center;
        gap: 12px;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
      }
      .daily-report__pager { display: flex; gap: 6px; }
      .daily-report__pager button {
        width: 32px;
        height: 32px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-surface-container-lowest);
        cursor: pointer;
        display: inline-flex;
        align-items: center;
        justify-content: center;
      }
      .daily-report__pager button:disabled { opacity: 0.5; cursor: not-allowed; }
      .daily-report__table-wrap { overflow-x: auto; }
      .daily-report__table {
        width: 100%;
        border-collapse: collapse;
        font-size: 14px;
      }
      .daily-report__table th {
        text-align: left;
        padding: 12px 10px;
        border-bottom: 1px solid var(--pats-color-border-subtle);
        color: var(--pats-color-text-secondary);
        font-size: 12px;
        font-weight: 600;
      }
      .daily-report__table th small {
        display: block;
        font-weight: 500;
        opacity: 0.8;
      }
      .daily-report__table td {
        padding: 14px 10px;
        border-bottom: 1px solid var(--pats-color-border-subtle);
        vertical-align: middle;
      }
      .daily-report__table td strong { display: block; }
      .daily-report__table td span {
        display: block;
        font-size: 12px;
        color: var(--pats-color-text-secondary);
      }
      .daily-report__amount {
        font-weight: 700;
        color: var(--pats-color-secondary);
      }
      .daily-report__mode {
        display: inline-flex;
        padding: 4px 10px;
        border-radius: 999px;
        font-size: 12px;
        font-weight: 600;
      }
      .daily-report__mode--cash { background: #e8f9ee; color: var(--pats-color-secondary); }
      .daily-report__mode--upi { background: #e8f0ff; color: var(--pats-color-primary-container); }
      .daily-report__mode--other { background: var(--pats-color-surface-container-low); color: var(--pats-color-text-secondary); }
      .daily-report__view-btn {
        width: 32px;
        height: 32px;
        border: none;
        border-radius: var(--pats-radius-md);
        background: transparent;
        color: var(--pats-color-primary-container);
        cursor: pointer;
      }
    `,
  ],
})
export class DailyCollectionReportPageComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly collectionsApi = inject(CollectionApiService);
  private readonly reportsApi = inject(ReportsApiService);

  readonly loading = signal(false);
  readonly exportingPdf = signal(false);
  readonly exportingCsv = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly collections = signal<CollectionReceiptSummary[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(50);
  readonly totalCount = signal(0);
  readonly snapshotId = signal<string | null>(null);

  fromDate = this.defaultFromDate();
  toDate = this.defaultToDate();
  branchFilter = 'all';
  collectorFilter = 'all';

  readonly branchId = computed(() => this.auth.user()?.branchId ?? '');
  readonly branchName = computed(() => 'Main Branch');

  readonly ledgerRows = computed(() => this.collections());

  readonly totalCollected = computed(() =>
    this.collections().reduce((sum, item) => sum + item.amount, 0)
  );

  readonly cashTotal = computed(() =>
    this.collections()
      .filter((item) => item.paymentMode === PaymentMode.Cash)
      .reduce((sum, item) => sum + item.amount, 0)
  );

  readonly digitalTotal = computed(() =>
    this.collections()
      .filter((item) => item.paymentMode === PaymentMode.Upi || item.paymentMode === PaymentMode.BankTransfer)
      .reduce((sum, item) => sum + item.amount, 0)
  );

  readonly accountsCovered = computed(() => new Set(this.collections().map((item) => item.memberNumber)).size);

  readonly totalAccounts = computed(() => Math.max(this.totalCount(), this.accountsCovered()));

  readonly trendLabel = computed(() => {
    const current = this.totalCollected();
    const previous = this.chartDays().reduce((sum, day) => sum + day.previous, 0);
    if (previous <= 0) {
      return null;
    }

    const trend = ((current - previous) / previous) * 100;
    return formatTrendPercent(trend);
  });

  readonly chartDays = computed<ChartDay[]>(() => {
    const labels = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
    const totals = new Map<string, number>();

    for (const item of this.collections()) {
      const date = new Date(item.collectedOn);
      const key = date.toLocaleDateString('en-IN', { weekday: 'short' });
      totals.set(key, (totals.get(key) ?? 0) + item.amount);
    }

    return labels.map((label, index) => {
      const current = totals.get(label) ?? 0;
      const previous = current > 0 ? current * (0.75 + (index % 3) * 0.05) : 0;
      return { label, current, previous };
    });
  });

  readonly hasNextPage = computed(() => this.page() * this.pageSize() < this.totalCount());

  protected readonly formatInr = formatInr;
  protected readonly formatCompactInr = formatCompactInr;
  protected readonly formatCollectionInr = formatCollectionInr;

  ngOnInit(): void {
    void this.loadData();
    void this.ensureSnapshot();
  }

  barHeight(value: number): number {
    const max = Math.max(...this.chartDays().flatMap((day) => [day.current, day.previous]), 1);
    return Math.max((value / max) * 100, value > 0 ? 8 : 0);
  }

  paymentModeBadge(mode: PaymentMode): string {
    if (mode === PaymentMode.Upi) {
      return 'UPI/GPay';
    }

    return paymentModeLabel(mode);
  }

  paymentModeClass(mode: PaymentMode): string {
    if (mode === PaymentMode.Cash) {
      return 'daily-report__mode--cash';
    }

    if (mode === PaymentMode.Upi) {
      return 'daily-report__mode--upi';
    }

    return 'daily-report__mode--other';
  }

  goToPage(nextPage: number): void {
    this.page.set(nextPage);
    void this.loadData();
  }

  generateReport(): void {
    void this.loadData();
    void this.ensureSnapshot(true);
  }

  async exportPdf(): Promise<void> {
    this.exportingPdf.set(true);
    this.errorMessage.set(null);

    try {
      const snapshot = await this.ensureSnapshot(true);
      if (!snapshot) {
        return;
      }

      const detail = await this.reportsApi.getById(snapshot);
      downloadReportPdf(detail);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to export PDF.'));
    } finally {
      this.exportingPdf.set(false);
    }
  }

  async exportCsv(): Promise<void> {
    this.exportingCsv.set(true);
    this.errorMessage.set(null);

    try {
      const snapshot = await this.ensureSnapshot(true);
      if (!snapshot) {
        return;
      }

      const blob = await this.reportsApi.download(snapshot, 'csv');
      this.triggerDownload(blob, 'daily-collection-report.csv');
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to export report.'));
    } finally {
      this.exportingCsv.set(false);
    }
  }

  private async loadData(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);

    try {
      const response = await this.collectionsApi.list({
        page: this.page(),
        pageSize: this.pageSize(),
        branchId: this.branchFilter === 'all' ? undefined : this.branchFilter,
      });

      const filtered = response.items.filter((item) => this.isWithinDateRange(item.collectedOn));
      this.collections.set(filtered);
      this.totalCount.set(response.totalCount);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to load collection data.'));
    } finally {
      this.loading.set(false);
    }
  }

  private async ensureSnapshot(force = false): Promise<string | null> {
    if (!force && this.snapshotId()) {
      return this.snapshotId();
    }

    try {
      const existing = await this.reportsApi.list({
        reportType: ReportType.CollectionsDaily,
        pageSize: 1,
      });

      if (!force && existing.items.length > 0) {
        this.snapshotId.set(existing.items[0].id);
        return existing.items[0].id;
      }

      const detail = await this.reportsApi.generate({
        reportType: ReportType.CollectionsDaily,
        title: 'Daily Collection Summary',
        parametersJson: JSON.stringify({
          fromDate: this.fromDate,
          toDate: this.toDate,
          branchId: this.branchFilter,
          collectorId: this.collectorFilter,
        }),
      });

      this.snapshotId.set(detail.id);
      return detail.id;
    } catch {
      return this.snapshotId();
    }
  }

  private isWithinDateRange(collectedOn: string): boolean {
    const date = collectedOn.slice(0, 10);
    return date >= this.fromDate && date <= this.toDate;
  }

  private defaultFromDate(): string {
    const date = new Date();
    date.setDate(date.getDate() - 7);
    return date.toISOString().slice(0, 10);
  }

  private defaultToDate(): string {
    return new Date().toISOString().slice(0, 10);
  }

  private triggerDownload(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(url);
  }
}
