import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HasPermissionDirective } from '@patsanstha/auth';
import {
  CollectionApiService,
  CollectionReceiptDetail,
  CollectionReceiptStatus,
  extractApiErrorMessage,
  formatInr,
  collectionStatusLabel,
  collectionStatusVariant,
  paymentModeLabel,
} from '@patsanstha/collections-data-access';
import { PatsButtonComponent, PatsStatusPillComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-collection-detail-page',
  standalone: true,
  imports: [RouterLink, HasPermissionDirective, PatsButtonComponent, PatsStatusPillComponent],
  template: `
    <section class="detail-page">
      <a routerLink="/collections" class="detail-page__back">
        <span class="material-symbols-outlined">arrow_back</span>
        Back to collections
      </a>

      @if (loading()) {
        <p>Loading receipt…</p>
      } @else if (errorMessage()) {
        <p class="detail-page__error">{{ errorMessage() }}</p>
      } @else {
        @if (receipt(); as r) {
          <header class="detail-page__header">
            <div>
              <p class="detail-page__eyebrow">{{ r.receiptNumber }}</p>
              <h1>{{ r.memberName }}</h1>
              <p>Loan {{ r.loanNumber }} · {{ r.memberNumber }}</p>
            </div>
            <pats-status-pill [label]="statusLabel()" [variant]="statusVariant()" />
          </header>

          <div class="detail-page__amount">
            <span>Collected Amount</span>
            <strong>{{ formatInr(r.amount) }}</strong>
          </div>

          <div class="detail-page__grid">
            <article class="detail-page__card">
              <h2>Payment</h2>
              <dl>
                <dt>Payment Mode</dt><dd>{{ paymentModeLabel(r.paymentMode) }}</dd>
                <dt>Reference</dt><dd>{{ r.referenceNumber ?? '—' }}</dd>
                <dt>Collected On</dt><dd>{{ r.collectedOn }}</dd>
              </dl>
            </article>

            <article class="detail-page__card">
              <h2>Record</h2>
              <dl>
                <dt>Created</dt><dd>{{ r.createdAt }}</dd>
                <dt>Modified</dt><dd>{{ r.modifiedAt ?? '—' }}</dd>
                <dt>Branch ID</dt><dd>{{ r.branchId }}</dd>
              </dl>
            </article>
          </div>

          @if (r.status === collectedStatus) {
            <div class="detail-page__actions">
              <pats-button
                *patsHasPermission="'collections.collect'"
                variant="ghost"
                [loading]="actionLoading()"
                (clicked)="reverse()">
                Reverse Collection
              </pats-button>
            </div>
          }
        }
      }
    </section>
  `,
  styles: [
    `
      .detail-page { display: flex; flex-direction: column; gap: 24px; }
      .detail-page__back { display: inline-flex; align-items: center; gap: 4px; color: var(--pats-color-primary-container); font-weight: 600; font-size: 14px; }
      .detail-page__header { display: flex; justify-content: space-between; gap: 16px; padding: 24px; border-radius: var(--pats-radius-lg); background: var(--pats-color-surface-container-lowest); border: 1px solid var(--pats-color-border-subtle); border-top: 4px solid var(--pats-color-primary-container); }
      .detail-page__eyebrow { margin: 0 0 4px; font-size: 13px; font-weight: 600; text-transform: uppercase; color: var(--pats-color-text-secondary); }
      .detail-page__header h1 { margin: 0; font-family: var(--pats-font-display); font-size: 32px; }
      .detail-page__amount { padding: 24px; border-radius: var(--pats-radius-lg); background: var(--pats-color-primary); color: var(--pats-color-on-primary); display: flex; flex-direction: column; gap: 8px; }
      .detail-page__amount strong { font-family: var(--pats-font-display); font-size: 36px; }
      .detail-page__grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 16px; }
      .detail-page__card { padding: 24px; border-radius: var(--pats-radius-lg); background: var(--pats-color-surface-container-lowest); border: 1px solid var(--pats-color-border-subtle); }
      .detail-page__card h2 { margin: 0 0 16px; font-family: var(--pats-font-display); color: var(--pats-color-primary-container); }
      dl { margin: 0; display: grid; gap: 12px; }
      dt { font-size: 11px; font-weight: 600; text-transform: uppercase; color: var(--pats-color-text-secondary); }
      dd { margin: 0; font-size: 15px; }
      .detail-page__actions { display: flex; flex-wrap: wrap; gap: 12px; padding: 16px; border-radius: var(--pats-radius-lg); background: var(--pats-color-surface-muted); }
      .detail-page__error { color: var(--pats-color-error); }
    `,
  ],
})
export class CollectionDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly collectionApi = inject(CollectionApiService);

  readonly loading = signal(true);
  readonly actionLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly receipt = signal<CollectionReceiptDetail | null>(null);
  readonly statusLabel = signal('');
  readonly statusVariant = signal<'active' | 'inactive' | 'pending' | 'warning' | 'error'>('active');

  readonly collectedStatus = CollectionReceiptStatus.Collected;

  protected readonly formatInr = formatInr;
  protected readonly paymentModeLabel = paymentModeLabel;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { this.errorMessage.set('Receipt id missing.'); this.loading.set(false); return; }
    void this.loadReceipt(id);
  }

  async reverse(): Promise<void> {
    const r = this.receipt();
    if (!r) return;
    this.actionLoading.set(true);
    try {
      const updated = await this.collectionApi.reverse(r.id);
      this.setReceipt(updated);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to reverse collection.'));
    } finally {
      this.actionLoading.set(false);
    }
  }

  private async loadReceipt(id: string): Promise<void> {
    this.loading.set(true);
    try {
      const detail = await this.collectionApi.getById(id);
      this.setReceipt(detail);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Collection receipt not found.'));
    } finally {
      this.loading.set(false);
    }
  }

  private setReceipt(detail: CollectionReceiptDetail): void {
    this.receipt.set(detail);
    this.statusLabel.set(collectionStatusLabel(detail.status));
    this.statusVariant.set(collectionStatusVariant(detail.status));
    this.errorMessage.set(null);
  }
}
