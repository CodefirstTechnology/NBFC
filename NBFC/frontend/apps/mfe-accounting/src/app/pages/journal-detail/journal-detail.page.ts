import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HasPermissionDirective } from '@patsanstha/auth';
import {
  AccountingApiService,
  JournalEntryDetail,
  JournalEntryStatus,
  extractApiErrorMessage,
  formatInr,
  journalStatusLabel,
  journalStatusVariant,
} from '@patsanstha/accounting-data-access';
import { PatsButtonComponent, PatsStatusPillComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-journal-detail-page',
  standalone: true,
  imports: [RouterLink, HasPermissionDirective, PatsButtonComponent, PatsStatusPillComponent],
  template: `
    <section class="detail-page">
      <a routerLink="/accounting" class="detail-page__back">
        <span class="material-symbols-outlined">arrow_back</span>
        Back to journal entries
      </a>

      @if (loading()) {
        <p>Loading journal entry…</p>
      } @else if (errorMessage()) {
        <p class="detail-page__error">{{ errorMessage() }}</p>
      } @else {
        @if (entry(); as e) {
          <header class="detail-page__header">
            <div>
              <p class="detail-page__eyebrow">{{ e.entryNumber }}</p>
              <h1>{{ e.description }}</h1>
              <p>{{ e.entryDate }} · {{ formatInr(e.amount) }}</p>
            </div>
            <pats-status-pill [label]="statusLabel()" [variant]="statusVariant()" />
          </header>

          <div class="detail-page__grid">
            <article class="detail-page__card">
              <h2>Accounts</h2>
              <dl>
                <dt>Debit</dt><dd>{{ e.debitAccountCode }}</dd>
                <dt>Credit</dt><dd>{{ e.creditAccountCode }}</dd>
                <dt>Amount</dt><dd>{{ formatInr(e.amount) }}</dd>
              </dl>
            </article>

            <article class="detail-page__card">
              <h2>Reference</h2>
              <dl>
                <dt>Type</dt><dd>{{ e.referenceType ?? '—' }}</dd>
                <dt>Id</dt><dd>{{ e.referenceId ?? '—' }}</dd>
                <dt>Created</dt><dd>{{ e.createdAt }}</dd>
                <dt>Modified</dt><dd>{{ e.modifiedAt ?? '—' }}</dd>
              </dl>
            </article>
          </div>

          @if (e.status === draftStatus) {
            <div class="detail-page__actions">
              <pats-button
                *patsHasPermission="'accounting.post'"
                [loading]="actionLoading()"
                (clicked)="postEntry()">
                Post Entry
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
export class JournalDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly accountingApi = inject(AccountingApiService);

  readonly loading = signal(true);
  readonly actionLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly entry = signal<JournalEntryDetail | null>(null);
  readonly statusLabel = signal('');
  readonly statusVariant = signal<'active' | 'inactive' | 'pending' | 'warning' | 'error'>('pending');

  readonly draftStatus = JournalEntryStatus.Draft;

  protected readonly formatInr = formatInr;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage.set('Journal entry id missing.');
      this.loading.set(false);
      return;
    }
    void this.loadEntry(id);
  }

  async postEntry(): Promise<void> {
    const e = this.entry();
    if (!e) return;

    this.actionLoading.set(true);
    try {
      const updated = await this.accountingApi.post(e.id);
      this.setEntry(updated);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to post journal entry.'));
    } finally {
      this.actionLoading.set(false);
    }
  }

  private async loadEntry(id: string): Promise<void> {
    this.loading.set(true);
    try {
      const detail = await this.accountingApi.getById(id);
      this.setEntry(detail);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Journal entry not found.'));
    } finally {
      this.loading.set(false);
    }
  }

  private setEntry(detail: JournalEntryDetail): void {
    this.entry.set(detail);
    this.statusLabel.set(journalStatusLabel(detail.status));
    this.statusVariant.set(journalStatusVariant(detail.status));
    this.errorMessage.set(null);
  }
}
