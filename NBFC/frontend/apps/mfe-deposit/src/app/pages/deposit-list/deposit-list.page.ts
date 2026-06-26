import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '@patsanstha/auth';
import {
  DepositAccountSummary,
  DepositApiService,
  DepositProductType,
  extractApiErrorMessage,
  depositProductLabel,
  depositStatusLabel,
  formatInr,
} from '@patsanstha/deposits-data-access';
import { PatsButtonComponent, PatsTableComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-deposit-list-page',
  standalone: true,
  imports: [FormsModule, HasPermissionDirective, PatsButtonComponent, PatsTableComponent],
  template: `
    <section class="deposits-page">
      <header class="deposits-page__header">
        <div>
          <h1>Deposit Accounts <span class="deposits-page__subtitle">/ ठेव खाते</span></h1>
          <p>Manage and monitor all customer deposit accounts.</p>
        </div>
        <pats-button
          *patsHasPermission="'deposits.create'"
          icon="add_circle"
          (clicked)="createDeposit()">
          New Deposit Account
        </pats-button>
      </header>

      <div class="deposits-page__toolbar">
        <input
          class="deposits-page__search"
          type="search"
          placeholder="Filter by member name or account no…"
          [ngModel]="search()"
          (ngModelChange)="onSearchChange($event)" />

        <select
          class="deposits-page__filter"
          [ngModel]="productFilter()"
          (ngModelChange)="onProductFilterChange($event)">
          <option [ngValue]="null">All Products</option>
          <option [ngValue]="0">Savings Account</option>
          <option [ngValue]="1">Recurring Deposit (RD)</option>
          <option [ngValue]="2">Fixed Deposit (FD)</option>
        </select>
      </div>

      @if (errorMessage()) {
        <p class="deposits-page__error">{{ errorMessage() }}</p>
      }

      <pats-table
        [columns]="columns"
        [rows]="tableRows()"
        [loading]="loading()"
        [rowClickable]="true"
        emptyMessage="No deposit accounts found."
        (rowClicked)="openDeposit($event)" />

      <footer class="deposits-page__pagination">
        <span>{{ totalCount() }} accounts</span>
        <div class="deposits-page__pagination-actions">
          <pats-button
            variant="ghost"
            size="sm"
            [disabled]="page() <= 1 || loading()"
            (clicked)="goToPage(page() - 1)">
            Previous
          </pats-button>
          <span>Page {{ page() }}</span>
          <pats-button
            variant="ghost"
            size="sm"
            [disabled]="!hasNextPage() || loading()"
            (clicked)="goToPage(page() + 1)">
            Next
          </pats-button>
        </div>
      </footer>
    </section>
  `,
  styles: [
    `
      .deposits-page {
        display: flex;
        flex-direction: column;
        gap: 24px;
      }

      .deposits-page__header {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 16px;
      }

      .deposits-page__header h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 32px;
        color: var(--pats-color-primary);
      }

      .deposits-page__subtitle {
        font-size: 18px;
        color: var(--pats-color-text-secondary);
        font-weight: 500;
      }

      .deposits-page__header p {
        margin: 8px 0 0;
        color: var(--pats-color-text-secondary);
      }

      .deposits-page__toolbar {
        display: flex;
        flex-wrap: wrap;
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
        min-width: 240px;
      }

      .deposits-page__error {
        color: var(--pats-color-error);
      }

      .deposits-page__pagination {
        display: flex;
        align-items: center;
        justify-content: space-between;
        color: var(--pats-color-text-secondary);
        font-size: 14px;
      }

      .deposits-page__pagination-actions {
        display: flex;
        align-items: center;
        gap: 12px;
      }
    `,
  ],
})
export class DepositListPageComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly depositApi = inject(DepositApiService);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly accounts = signal<DepositAccountSummary[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly search = signal('');
  readonly productFilter = signal<DepositProductType | null>(null);

  readonly columns = [
    { key: 'accountNumber', header: 'Account' },
    { key: 'memberName', header: 'Member' },
    { key: 'productLabel', header: 'Product' },
    { key: 'balanceLabel', header: 'Balance' },
    { key: 'interestLabel', header: 'Interest' },
    { key: 'maturityLabel', header: 'Maturity' },
    { key: 'statusLabel', header: 'Status' },
  ];

  readonly tableRows = computed(() =>
    this.accounts().map((account) => ({
      ...account,
      productLabel: depositProductLabel(account.productType),
      balanceLabel: formatInr(account.currentBalance),
      interestLabel: `${account.interestRate}%`,
      maturityLabel: account.maturityDate ?? '—',
      statusLabel: depositStatusLabel(account.status),
    }))
  );

  readonly hasNextPage = computed(() => this.page() * this.pageSize() < this.totalCount());

  ngOnInit(): void {
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

  goToPage(nextPage: number): void {
    this.page.set(nextPage);
    void this.loadDeposits();
  }

  openDeposit(row: DepositAccountSummary & { productLabel?: string }): void {
    void this.router.navigate(['/deposits', row.id]);
  }

  createDeposit(): void {
    void this.router.navigate(['/deposits/new']);
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
