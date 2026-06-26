import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '@patsanstha/auth';
import {
  LoanApiService,
  LoanApplicationSummary,
  LoanApplicationStatus,
  extractApiErrorMessage,
  formatInr,
  loanProductLabel,
  loanStatusLabel,
} from '@patsanstha/loans-data-access';
import { PatsButtonComponent, PatsTableComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-loan-list-page',
  standalone: true,
  imports: [FormsModule, HasPermissionDirective, PatsButtonComponent, PatsTableComponent],
  template: `
    <section class="loans-page">
      <header class="loans-page__header">
        <div>
          <h1>Loan Applications <span class="loans-page__subtitle">/ कर्ज अर्ज</span></h1>
          <p>Review, approve, and disburse member loan applications.</p>
        </div>
        <pats-button
          *patsHasPermission="'loans.create'"
          icon="add_circle"
          (clicked)="createLoan()">
          New Application
        </pats-button>
      </header>

      <div class="loans-page__toolbar">
        <input
          class="loans-page__search"
          type="search"
          placeholder="Search by member or loan number…"
          [ngModel]="search()"
          (ngModelChange)="onSearchChange($event)" />
        <select
          class="loans-page__filter"
          [ngModel]="statusFilter()"
          (ngModelChange)="onStatusFilterChange($event)">
          <option [ngValue]="null">All Status</option>
          <option [ngValue]="0">Submitted</option>
          <option [ngValue]="2">Approved</option>
          <option [ngValue]="3">Rejected</option>
          <option [ngValue]="4">Disbursed</option>
        </select>
      </div>

      @if (errorMessage()) {
        <p class="loans-page__error">{{ errorMessage() }}</p>
      }

      <pats-table
        [columns]="columns"
        [rows]="tableRows()"
        [loading]="loading()"
        [rowClickable]="true"
        emptyMessage="No loan applications found."
        (rowClicked)="openLoan($event)" />

      <footer class="loans-page__pagination">
        <span>{{ totalCount() }} applications</span>
        <div class="loans-page__pagination-actions">
          <pats-button variant="ghost" size="sm" [disabled]="page() <= 1 || loading()" (clicked)="goToPage(page() - 1)">Previous</pats-button>
          <span>Page {{ page() }}</span>
          <pats-button variant="ghost" size="sm" [disabled]="!hasNextPage() || loading()" (clicked)="goToPage(page() + 1)">Next</pats-button>
        </div>
      </footer>
    </section>
  `,
  styles: [
    `
      .loans-page { display: flex; flex-direction: column; gap: 24px; }
      .loans-page__header { display: flex; align-items: flex-start; justify-content: space-between; gap: 16px; }
      .loans-page__header h1 { margin: 0; font-family: var(--pats-font-display); font-size: 32px; color: var(--pats-color-primary); }
      .loans-page__subtitle { font-size: 18px; color: var(--pats-color-text-secondary); font-weight: 500; }
      .loans-page__header p { margin: 8px 0 0; color: var(--pats-color-text-secondary); }
      .loans-page__toolbar { display: flex; flex-wrap: wrap; gap: 12px; }
      .loans-page__search, .loans-page__filter { min-height: 44px; padding: 0 16px; border: 1px solid var(--pats-color-border-subtle); border-radius: var(--pats-radius-md); background: var(--pats-color-surface-container-lowest); font-size: 14px; }
      .loans-page__search { flex: 1; min-width: 240px; }
      .loans-page__error { color: var(--pats-color-error); }
      .loans-page__pagination { display: flex; align-items: center; justify-content: space-between; color: var(--pats-color-text-secondary); font-size: 14px; }
      .loans-page__pagination-actions { display: flex; align-items: center; gap: 12px; }
    `,
  ],
})
export class LoanListPageComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly loanApi = inject(LoanApiService);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly loans = signal<LoanApplicationSummary[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly search = signal('');
  readonly statusFilter = signal<LoanApplicationStatus | null>(null);

  readonly columns = [
    { key: 'loanNumber', header: 'Loan No.' },
    { key: 'memberName', header: 'Member' },
    { key: 'productLabel', header: 'Product' },
    { key: 'requestedLabel', header: 'Requested' },
    { key: 'emiLabel', header: 'EMI' },
    { key: 'statusLabel', header: 'Status' },
  ];

  readonly tableRows = computed(() =>
    this.loans().map((loan) => ({
      ...loan,
      productLabel: loanProductLabel(loan.productType),
      requestedLabel: formatInr(loan.requestedAmount),
      emiLabel: loan.emiAmount != null ? formatInr(loan.emiAmount) : '—',
      statusLabel: loanStatusLabel(loan.status),
    }))
  );

  readonly hasNextPage = computed(() => this.page() * this.pageSize() < this.totalCount());

  ngOnInit(): void { void this.loadLoans(); }

  onSearchChange(value: string): void { this.search.set(value); this.page.set(1); void this.loadLoans(); }
  onStatusFilterChange(value: LoanApplicationStatus | null): void { this.statusFilter.set(value); this.page.set(1); void this.loadLoans(); }
  goToPage(nextPage: number): void { this.page.set(nextPage); void this.loadLoans(); }
  openLoan(row: LoanApplicationSummary): void { void this.router.navigate(['/loans', row.id]); }
  createLoan(): void { void this.router.navigate(['/loans/new']); }

  private async loadLoans(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);
    try {
      const response = await this.loanApi.list({
        page: this.page(),
        pageSize: this.pageSize(),
        search: this.search() || undefined,
        status: this.statusFilter() ?? undefined,
      });
      this.loans.set(response.items);
      this.totalCount.set(response.totalCount);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to load loans.'));
    } finally {
      this.loading.set(false);
    }
  }
}
