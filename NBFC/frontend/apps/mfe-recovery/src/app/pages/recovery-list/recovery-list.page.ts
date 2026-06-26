import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '@patsanstha/auth';
import {
  RecoveryApiService,
  RecoveryCaseStatus,
  RecoveryCaseSummary,
  extractApiErrorMessage,
  formatInr,
  recoveryStatusLabel,
} from '@patsanstha/recovery-data-access';
import { PatsButtonComponent, PatsTableComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-recovery-list-page',
  standalone: true,
  imports: [FormsModule, HasPermissionDirective, PatsButtonComponent, PatsTableComponent],
  template: `
    <section class="recovery-page">
      <header class="recovery-page__header">
        <div>
          <h1>Recovery Cases <span class="recovery-page__subtitle">/ वसुली प्रकरण</span></h1>
          <p>Track and manage overdue loan recovery cases.</p>
        </div>
        <pats-button
          *patsHasPermission="'recovery.manage'"
          icon="add_circle"
          (clicked)="createCase()">
          Open Recovery Case
        </pats-button>
      </header>

      <div class="recovery-page__toolbar">
        <input
          class="recovery-page__search"
          type="search"
          placeholder="Search by member, loan, or case no…"
          [ngModel]="search()"
          (ngModelChange)="onSearchChange($event)" />
        <select
          class="recovery-page__filter"
          [ngModel]="statusFilter()"
          (ngModelChange)="onStatusFilterChange($event)">
          <option [ngValue]="null">All Status</option>
          <option [ngValue]="0">Open</option>
          <option [ngValue]="1">In Progress</option>
          <option [ngValue]="2">Resolved</option>
          <option [ngValue]="3">Written Off</option>
        </select>
      </div>

      @if (errorMessage()) {
        <p class="recovery-page__error">{{ errorMessage() }}</p>
      }

      <pats-table
        [columns]="columns"
        [rows]="tableRows()"
        [loading]="loading()"
        [rowClickable]="true"
        emptyMessage="No recovery cases found."
        (rowClicked)="openCase($event)" />

      <footer class="recovery-page__pagination">
        <span>{{ totalCount() }} cases</span>
        <div class="recovery-page__pagination-actions">
          <pats-button variant="ghost" size="sm" [disabled]="page() <= 1 || loading()" (clicked)="goToPage(page() - 1)">Previous</pats-button>
          <span>Page {{ page() }}</span>
          <pats-button variant="ghost" size="sm" [disabled]="!hasNextPage() || loading()" (clicked)="goToPage(page() + 1)">Next</pats-button>
        </div>
      </footer>
    </section>
  `,
  styles: [
    `
      .recovery-page { display: flex; flex-direction: column; gap: 24px; }
      .recovery-page__header { display: flex; align-items: flex-start; justify-content: space-between; gap: 16px; }
      .recovery-page__header h1 { margin: 0; font-family: var(--pats-font-display); font-size: 32px; color: var(--pats-color-primary); }
      .recovery-page__subtitle { font-size: 18px; color: var(--pats-color-text-secondary); font-weight: 500; }
      .recovery-page__header p { margin: 8px 0 0; color: var(--pats-color-text-secondary); }
      .recovery-page__toolbar { display: flex; flex-wrap: wrap; gap: 12px; }
      .recovery-page__search, .recovery-page__filter { min-height: 44px; padding: 0 16px; border: 1px solid var(--pats-color-border-subtle); border-radius: var(--pats-radius-md); background: var(--pats-color-surface-container-lowest); font-size: 14px; }
      .recovery-page__search { flex: 1; min-width: 240px; }
      .recovery-page__error { color: var(--pats-color-error); }
      .recovery-page__pagination { display: flex; align-items: center; justify-content: space-between; color: var(--pats-color-text-secondary); font-size: 14px; }
      .recovery-page__pagination-actions { display: flex; align-items: center; gap: 12px; }
    `,
  ],
})
export class RecoveryListPageComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly recoveryApi = inject(RecoveryApiService);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly cases = signal<RecoveryCaseSummary[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly search = signal('');
  readonly statusFilter = signal<RecoveryCaseStatus | null>(null);

  readonly columns = [
    { key: 'caseNumber', header: 'Case No.' },
    { key: 'loanNumber', header: 'Loan No.' },
    { key: 'memberName', header: 'Member' },
    { key: 'outstandingLabel', header: 'Outstanding' },
    { key: 'daysPastDue', header: 'DPD' },
    { key: 'openedOn', header: 'Opened On' },
    { key: 'statusLabel', header: 'Status' },
  ];

  readonly tableRows = computed(() =>
    this.cases().map((recoveryCase) => ({
      ...recoveryCase,
      outstandingLabel: formatInr(recoveryCase.outstandingAmount),
      statusLabel: recoveryStatusLabel(recoveryCase.status),
    }))
  );

  readonly hasNextPage = computed(() => this.page() * this.pageSize() < this.totalCount());

  ngOnInit(): void { void this.loadCases(); }

  onSearchChange(value: string): void { this.search.set(value); this.page.set(1); void this.loadCases(); }
  onStatusFilterChange(value: RecoveryCaseStatus | null): void { this.statusFilter.set(value); this.page.set(1); void this.loadCases(); }
  goToPage(nextPage: number): void { this.page.set(nextPage); void this.loadCases(); }
  openCase(row: RecoveryCaseSummary): void { void this.router.navigate(['/recovery', row.id]); }
  createCase(): void { void this.router.navigate(['/recovery/new']); }

  private async loadCases(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);
    try {
      const response = await this.recoveryApi.list({
        page: this.page(),
        pageSize: this.pageSize(),
        search: this.search() || undefined,
        status: this.statusFilter() ?? undefined,
      });
      this.cases.set(response.items);
      this.totalCount.set(response.totalCount);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to load recovery cases.'));
    } finally {
      this.loading.set(false);
    }
  }
}
