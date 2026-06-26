import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '@patsanstha/auth';
import {
  AccountingApiService,
  JournalEntryStatus,
  JournalEntrySummary,
  extractApiErrorMessage,
  formatInr,
  journalStatusLabel,
} from '@patsanstha/accounting-data-access';
import { PatsButtonComponent, PatsTableComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-journal-list-page',
  standalone: true,
  imports: [FormsModule, HasPermissionDirective, PatsButtonComponent, PatsTableComponent],
  template: `
    <section class="page">
      <header class="page__header">
        <div>
          <h1>Journal Entries <span class="page__subtitle">/ लेखा नोंद</span></h1>
          <p>Review and post draft journal entries.</p>
        </div>
        <pats-button
          *patsHasPermission="'accounting.post'"
          icon="add_circle"
          (clicked)="createEntry()">
          New Entry
        </pats-button>
      </header>

      <div class="page__toolbar">
        <input
          class="page__search"
          type="search"
          placeholder="Search by entry number or description…"
          [ngModel]="search()"
          (ngModelChange)="onSearchChange($event)" />
        <select
          class="page__filter"
          [ngModel]="statusFilter()"
          (ngModelChange)="onStatusFilterChange($event)">
          <option [ngValue]="null">All Status</option>
          <option [ngValue]="0">Draft</option>
          <option [ngValue]="1">Posted</option>
          <option [ngValue]="2">Reversed</option>
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
        emptyMessage="No journal entries found."
        (rowClicked)="openEntry($event)" />

      <footer class="page__pagination">
        <span>{{ totalCount() }} entries</span>
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
      .page__search, .page__filter { min-height: 44px; padding: 0 16px; border: 1px solid var(--pats-color-border-subtle); border-radius: var(--pats-radius-md); background: var(--pats-color-surface-container-lowest); font-size: 14px; }
      .page__search { flex: 1; min-width: 240px; }
      .page__error { color: var(--pats-color-error); }
      .page__pagination { display: flex; align-items: center; justify-content: space-between; color: var(--pats-color-text-secondary); font-size: 14px; }
      .page__pagination-actions { display: flex; align-items: center; gap: 12px; }
    `,
  ],
})
export class JournalListPageComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly accountingApi = inject(AccountingApiService);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly entries = signal<JournalEntrySummary[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly search = signal('');
  readonly statusFilter = signal<JournalEntryStatus | null>(null);

  readonly columns = [
    { key: 'entryNumber', header: 'Entry No.' },
    { key: 'description', header: 'Description' },
    { key: 'entryDate', header: 'Date' },
    { key: 'debitAccountCode', header: 'Debit' },
    { key: 'creditAccountCode', header: 'Credit' },
    { key: 'amountLabel', header: 'Amount' },
    { key: 'statusLabel', header: 'Status' },
  ];

  readonly tableRows = computed(() =>
    this.entries().map((entry) => ({
      ...entry,
      amountLabel: formatInr(entry.amount),
      statusLabel: journalStatusLabel(entry.status),
    }))
  );

  readonly hasNextPage = computed(() => this.page() * this.pageSize() < this.totalCount());

  ngOnInit(): void {
    void this.loadEntries();
  }

  onSearchChange(value: string): void {
    this.search.set(value);
    this.page.set(1);
    void this.loadEntries();
  }

  onStatusFilterChange(value: JournalEntryStatus | null): void {
    this.statusFilter.set(value);
    this.page.set(1);
    void this.loadEntries();
  }

  goToPage(nextPage: number): void {
    this.page.set(nextPage);
    void this.loadEntries();
  }

  openEntry(row: JournalEntrySummary): void {
    void this.router.navigate(['/accounting', row.id]);
  }

  createEntry(): void {
    void this.router.navigate(['/accounting/new']);
  }

  private async loadEntries(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);
    try {
      const response = await this.accountingApi.list({
        page: this.page(),
        pageSize: this.pageSize(),
        search: this.search() || undefined,
        status: this.statusFilter() ?? undefined,
      });
      this.entries.set(response.items);
      this.totalCount.set(response.totalCount);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to load journal entries.'));
    } finally {
      this.loading.set(false);
    }
  }
}
