import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '@patsanstha/auth';
import {
  CollectionApiService,
  CollectionReceiptSummary,
  extractApiErrorMessage,
  formatInr,
  collectionStatusLabel,
  paymentModeLabel,
} from '@patsanstha/collections-data-access';
import { PatsButtonComponent, PatsTableComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-collection-list-page',
  standalone: true,
  imports: [FormsModule, HasPermissionDirective, PatsButtonComponent, PatsTableComponent],
  template: `
    <section class="collections-page">
      <header class="collections-page__header">
        <div>
          <h1>Collections <span class="collections-page__subtitle">/ वसुली</span></h1>
          <p>Record and track loan repayment collections.</p>
        </div>
        <pats-button
          *patsHasPermission="'collections.collect'"
          icon="add_circle"
          (clicked)="createCollection()">
          Record Collection
        </pats-button>
      </header>

      <div class="collections-page__toolbar">
        <input
          class="collections-page__search"
          type="search"
          placeholder="Search by member, loan, or receipt no…"
          [ngModel]="search()"
          (ngModelChange)="onSearchChange($event)" />
      </div>

      @if (errorMessage()) {
        <p class="collections-page__error">{{ errorMessage() }}</p>
      }

      <pats-table
        [columns]="columns"
        [rows]="tableRows()"
        [loading]="loading()"
        [rowClickable]="true"
        emptyMessage="No collection receipts found."
        (rowClicked)="openCollection($event)" />

      <footer class="collections-page__pagination">
        <span>{{ totalCount() }} receipts</span>
        <div class="collections-page__pagination-actions">
          <pats-button variant="ghost" size="sm" [disabled]="page() <= 1 || loading()" (clicked)="goToPage(page() - 1)">Previous</pats-button>
          <span>Page {{ page() }}</span>
          <pats-button variant="ghost" size="sm" [disabled]="!hasNextPage() || loading()" (clicked)="goToPage(page() + 1)">Next</pats-button>
        </div>
      </footer>
    </section>
  `,
  styles: [
    `
      .collections-page { display: flex; flex-direction: column; gap: 24px; }
      .collections-page__header { display: flex; align-items: flex-start; justify-content: space-between; gap: 16px; }
      .collections-page__header h1 { margin: 0; font-family: var(--pats-font-display); font-size: 32px; color: var(--pats-color-primary); }
      .collections-page__subtitle { font-size: 18px; color: var(--pats-color-text-secondary); font-weight: 500; }
      .collections-page__header p { margin: 8px 0 0; color: var(--pats-color-text-secondary); }
      .collections-page__toolbar { display: flex; flex-wrap: wrap; gap: 12px; }
      .collections-page__search { flex: 1; min-width: 240px; min-height: 44px; padding: 0 16px; border: 1px solid var(--pats-color-border-subtle); border-radius: var(--pats-radius-md); background: var(--pats-color-surface-container-lowest); font-size: 14px; }
      .collections-page__error { color: var(--pats-color-error); }
      .collections-page__pagination { display: flex; align-items: center; justify-content: space-between; color: var(--pats-color-text-secondary); font-size: 14px; }
      .collections-page__pagination-actions { display: flex; align-items: center; gap: 12px; }
    `,
  ],
})
export class CollectionListPageComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly collectionApi = inject(CollectionApiService);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly receipts = signal<CollectionReceiptSummary[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly search = signal('');

  readonly columns = [
    { key: 'receiptNumber', header: 'Receipt No.' },
    { key: 'loanNumber', header: 'Loan No.' },
    { key: 'memberName', header: 'Member' },
    { key: 'amountLabel', header: 'Amount' },
    { key: 'paymentModeLabel', header: 'Mode' },
    { key: 'collectedOn', header: 'Collected On' },
    { key: 'statusLabel', header: 'Status' },
  ];

  readonly tableRows = computed(() =>
    this.receipts().map((receipt) => ({
      ...receipt,
      amountLabel: formatInr(receipt.amount),
      paymentModeLabel: paymentModeLabel(receipt.paymentMode),
      statusLabel: collectionStatusLabel(receipt.status),
    }))
  );

  readonly hasNextPage = computed(() => this.page() * this.pageSize() < this.totalCount());

  ngOnInit(): void { void this.loadCollections(); }

  onSearchChange(value: string): void { this.search.set(value); this.page.set(1); void this.loadCollections(); }
  goToPage(nextPage: number): void { this.page.set(nextPage); void this.loadCollections(); }
  openCollection(row: CollectionReceiptSummary): void { void this.router.navigate(['/collections', row.id]); }
  createCollection(): void { void this.router.navigate(['/collections/new']); }

  private async loadCollections(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);
    try {
      const response = await this.collectionApi.list({
        page: this.page(),
        pageSize: this.pageSize(),
        search: this.search() || undefined,
      });
      this.receipts.set(response.items);
      this.totalCount.set(response.totalCount);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to load collections.'));
    } finally {
      this.loading.set(false);
    }
  }
}
