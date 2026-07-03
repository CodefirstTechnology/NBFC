import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  DepositAccountDetail,
  DepositAccountStatus,
  DepositApiService,
  depositProductLabel,
  depositProductShort,
  depositStatusLabel,
  depositStatusVariant,
  extractApiErrorMessage,
  formatDepositDate,
  formatInr,
  interestPayoutLabel,
} from '@patsanstha/deposits-data-access';
import { PatsStatusPillComponent } from '@patsanstha/ui-kit';

interface DepositTransactionRow {
  id: string;
  date: string;
  description: string;
  descriptionMr: string;
  type: 'CREDIT' | 'DEBIT';
  amount: number;
}

@Component({
  selector: 'pats-deposit-detail-page',
  standalone: true,
  imports: [RouterLink, PatsStatusPillComponent],
  template: `
    <section class="detail-page">
      <nav class="detail-page__breadcrumb">
        <a routerLink="/deposits">Deposits</a>
        <span class="material-symbols-outlined">chevron_right</span>
        <span>Account Details</span>
      </nav>

      @if (loading()) {
        <p class="detail-page__loading">Loading account…</p>
      } @else if (errorMessage()) {
        <p class="detail-page__error">{{ errorMessage() }}</p>
      } @else {
        @if (account(); as a) {
          <header class="detail-page__hero">
            <div>
              <h1>{{ depositProductLabel(a.productType) }} Detail</h1>
              <p>Managing financial security with Patsanstha Credit Management</p>
            </div>
            <div class="detail-page__hero-actions">
              <button type="button" class="detail-page__primary-btn" (click)="printPassbook()">
                <span class="material-symbols-outlined">print</span>
                Print Passbook
              </button>
              <button
                type="button"
                class="detail-page__danger-btn"
                [disabled]="closing()"
                (click)="closeAccount()">
                <span class="material-symbols-outlined">close</span>
                Close Account
              </button>
            </div>
          </header>

          <div class="detail-page__meta">
            <div>
              <span>Account Number</span>
              <strong>{{ a.accountNumber }}</strong>
            </div>
            <div>
              <span>Product Type</span>
              <strong>
                <span class="detail-page__product-badge">{{ depositProductShort(a.productType) }}</span>
                {{ depositProductLabel(a.productType) }}
              </strong>
            </div>
            <div>
              <span>Member Name</span>
              <strong>{{ a.memberName }}</strong>
            </div>
            <pats-status-pill [label]="statusLabel()" [variant]="statusVariant()" />
          </div>

          <div class="detail-page__stats">
            <article class="detail-page__stat detail-page__stat--green">
              <span class="material-symbols-outlined">account_balance_wallet</span>
              <div>
                <span>Current Balance</span>
                <strong>{{ formatInr(a.currentBalance) }}</strong>
                <small>+{{ a.interestRate.toFixed(1) }}% p.a.</small>
              </div>
            </article>

            <article class="detail-page__stat detail-page__stat--orange">
              <span class="material-symbols-outlined">calculate</span>
              <div>
                <span>Total Interest Accrued</span>
                <strong>{{ formatInr(interestAccrued()) }}</strong>
              </div>
            </article>

            <article class="detail-page__stat detail-page__stat--blue">
              <span class="material-symbols-outlined">event</span>
              <div>
                <span>Maturity Date</span>
                <strong>{{ formatDepositDate(a.maturityDate) }}</strong>
              </div>
            </article>
          </div>

          <article class="detail-page__transactions">
            <header>
              <div>
                <h2>Transaction History</h2>
                <p>Recent account activity</p>
              </div>
              <div class="detail-page__transactions-tools">
                <input
                  type="search"
                  placeholder="Search transactions…"
                  [value]="transactionSearch()"
                  (input)="onTransactionSearch($event)" />
                <button type="button" class="detail-page__icon-btn">
                  <span class="material-symbols-outlined">filter_list</span>
                </button>
              </div>
            </header>

            <div class="detail-page__table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Description</th>
                    <th>Type</th>
                    <th>Amount (₹)</th>
                    <th>Action</th>
                  </tr>
                </thead>
                <tbody>
                  @if (filteredTransactions().length === 0) {
                    <tr>
                      <td colspan="5" class="detail-page__empty">No transactions found.</td>
                    </tr>
                  } @else {
                    @for (txn of filteredTransactions(); track txn.id) {
                      <tr>
                        <td>{{ txn.date }}</td>
                        <td>
                          <strong>{{ txn.description }}</strong>
                          <span>{{ txn.descriptionMr }}</span>
                        </td>
                        <td>
                          <span
                            class="detail-page__txn-type"
                            [class.detail-page__txn-type--credit]="txn.type === 'CREDIT'"
                            [class.detail-page__txn-type--debit]="txn.type === 'DEBIT'">
                            {{ txn.type }}
                          </span>
                        </td>
                        <td
                          [class.detail-page__amount--credit]="txn.type === 'CREDIT'"
                          [class.detail-page__amount--debit]="txn.type === 'DEBIT'">
                          {{ txn.type === 'CREDIT' ? '+' : '-' }} {{ formatInr(txn.amount) }}
                        </td>
                        <td>
                          <button type="button" class="detail-page__icon-btn">
                            <span class="material-symbols-outlined">description</span>
                          </button>
                        </td>
                      </tr>
                    }
                  }
                </tbody>
              </table>
            </div>

            <footer>
              <span>Showing 1 to {{ filteredTransactions().length }} of {{ transactions().length }} transactions</span>
              <div class="detail-page__pagination">
                <button type="button" class="detail-page__page-btn" disabled>Previous</button>
                <button type="button" class="detail-page__page-btn detail-page__page-btn--active">1</button>
                <button type="button" class="detail-page__page-btn" disabled>Next</button>
              </div>
            </footer>
          </article>

          <div class="detail-page__bottom">
            <article class="detail-page__policy">
              <span class="material-symbols-outlined">info</span>
              <div>
                <h3>Account Terms &amp; Conditions</h3>
                <p>
                  This deposit account is subject to society lock-in periods and premature withdrawal penalties.
                  Interest payout mode: {{ interestPayoutLabel(a.interestPayoutMode) }}.
                  Auto-renewal: {{ a.autoRenewal ? 'Enabled' : 'Disabled' }}.
                </p>
                <button type="button" class="detail-page__link">
                  View Full Policy
                  <span class="material-symbols-outlined">open_in_new</span>
                </button>
              </div>
            </article>

            <article class="detail-page__nominee">
              <h3>Nominee Details</h3>
              <dl>
                <div>
                  <dt>Name</dt>
                  <dd>Not recorded on file</dd>
                </div>
                <div>
                  <dt>Relationship</dt>
                  <dd>—</dd>
                </div>
                <div>
                  <dt>Principal</dt>
                  <dd>{{ formatInr(a.principalAmount) }}</dd>
                </div>
                <div>
                  <dt>Opened On</dt>
                  <dd>{{ formatDepositDate(a.openedOn) }}</dd>
                </div>
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
        width: 100%;
      }

      .detail-page__breadcrumb {
        display: flex;
        align-items: center;
        gap: 6px;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
      }

      .detail-page__breadcrumb a {
        color: var(--pats-color-primary-container);
        font-weight: 600;
      }

      .detail-page__hero {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 16px;
        flex-wrap: wrap;
      }

      .detail-page__hero h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 32px;
        color: var(--pats-color-primary);
      }

      .detail-page__hero p {
        margin: 8px 0 0;
        color: var(--pats-color-text-secondary);
        font-size: 14px;
      }

      .detail-page__hero-actions {
        display: flex;
        gap: 12px;
        flex-wrap: wrap;
      }

      .detail-page__primary-btn,
      .detail-page__danger-btn,
      .detail-page__icon-btn,
      .detail-page__page-btn,
      .detail-page__link {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        min-height: 44px;
        padding: 0 16px;
        border-radius: var(--pats-radius-md);
        font-size: 14px;
        font-weight: 600;
        cursor: pointer;
      }

      .detail-page__primary-btn {
        border: none;
        background: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary);
      }

      .detail-page__danger-btn {
        border: 1px solid var(--pats-color-error);
        background: var(--pats-color-surface-container-lowest);
        color: var(--pats-color-error);
      }

      .detail-page__meta {
        display: grid;
        grid-template-columns: repeat(4, minmax(0, 1fr));
        gap: 16px;
        padding: 20px 24px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
        box-shadow: var(--pats-shadow-card);
        align-items: center;
      }

      @media (max-width: 900px) {
        .detail-page__meta {
          grid-template-columns: repeat(2, minmax(0, 1fr));
        }
      }

      .detail-page__meta span {
        display: block;
        margin-bottom: 4px;
        font-size: 12px;
        color: var(--pats-color-text-secondary);
      }

      .detail-page__meta strong {
        font-size: 16px;
      }

      .detail-page__product-badge {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 28px;
        height: 24px;
        margin-right: 8px;
        padding: 0 8px;
        border-radius: var(--pats-radius-full);
        background: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary);
        font-size: 11px;
      }

      .detail-page__stats {
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 16px;
      }

      @media (max-width: 900px) {
        .detail-page__stats {
          grid-template-columns: 1fr;
        }
      }

      .detail-page__stat {
        display: flex;
        gap: 16px;
        padding: 24px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
        box-shadow: var(--pats-shadow-card);
      }

      .detail-page__stat .material-symbols-outlined {
        width: 48px;
        height: 48px;
        border-radius: var(--pats-radius-md);
        display: grid;
        place-items: center;
      }

      .detail-page__stat--green .material-symbols-outlined {
        background: rgba(46, 158, 91, 0.12);
        color: var(--pats-color-success);
      }

      .detail-page__stat--orange .material-symbols-outlined {
        background: rgba(242, 169, 59, 0.15);
        color: var(--pats-color-warning);
      }

      .detail-page__stat--blue .material-symbols-outlined {
        background: rgba(26, 60, 110, 0.12);
        color: var(--pats-color-primary-container);
      }

      .detail-page__stat span {
        display: block;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
      }

      .detail-page__stat strong {
        display: block;
        margin-top: 4px;
        font-family: var(--pats-font-display);
        font-size: 28px;
      }

      .detail-page__stat small {
        display: block;
        margin-top: 4px;
        color: var(--pats-color-success);
        font-size: 12px;
        font-weight: 600;
      }

      .detail-page__transactions {
        padding: 24px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
        box-shadow: var(--pats-shadow-card);
      }

      .detail-page__transactions header {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 16px;
        margin-bottom: 20px;
        flex-wrap: wrap;
      }

      .detail-page__transactions h2 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 22px;
        color: var(--pats-color-primary-container);
      }

      .detail-page__transactions header p {
        margin: 4px 0 0;
        color: var(--pats-color-text-secondary);
        font-size: 13px;
      }

      .detail-page__transactions-tools {
        display: flex;
        gap: 8px;
      }

      .detail-page__transactions-tools input {
        min-width: 220px;
        min-height: 44px;
        padding: 0 14px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
      }

      .detail-page__icon-btn {
        width: 44px;
        padding: 0;
        border: 1px solid var(--pats-color-border-subtle);
        background: var(--pats-color-surface-container-lowest);
        justify-content: center;
      }

      .detail-page__table-wrap {
        overflow: auto;
      }

      .detail-page__transactions table {
        width: 100%;
        border-collapse: collapse;
      }

      .detail-page__transactions th,
      .detail-page__transactions td {
        padding: 14px 12px;
        border-bottom: 1px solid var(--pats-color-border-subtle);
        text-align: left;
        font-size: 14px;
      }

      .detail-page__transactions th {
        font-size: 12px;
        color: var(--pats-color-text-secondary);
        background: var(--pats-color-surface-muted);
      }

      .detail-page__transactions td strong {
        display: block;
      }

      .detail-page__transactions td span {
        display: block;
        margin-top: 4px;
        font-size: 12px;
        color: var(--pats-color-text-secondary);
      }

      .detail-page__txn-type {
        display: inline-flex;
        padding: 4px 10px;
        border-radius: var(--pats-radius-full);
        font-size: 11px;
        font-weight: 700;
      }

      .detail-page__txn-type--credit {
        background: rgba(46, 158, 91, 0.12);
        color: var(--pats-color-success);
      }

      .detail-page__txn-type--debit {
        background: rgba(186, 26, 26, 0.12);
        color: var(--pats-color-error);
      }

      .detail-page__amount--credit {
        color: var(--pats-color-success);
        font-weight: 600;
      }

      .detail-page__amount--debit {
        color: var(--pats-color-error);
        font-weight: 600;
      }

      .detail-page__transactions footer {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 16px;
        margin-top: 16px;
        color: var(--pats-color-text-secondary);
        font-size: 13px;
        flex-wrap: wrap;
      }

      .detail-page__pagination {
        display: flex;
        gap: 6px;
      }

      .detail-page__page-btn {
        min-width: 36px;
        min-height: 36px;
        padding: 0 10px;
        border: 1px solid var(--pats-color-border-subtle);
        background: var(--pats-color-surface-container-lowest);
      }

      .detail-page__page-btn--active {
        background: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary);
        border-color: var(--pats-color-primary-container);
      }

      .detail-page__page-btn:disabled {
        opacity: 0.45;
        cursor: not-allowed;
      }

      .detail-page__bottom {
        display: grid;
        grid-template-columns: 1.2fr 0.8fr;
        gap: 16px;
      }

      @media (max-width: 900px) {
        .detail-page__bottom {
          grid-template-columns: 1fr;
        }
      }

      .detail-page__policy,
      .detail-page__nominee {
        padding: 20px;
        border-radius: var(--pats-radius-lg);
        border: 1px solid var(--pats-color-border-subtle);
        background: var(--pats-color-surface-container-lowest);
      }

      .detail-page__policy {
        display: flex;
        gap: 12px;
        background: rgba(26, 60, 110, 0.06);
      }

      .detail-page__policy .material-symbols-outlined {
        color: var(--pats-color-primary-container);
      }

      .detail-page__policy h3,
      .detail-page__nominee h3 {
        margin: 0 0 8px;
        font-size: 16px;
      }

      .detail-page__policy p {
        margin: 0;
        color: var(--pats-color-text-secondary);
        font-size: 14px;
        line-height: 1.5;
      }

      .detail-page__link {
        margin-top: 12px;
        padding: 0;
        border: none;
        background: none;
        color: var(--pats-color-primary-container);
      }

      .detail-page__nominee dl {
        margin: 0;
        display: grid;
        gap: 12px;
      }

      .detail-page__nominee dt {
        font-size: 11px;
        font-weight: 600;
        text-transform: uppercase;
        color: var(--pats-color-text-secondary);
      }

      .detail-page__nominee dd {
        margin: 4px 0 0;
        font-size: 15px;
      }

      .detail-page__empty,
      .detail-page__loading,
      .detail-page__error {
        font-size: 14px;
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
  readonly closing = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly account = signal<DepositAccountDetail | null>(null);
  readonly statusLabel = signal('');
  readonly statusVariant = signal<'active' | 'inactive' | 'pending' | 'warning' | 'error'>('active');
  readonly transactionSearch = signal('');

  protected readonly formatInr = formatInr;
  protected readonly formatDepositDate = formatDepositDate;
  protected readonly depositProductLabel = depositProductLabel;
  protected readonly depositProductShort = depositProductShort;
  protected readonly interestPayoutLabel = interestPayoutLabel;

  readonly interestAccrued = computed(() => {
    const account = this.account();
    if (!account) {
      return 0;
    }

    return Math.max(0, account.currentBalance - account.principalAmount);
  });

  readonly transactions = computed(() => {
    const account = this.account();
    if (!account) {
      return [] as DepositTransactionRow[];
    }

    const rows: DepositTransactionRow[] = [
      {
        id: 'opening',
        date: this.formatTransactionDate(account.openedOn, '10:30 AM'),
        description: 'Account Opening Deposit',
        descriptionMr: 'खाते उघडण्याची ठेव',
        type: 'CREDIT',
        amount: account.principalAmount,
      },
    ];

    const accrued = this.interestAccrued();
    if (accrued > 0) {
      rows.unshift({
        id: 'interest',
        date: this.formatTransactionDate(new Date().toISOString(), '10:45 AM'),
        description: 'Quarterly Interest Credit',
        descriptionMr: 'त्रैमासिक व्याज जमा',
        type: 'CREDIT',
        amount: accrued,
      });
    }

    return rows;
  });

  readonly filteredTransactions = computed(() => {
    const query = this.transactionSearch().trim().toLowerCase();
    if (!query) {
      return this.transactions();
    }

    return this.transactions().filter(
      (txn) =>
        txn.description.toLowerCase().includes(query) ||
        txn.descriptionMr.toLowerCase().includes(query) ||
        txn.type.toLowerCase().includes(query)
    );
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage.set('Account id is missing.');
      this.loading.set(false);
      return;
    }

    void this.loadAccount(id);
  }

  onTransactionSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.transactionSearch.set(value);
  }

  printPassbook(): void {
    window.print();
  }

  async closeAccount(): Promise<void> {
    const account = this.account();
    if (!account || account.status === DepositAccountStatus.Closed) {
      return;
    }

    this.closing.set(true);
    this.errorMessage.set(null);

    try {
      const updated = await this.depositApi.update(account.id, {
        status: DepositAccountStatus.Closed,
      });
      this.account.set(updated);
      this.statusLabel.set(depositStatusLabel(updated.status));
      this.statusVariant.set(depositStatusVariant(updated.status));
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to close deposit account.'));
    } finally {
      this.closing.set(false);
    }
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

  private formatTransactionDate(value: string, time: string): string {
    const formatted = formatDepositDate(value);
    return formatted === '—' ? formatted : `${formatted}, ${time}`;
  }
}
