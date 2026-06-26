import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  DepositAccountDetail,
  DepositApiService,
  depositProductLabel,
  depositStatusLabel,
  depositStatusVariant,
  extractApiErrorMessage,
  formatInr,
  interestPayoutLabel,
} from '@patsanstha/deposits-data-access';
import { PatsStatusPillComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-deposit-detail-page',
  standalone: true,
  imports: [RouterLink, PatsStatusPillComponent],
  template: `
    <section class="detail-page">
      <a routerLink="/deposits" class="detail-page__back">
        <span class="material-symbols-outlined">arrow_back</span>
        Back to directory
      </a>

      @if (loading()) {
        <p>Loading account…</p>
      } @else if (errorMessage()) {
        <p class="detail-page__error">{{ errorMessage() }}</p>
      } @else {
        @if (account(); as a) {
          <header class="detail-page__header">
            <div>
              <p class="detail-page__eyebrow">{{ a.accountNumber }}</p>
              <h1>{{ a.memberName }}</h1>
              <p>{{ depositProductLabel(a.productType) }} · {{ a.memberNumber }}</p>
            </div>
            <pats-status-pill [label]="statusLabel()" [variant]="statusVariant()" />
          </header>

          <div class="detail-page__balance">
            <span>Current Balance</span>
            <strong>{{ formatInr(a.currentBalance) }}</strong>
          </div>

          <div class="detail-page__grid">
            <article class="detail-page__card">
              <h2>Account Details</h2>
              <dl>
                <dt>Principal</dt><dd>{{ formatInr(a.principalAmount) }}</dd>
                <dt>Interest Rate</dt><dd>{{ a.interestRate }}% p.a.</dd>
                <dt>Opened On</dt><dd>{{ a.openedOn }}</dd>
                <dt>Maturity Date</dt><dd>{{ a.maturityDate ?? '—' }}</dd>
              </dl>
            </article>

            <article class="detail-page__card">
              <h2>Terms</h2>
              <dl>
                <dt>Tenure</dt><dd>{{ a.tenureMonths ? a.tenureMonths + ' months' : 'Flexible' }}</dd>
                <dt>Interest Payout</dt><dd>{{ interestPayoutLabel(a.interestPayoutMode) }}</dd>
                <dt>Auto Renewal</dt><dd>{{ a.autoRenewal ? 'Yes' : 'No' }}</dd>
                <dt>Branch ID</dt><dd>{{ a.branchId }}</dd>
              </dl>
            </article>
          </div>
        }
      }
    </section>
  `,
  styles: [
    `
      .detail-page {
        display: flex;
        flex-direction: column;
        gap: 24px;
      }

      .detail-page__back {
        display: inline-flex;
        align-items: center;
        gap: 4px;
        color: var(--pats-color-primary-container);
        font-size: 14px;
        font-weight: 600;
      }

      .detail-page__header {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 16px;
        padding: 24px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
        border-top: 4px solid var(--pats-color-primary-container);
        box-shadow: var(--pats-shadow-card);
      }

      .detail-page__eyebrow {
        margin: 0 0 4px;
        font-size: 13px;
        font-weight: 600;
        color: var(--pats-color-text-secondary);
        text-transform: uppercase;
        letter-spacing: 0.04em;
      }

      .detail-page__header h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 32px;
      }

      .detail-page__balance {
        padding: 24px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-primary);
        color: var(--pats-color-on-primary);
        display: flex;
        flex-direction: column;
        gap: 8px;
      }

      .detail-page__balance strong {
        font-family: var(--pats-font-display);
        font-size: 36px;
      }

      .detail-page__grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
        gap: 16px;
      }

      .detail-page__card {
        padding: 24px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
      }

      .detail-page__card h2 {
        margin: 0 0 16px;
        font-family: var(--pats-font-display);
        font-size: 18px;
        color: var(--pats-color-primary-container);
      }

      dl {
        margin: 0;
        display: grid;
        gap: 12px;
      }

      dt {
        font-size: 11px;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: 0.04em;
        color: var(--pats-color-text-secondary);
      }

      dd {
        margin: 0;
        font-size: 15px;
      }

      .detail-page__error {
        color: var(--pats-color-error);
      }
    `,
  ],
})
export class DepositDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly depositApi = inject(DepositApiService);

  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly account = signal<DepositAccountDetail | null>(null);
  readonly statusLabel = signal('');
  readonly statusVariant = signal<'active' | 'inactive' | 'pending' | 'warning' | 'error'>('active');

  protected readonly formatInr = formatInr;
  protected readonly depositProductLabel = depositProductLabel;
  protected readonly interestPayoutLabel = interestPayoutLabel;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage.set('Account id is missing.');
      this.loading.set(false);
      return;
    }

    void this.loadAccount(id);
  }

  private async loadAccount(id: string): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);

    try {
      const detail = await this.depositApi.getById(id);
      this.account.set(detail);
      this.statusLabel.set(depositStatusLabel(detail.status));
      this.statusVariant.set(depositStatusVariant(detail.status));
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Deposit account not found.'));
    } finally {
      this.loading.set(false);
    }
  }
}
