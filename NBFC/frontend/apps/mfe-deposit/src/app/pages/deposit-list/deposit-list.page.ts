import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective, AuthService } from '@patsanstha/auth';
import {
  DepositAccountStatus,
  DepositAccountSummary,
  DepositApiService,
  DepositProductType,
  DepositSummary,
  depositProductDotClass,
  depositProductLabel,
  depositStatusLabel,
  depositStatusVariant,
  extractApiErrorMessage,
  formatCompactInr,
  formatDepositDate,
  formatInr,
  formatTrendPercent,
  isMaturityPast,
  isMaturitySoon,
} from '@patsanstha/deposits-data-access';
import { PatsStatusPillComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-deposit-list-page',
  standalone: true,
  imports: [DecimalPipe, FormsModule, HasPermissionDirective, PatsStatusPillComponent],
  template: `
    <section class="deposits-page">
      <header class="deposits-page__header">
        <div>
          <h1>Deposit Accounts</h1>
          <p class="deposits-page__subtitle">
            ठेव खाते यादी — Manage and monitor all customer deposit accounts
          </p>
        </div>
        <button
          type="button"
          class="deposits-page__primary-btn"
          *patsHasPermission="'deposits.create'"
          (click)="createDeposit()">
          <span class="material-symbols-outlined">add_circle</span>
          New Deposit Account (नवीन ठेव खाते)
        </button>
      </header>

      <div class="deposits-page__kpis">
        <article class="deposits-page__kpi deposits-page__kpi--blue">
          <div class="deposits-page__kpi-top">
            <span>Total Deposits</span>
            <span class="material-symbols-outlined">account_balance</span>
          </div>
          <strong>{{ formatCompactInr(summary().totalDepositsAmount) }}</strong>
          @if (formatTrendPercent(summary().depositsTrendPercent); as trend) {
            <p class="deposits-page__kpi-trend">
              <span class="material-symbols-outlined">trending_up</span>
              {{ trend }} this month
            </p>
          } @else {
            <p class="deposits-page__kpi-sub">{{ summary().totalActiveAccounts }} active accounts</p>
          }
        </article>

        <article class="deposits-page__kpi deposits-page__kpi--green">
          <div class="deposits-page__kpi-top">
            <span>Active Savings</span>
            <span class="material-symbols-outlined">savings</span>
          </div>
          <strong>{{ summary().activeSavingsCount | number }}</strong>
          <p class="deposits-page__kpi-sub">बचत खाते</p>
        </article>

        <article class="deposits-page__kpi deposits-page__kpi--brown">
          <div class="deposits-page__kpi-top">
            <span>Fixed Deposits</span>
            <span class="material-symbols-outlined">lock_clock</span>
          </div>
          <strong>{{ formatCompactInr(summary().fixedDepositsBalance) }}</strong>
          <p class="deposits-page__kpi-sub">मुदत ठेव</p>
        </article>

        <article class="deposits-page__kpi deposits-page__kpi--red">
          <div class="deposits-page__kpi-top">
            <span>Due This Month</span>
            <span class="material-symbols-outlined">event_upcoming</span>
          </div>
          <strong>{{ summary().dueThisMonthCount | number }}</strong>
          <p class="deposits-page__kpi-sub">येत्या काळात मॅच्युअर होणारे</p>
        </article>
      </div>

      <div class="deposits-page__toolbar">
        <input
          class="deposits-page__search"
          type="search"
          placeholder="Filter by Member Name or Acc"
          [ngModel]="search()"
          (ngModelChange)="onSearchChange($event)" />

        <select
          class="deposits-page__filter"
          [ngModel]="productFilter()"
          (ngModelChange)="onProductFilterChange($event)">
          <option [ngValue]="null">Product: All Products</option>
          <option [ngValue]="0">Savings Account</option>
          <option [ngValue]="1">Recurring Deposit</option>
          <option [ngValue]="2">Fixed Deposit</option>
        </select>

        <select
          class="deposits-page__filter"
          [ngModel]="statusFilter()"
          (ngModelChange)="onStatusFilterChange($event)">
          <option [ngValue]="null">Status: All Status</option>
          <option [ngValue]="0">Active</option>
          <option [ngValue]="1">Matured</option>
          <option [ngValue]="2">Closed</option>
        </select>

        <button type="button" class="deposits-page__reset" (click)="resetFilters()">
          <span class="material-symbols-outlined">refresh</span>
          Reset
        </button>

        <div class="deposits-page__toolbar-actions">
          <button type="button" class="deposits-page__icon-btn" title="Download">
            <span class="material-symbols-outlined">download</span>
          </button>
          <button type="button" class="deposits-page__icon-btn" title="Print" (click)="printList()">
            <span class="material-symbols-outlined">print</span>
          </button>
        </div>
      </div>

      @if (errorMessage()) {
        <p class="deposits-page__error">{{ errorMessage() }}</p>
      }

      <div class="deposits-page__table-wrap">
        <table class="deposits-page__table">
          <thead>
            <tr>
              <th>Account Details</th>
              <th>Member Name (सभासद नाव)</th>
              <th>Product Type</th>
              <th>Balance (INR)</th>
              <th>Interest</th>
              <th>Maturity Date</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            @if (loading()) {
              <tr>
                <td colspan="8" class="deposits-page__empty">Loading deposit accounts…</td>
              </tr>
            } @else if (accounts().length === 0) {
              <tr>
                <td colspan="8" class="deposits-page__empty">No deposit accounts found.</td>
              </tr>
            } @else {
              @for (account of accounts(); track account.id) {
                <tr
                  class="deposits-page__row"
                  [class.deposits-page__row--alert]="isRowAlert(account)"
                  (click)="openDeposit(account)">
                  <td>
                    <strong>{{ account.accountNumber }}</strong>
                    <span>Opened {{ formatDepositDate(account.openedOn) }}</span>
                  </td>
                  <td>
                    <strong>{{ account.memberName }}</strong>
                    <span>{{ account.memberNumber }}</span>
                  </td>
                  <td>
                    <span class="deposits-page__product">
                      <i [class]="'deposits-page__dot ' + productDotClass(account.productType)"></i>
                      {{ depositProductLabel(account.productType) }}
                    </span>
                  </td>
                  <td class="deposits-page__amount">{{ formatInr(account.currentBalance) }}</td>
                  <td>{{ account.interestRate.toFixed(1) }}%</td>
                  <td
                    [class.deposits-page__maturity--warn]="isMaturitySoon(account.maturityDate) || isMaturityPast(account.maturityDate)">
                    {{ formatDepositDate(account.maturityDate) }}
                  </td>
                  <td (click)="$event.stopPropagation()">
                    <pats-status-pill
                      [label]="depositStatusLabel(account.status)"
                      [variant]="depositStatusVariant(account.status)" />
                  </td>
                  <td (click)="$event.stopPropagation()">
                    <button type="button" class="deposits-page__menu-btn" (click)="openDeposit(account)">
                      <span class="material-symbols-outlined">more_vert</span>
                    </button>
                  </td>
                </tr>
              }
            }
          </tbody>
        </table>
      </div>

      <footer class="deposits-page__pagination">
        <span>
          Showing {{ rangeStart() }} to {{ rangeEnd() }} of {{ totalCount() | number }} accounts
        </span>
        <div class="deposits-page__pagination-actions">
          <button
            type="button"
            class="deposits-page__page-btn"
            [disabled]="page() <= 1 || loading()"
            (click)="goToPage(page() - 1)">
            <span class="material-symbols-outlined">chevron_left</span>
          </button>
          @for (pageNumber of visiblePages(); track pageNumber) {
            @if (pageNumber === -1) {
              <span class="deposits-page__ellipsis">…</span>
            } @else {
              <button
                type="button"
                class="deposits-page__page-btn"
                [class.deposits-page__page-btn--active]="pageNumber === page()"
                (click)="goToPage(pageNumber)">
                {{ pageNumber }}
              </button>
            }
          }
          <button
            type="button"
            class="deposits-page__page-btn"
            [disabled]="!hasNextPage() || loading()"
            (click)="goToPage(page() + 1)">
            <span class="material-symbols-outlined">chevron_right</span>
          </button>
        </div>
      </footer>

      <div class="deposits-page__bottom">
        <article class="deposits-page__guide">
          <h3>Quick Status Guide</h3>
          <ul>
            <li><i class="deposits-page__dot dot--savings"></i> Active</li>
            <li><i class="deposits-page__dot dot--default"></i> Closed</li>
            <li><i class="deposits-page__dot dot--fixed"></i> Matured</li>
            <li><i class="deposits-page__dot dot--alert"></i> Dormant</li>
          </ul>
        </article>

        <article class="deposits-page__renewals">
          <span class="material-symbols-outlined">info</span>
          <div>
            <p>
              You have <strong>{{ summary().dueThisMonthCount }}</strong> Fixed Deposits maturing in the next 30 days.
              Contact members for renewal or payout preferences.
            </p>
            <button type="button" class="deposits-page__link" (click)="showMaturing()">
              View Maturing List →
            </button>
          </div>
        </article>
      </div>
    </section>
  `,
  styles: [
    `
      .deposits-page {
        display: flex;
        flex-direction: column;
        gap: 24px;
        width: 100%;
      }

      .deposits-page__header {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 16px;
        flex-wrap: wrap;
      }

      .deposits-page__header h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 32px;
        color: var(--pats-color-primary);
      }

      .deposits-page__subtitle {
        margin: 8px 0 0;
        color: var(--pats-color-text-secondary);
        font-size: 14px;
      }

      .deposits-page__primary-btn {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        min-height: 44px;
        padding: 0 20px;
        border: none;
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary);
        font-size: 14px;
        font-weight: 600;
        cursor: pointer;
      }

      .deposits-page__kpis {
        display: grid;
        grid-template-columns: repeat(4, minmax(0, 1fr));
        gap: 16px;
      }

      @media (max-width: 1100px) {
        .deposits-page__kpis {
          grid-template-columns: repeat(2, minmax(0, 1fr));
        }
      }

      @media (max-width: 640px) {
        .deposits-page__kpis {
          grid-template-columns: 1fr;
        }
      }

      .deposits-page__kpi {
        padding: 20px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
        box-shadow: var(--pats-shadow-card);
      }

      .deposits-page__kpi--blue { border-top: 4px solid #1a3c6e; }
      .deposits-page__kpi--green { border-top: 4px solid var(--pats-color-success); }
      .deposits-page__kpi--brown { border-top: 4px solid #8b6914; }
      .deposits-page__kpi--red { border-top: 4px solid var(--pats-color-error); }

      .deposits-page__kpi-top {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 12px;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
      }

      .deposits-page__kpi strong {
        display: block;
        font-family: var(--pats-font-display);
        font-size: 28px;
        line-height: 1.2;
      }

      .deposits-page__kpi-trend,
      .deposits-page__kpi-sub {
        margin: 8px 0 0;
        font-size: 12px;
        color: var(--pats-color-text-secondary);
      }

      .deposits-page__kpi-trend {
        display: inline-flex;
        align-items: center;
        gap: 4px;
        color: var(--pats-color-success);
        font-weight: 600;
      }

      .deposits-page__toolbar {
        display: flex;
        flex-wrap: wrap;
        align-items: center;
        gap: 12px;
      }

      .deposits-page__search,
      .deposits-page__filter {
        min-height: 44px;
        padding: 0 16px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-surface-container-lowest);
        font-size: 14px;
      }

      .deposits-page__search {
        flex: 1;
        min-width: 220px;
      }

      .deposits-page__reset,
      .deposits-page__icon-btn,
      .deposits-page__page-btn,
      .deposits-page__menu-btn {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 4px;
        min-height: 44px;
        padding: 0 12px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-surface-container-lowest);
        color: var(--pats-color-on-surface);
        cursor: pointer;
      }

      .deposits-page__toolbar-actions {
        display: flex;
        gap: 8px;
        margin-left: auto;
      }

      .deposits-page__icon-btn {
        width: 44px;
        padding: 0;
      }

      .deposits-page__table-wrap {
        overflow: auto;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        box-shadow: var(--pats-shadow-card);
      }

      .deposits-page__table {
        width: 100%;
        border-collapse: collapse;
      }

      .deposits-page__table thead th {
        padding: 14px 16px;
        text-align: left;
        font-size: 12px;
        font-weight: 600;
        color: var(--pats-color-text-secondary);
        background: var(--pats-color-surface-muted);
        border-bottom: 1px solid var(--pats-color-border-subtle);
        white-space: nowrap;
      }

      .deposits-page__table tbody td {
        padding: 16px;
        border-bottom: 1px solid var(--pats-color-border-subtle);
        vertical-align: middle;
        font-size: 14px;
      }

      .deposits-page__table tbody td strong {
        display: block;
        font-weight: 600;
      }

      .deposits-page__table tbody td span {
        display: block;
        margin-top: 4px;
        font-size: 12px;
        color: var(--pats-color-text-secondary);
      }

      .deposits-page__row {
        cursor: pointer;
        transition: background 0.15s ease;
      }

      .deposits-page__row:hover {
        background: rgba(26, 60, 110, 0.04);
      }

      .deposits-page__row--alert {
        box-shadow: inset 4px 0 0 var(--pats-color-error);
      }

      .deposits-page__product {
        display: inline-flex;
        align-items: center;
        gap: 8px;
      }

      .deposits-page__dot {
        width: 8px;
        height: 8px;
        border-radius: 50%;
        flex-shrink: 0;
      }

      .dot--savings { background: var(--pats-color-success); }
      .dot--recurring { background: #2563eb; }
      .dot--fixed { background: #8b6914; }
      .dot--default { background: #94a3b8; }
      .dot--alert { background: var(--pats-color-error); }

      .deposits-page__amount {
        font-weight: 600;
        white-space: nowrap;
      }

      .deposits-page__maturity--warn {
        color: var(--pats-color-error);
        font-weight: 600;
      }

      .deposits-page__menu-btn {
        width: 36px;
        min-height: 36px;
        padding: 0;
      }

      .deposits-page__empty {
        text-align: center;
        color: var(--pats-color-text-secondary);
        padding: 32px !important;
      }

      .deposits-page__pagination {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 16px;
        flex-wrap: wrap;
        color: var(--pats-color-text-secondary);
        font-size: 14px;
      }

      .deposits-page__pagination-actions {
        display: flex;
        align-items: center;
        gap: 6px;
      }

      .deposits-page__page-btn {
        min-width: 36px;
        min-height: 36px;
        padding: 0 10px;
      }

      .deposits-page__page-btn--active {
        background: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary);
        border-color: var(--pats-color-primary-container);
      }

      .deposits-page__page-btn:disabled {
        opacity: 0.45;
        cursor: not-allowed;
      }

      .deposits-page__ellipsis {
        padding: 0 4px;
        color: var(--pats-color-text-secondary);
      }

      .deposits-page__bottom {
        display: grid;
        grid-template-columns: 1fr 2fr;
        gap: 16px;
      }

      @media (max-width: 900px) {
        .deposits-page__bottom {
          grid-template-columns: 1fr;
        }
      }

      .deposits-page__guide,
      .deposits-page__renewals {
        padding: 20px;
        border-radius: var(--pats-radius-lg);
        border: 1px solid var(--pats-color-border-subtle);
        background: var(--pats-color-surface-container-lowest);
      }

      .deposits-page__guide h3 {
        margin: 0 0 12px;
        font-size: 15px;
        font-family: var(--pats-font-display);
      }

      .deposits-page__guide ul {
        list-style: none;
        margin: 0;
        padding: 0;
        display: grid;
        gap: 8px;
      }

      .deposits-page__guide li {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
      }

      .deposits-page__renewals {
        display: flex;
        gap: 12px;
        align-items: flex-start;
        background: rgba(26, 60, 110, 0.06);
      }

      .deposits-page__renewals .material-symbols-outlined {
        color: var(--pats-color-primary-container);
      }

      .deposits-page__renewals p {
        margin: 0;
        font-size: 14px;
        color: var(--pats-color-text-secondary);
        line-height: 1.5;
      }

      .deposits-page__link {
        margin-top: 8px;
        padding: 0;
        border: none;
        background: none;
        color: var(--pats-color-primary-container);
        font-size: 13px;
        font-weight: 600;
        cursor: pointer;
      }

      .deposits-page__error {
        color: var(--pats-color-error);
      }
    `,
  ],
})
export class DepositListPageComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly depositApi = inject(DepositApiService);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly accounts = signal<DepositAccountSummary[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(5);
  readonly totalCount = signal(0);
  readonly search = signal('');
  readonly productFilter = signal<DepositProductType | null>(null);
  readonly statusFilter = signal<DepositAccountStatus | null>(null);
  readonly summary = signal<DepositSummary>({
    totalDepositsAmount: 0,
    depositsTrendPercent: null,
    totalActiveAccounts: 0,
    activeSavingsCount: 0,
    fixedDepositsBalance: 0,
    dueThisMonthCount: 0,
  });

  protected readonly formatInr = formatInr;
  protected readonly formatCompactInr = formatCompactInr;
  protected readonly formatDepositDate = formatDepositDate;
  protected readonly formatTrendPercent = formatTrendPercent;
  protected readonly depositProductLabel = depositProductLabel;
  protected readonly depositStatusLabel = depositStatusLabel;
  protected readonly depositStatusVariant = depositStatusVariant;
  protected readonly isMaturitySoon = isMaturitySoon;
  protected readonly isMaturityPast = isMaturityPast;
  protected readonly productDotClass = depositProductDotClass;

  readonly hasNextPage = computed(() => this.page() * this.pageSize() < this.totalCount());
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));
  readonly rangeStart = computed(() =>
    this.totalCount() === 0 ? 0 : (this.page() - 1) * this.pageSize() + 1
  );
  readonly rangeEnd = computed(() =>
    Math.min(this.page() * this.pageSize(), this.totalCount())
  );

  readonly visiblePages = computed(() => {
    const total = this.totalPages();
    const current = this.page();
    const pages: number[] = [];

    if (total <= 7) {
      for (let i = 1; i <= total; i += 1) {
        pages.push(i);
      }
      return pages;
    }

    pages.push(1);
    if (current > 3) {
      pages.push(-1);
    }

    const start = Math.max(2, current - 1);
    const end = Math.min(total - 1, current + 1);
    for (let i = start; i <= end; i += 1) {
      pages.push(i);
    }

    if (current < total - 2) {
      pages.push(-1);
    }
    pages.push(total);

    return pages;
  });

  ngOnInit(): void {
    void this.loadSummary();
    void this.loadDeposits();
  }

  onSearchChange(value: string): void {
    this.search.set(value);
    this.page.set(1);
    void this.loadDeposits();
  }

  onProductFilterChange(value: DepositProductType | null): void {
    this.productFilter.set(value);
    this.page.set(1);
    void this.loadDeposits();
  }

  onStatusFilterChange(value: DepositAccountStatus | null): void {
    this.statusFilter.set(value);
    this.page.set(1);
    void this.loadDeposits();
  }

  resetFilters(): void {
    this.search.set('');
    this.productFilter.set(null);
    this.statusFilter.set(null);
    this.page.set(1);
    void this.loadDeposits();
  }

  goToPage(nextPage: number): void {
    if (nextPage < 1 || nextPage > this.totalPages()) {
      return;
    }

    this.page.set(nextPage);
    void this.loadDeposits();
  }

  openDeposit(account: DepositAccountSummary): void {
    void this.router.navigate(['/deposits', account.id]);
  }

  createDeposit(): void {
    void this.router.navigate(['/deposits/new']);
  }

  isRowAlert(account: DepositAccountSummary): boolean {
    return (
      account.status === DepositAccountStatus.Matured ||
      isMaturityPast(account.maturityDate) ||
      isMaturitySoon(account.maturityDate, 7)
    );
  }

  showMaturing(): void {
    this.productFilter.set(DepositProductType.FixedDeposit);
    this.statusFilter.set(DepositAccountStatus.Active);
    this.page.set(1);
    void this.loadDeposits();
  }

  printList(): void {
    window.print();
  }

  private async loadSummary(): Promise<void> {
    try {
      const branchId = this.auth.user()?.branchId ?? undefined;
      const summary = await this.depositApi.getSummary(branchId);
      this.summary.set(summary);
    } catch {
      this.summary.set({
        totalDepositsAmount: 0,
        depositsTrendPercent: null,
        totalActiveAccounts: 0,
        activeSavingsCount: 0,
        fixedDepositsBalance: 0,
        dueThisMonthCount: 0,
      });
    }
  }

  private async loadDeposits(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);

    try {
      const response = await this.depositApi.list({
        page: this.page(),
        pageSize: this.pageSize(),
        search: this.search() || undefined,
        productType: this.productFilter() ?? undefined,
        status: this.statusFilter() ?? undefined,
      });

      this.accounts.set(response.items);
      this.totalCount.set(response.totalCount);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to load deposit accounts.'));
    } finally {
      this.loading.set(false);
    }
  }
}
