import { Component, inject, signal, computed } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '@patsanstha/auth';
import { MemberApiService, MemberSummary, extractApiErrorMessage as memberError } from '@patsanstha/members-data-access';
import {
  DEPOSIT_PRODUCTS,
  DepositApiService,
  DepositProductType,
  InterestPayoutMode,
  ProductRateInfo,
  extractApiErrorMessage,
  formatInr,
} from '@patsanstha/deposits-data-access';
import {
  DepositCalcProductType,
  estimateDepositMaturity,
} from './deposit-maturity.calc';

@Component({
  selector: 'pats-deposit-create-page',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <section class="create-page">
      <header class="create-page__header">
        <a routerLink="/deposits" class="create-page__back">
          <span class="material-symbols-outlined">arrow_back</span>
          Back to directory
        </a>
        <h1>New Deposit Account</h1>
        <p class="create-page__subtitle">नवीन ठेव खाते — Open a savings, RD, or FD account for an existing member</p>
      </header>

      <nav class="create-page__stepper" aria-label="Deposit opening steps">
        <div class="create-page__stepper-line"></div>
        @for (item of steps; track item.id) {
          <div
            class="create-page__step"
            [class.create-page__step--active]="step() === item.id"
            [class.create-page__step--done]="step() > item.id">
            <div class="create-page__step-circle">{{ item.id }}</div>
            <span>{{ item.label }}</span>
          </div>
        }
      </nav>

      @if (errorMessage()) {
        <p class="create-page__error">{{ errorMessage() }}</p>
      }

      <div class="create-page__layout">
        <div class="create-page__main">
          @if (step() === 1) {
            <article class="create-page__card">
              <h2>Identify Member</h2>
              <p>Search by Member ID, Aadhaar, or Name</p>

              <div class="create-page__search-wrap">
                <span class="material-symbols-outlined">search</span>
                <input
                  class="create-page__search"
                  type="search"
                  placeholder="Search Member (e.g. MBR-2024-001)"
                  [ngModel]="memberSearch()"
                  (ngModelChange)="searchMembers($event)" />
                <kbd>Cmd K</kbd>
              </div>

              @if (memberSearchLoading()) {
                <p class="create-page__hint">Searching…</p>
              }

              @if (memberResults().length > 0 && !selectedMember()) {
                <ul class="create-page__member-list">
                  @for (member of memberResults(); track member.id) {
                    <li>
                      <button type="button" (click)="selectMember(member)">
                        <strong>{{ member.fullName }}</strong>
                        <span>{{ member.memberNumber }} · {{ member.mobileNumber }}</span>
                      </button>
                    </li>
                  }
                </ul>
              }

              @if (selectedMember(); as member) {
                <div class="create-page__member-card">
                  <div>
                    <strong>{{ member.fullName }}</strong>
                    <span>{{ member.memberNumber }}</span>
                    <span>Branch member account</span>
                  </div>
                  <span class="create-page__verified">Verified Member</span>
                </div>
              }

              <div class="create-page__actions">
                <button
                  type="button"
                  class="create-page__primary-btn"
                  [disabled]="!selectedMember()"
                  (click)="step.set(2)">
                  Next: Product Type
                  <span class="material-symbols-outlined">arrow_forward</span>
                </button>
              </div>
            </article>
          }

          @if (step() === 2) {
            <article class="create-page__card">
              <h2>Choose Account Type</h2>
              <p>Select the deposit product for this member</p>

              <div class="create-page__products">
                @for (product of products; track product.productType) {
                  <button
                    type="button"
                    class="create-page__product"
                    [class.create-page__product--selected]="selectedProduct()?.productType === product.productType"
                    (click)="selectProduct(product)">
                    <span class="material-symbols-outlined">{{ product.icon }}</span>
                    <h3>{{ product.label }}</h3>
                    <p>{{ product.description }}</p>
                    <strong>{{ product.rate }}% <small>p.a.</small></strong>
                  </button>
                }
              </div>

              <div class="create-page__actions">
                <button type="button" class="create-page__ghost-btn" (click)="step.set(1)">Back</button>
                <button
                  type="button"
                  class="create-page__primary-btn"
                  [disabled]="!selectedProduct()"
                  (click)="step.set(3)">
                  Next: Financial Details
                  <span class="material-symbols-outlined">arrow_forward</span>
                </button>
              </div>
            </article>
          }

          @if (step() === 3) {
            <article class="create-page__card">
              <h2>Financial Setup</h2>
              <p>Configure principal, tenure, and payout preferences</p>

              <div class="create-page__form">
                <label class="create-page__field">
                  <span>{{ amountLabel() }}</span>
                  <input
                    type="number"
                    min="1"
                    step="0.01"
                    placeholder="Enter amount"
                    [ngModel]="principalAmount()"
                    (ngModelChange)="onPrincipalChange($event)" />
                </label>

                @if (requiresTenure()) {
                  <label class="create-page__field">
                    <span>Tenure (Months)</span>
                    <select
                      [ngModel]="tenureMonths()"
                      (ngModelChange)="onTenureChange($event)">
                      <option [ngValue]="null">Select tenure</option>
                      <option [ngValue]="12">12 Months</option>
                      <option [ngValue]="24">24 Months</option>
                      <option [ngValue]="36">36 Months</option>
                      <option [ngValue]="60">60 Months</option>
                    </select>
                  </label>
                }

                <label class="create-page__field">
                  <span>Interest Payout</span>
                  <select [(ngModel)]="interestPayoutMode">
                    <option [ngValue]="0">On Maturity</option>
                    <option [ngValue]="1">Monthly</option>
                  </select>
                </label>

                <label class="create-page__checkbox">
                  <input type="checkbox" [(ngModel)]="autoRenewal" />
                  Auto-renew principal + interest on maturity
                </label>
              </div>

              <div class="create-page__actions">
                <button type="button" class="create-page__ghost-btn" (click)="cancel()">Cancel</button>
                <button type="button" class="create-page__ghost-btn" (click)="step.set(2)">Back</button>
                <button
                  type="button"
                  class="create-page__primary-btn"
                  [disabled]="loading()"
                  (click)="submit()">
                  {{ loading() ? 'Opening…' : 'Open Account' }}
                </button>
              </div>
            </article>
          }
        </div>

        <aside class="create-page__aside">
          <article class="create-page__preview">
            <span class="create-page__preview-label">Interest Rate</span>
            <strong>{{ previewRate() }}%</strong>
            <span class="create-page__preview-label">{{ previewInvestedLabel() }}</span>
            <strong>{{ formatInr(preview().totalInvested) }}</strong>
            <span class="create-page__preview-label">Total Interest</span>
            <strong>{{ formatInr(preview().interest) }}</strong>
            <span class="create-page__preview-label">{{ previewMaturityLabel() }}</span>
            <strong class="create-page__preview-total">{{ formatInr(preview().maturity) }}</strong>
            <footer>
              <span>Product: {{ previewProductLabel() }}</span>
              <span>Tenure: {{ previewTenureLabel() }}</span>
            </footer>
            <p class="create-page__preview-note">{{ preview().formulaLabel }}</p>
          </article>

          <article class="create-page__checklist">
            <h3>
              <span class="material-symbols-outlined">info</span>
              Compliance Checklist
            </h3>
            <ul>
              <li [class.create-page__check--done]="!!selectedMember()">
                <span class="material-symbols-outlined">
                  {{ selectedMember() ? 'check_circle' : 'radio_button_unchecked' }}
                </span>
                KYC Verification Status: Valid
              </li>
              <li [class.create-page__check--done]="!!selectedMember()">
                <span class="material-symbols-outlined">
                  {{ selectedMember() ? 'check_circle' : 'radio_button_unchecked' }}
                </span>
                PAN Card Linked
              </li>
              <li [class.create-page__check--done]="step() >= 3">
                <span class="material-symbols-outlined">
                  {{ step() >= 3 ? 'check_circle' : 'radio_button_unchecked' }}
                </span>
                Nominee Details Updated
              </li>
            </ul>
          </article>
        </aside>
      </div>
    </section>
  `,
  styles: [
    `
      .create-page {
        display: flex;
        flex-direction: column;
        gap: 24px;
        width: 100%;
      }

      .create-page__back {
        display: inline-flex;
        align-items: center;
        gap: 4px;
        margin-bottom: 12px;
        color: var(--pats-color-primary-container);
        font-size: 14px;
        font-weight: 600;
      }

      .create-page__header h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 32px;
        color: var(--pats-color-primary);
      }

      .create-page__subtitle {
        margin: 8px 0 0;
        color: var(--pats-color-text-secondary);
        font-size: 14px;
      }

      .create-page__stepper {
        position: relative;
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 16px;
        padding: 8px 0 24px;
      }

      .create-page__stepper-line {
        position: absolute;
        top: 24px;
        left: 10%;
        right: 10%;
        height: 2px;
        background: var(--pats-color-border-subtle);
      }

      .create-page__step {
        position: relative;
        z-index: 1;
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 8px;
        color: var(--pats-color-text-secondary);
        font-size: 13px;
        font-weight: 600;
      }

      .create-page__step-circle {
        width: 40px;
        height: 40px;
        border-radius: 50%;
        display: grid;
        place-items: center;
        background: var(--pats-color-surface-container);
        border: 2px solid var(--pats-color-border-subtle);
        font-weight: 700;
      }

      .create-page__step--active,
      .create-page__step--done {
        color: var(--pats-color-primary-container);
      }

      .create-page__step--active .create-page__step-circle,
      .create-page__step--done .create-page__step-circle {
        background: var(--pats-color-primary-container);
        border-color: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary);
      }

      .create-page__layout {
        display: grid;
        grid-template-columns: minmax(0, 1.4fr) minmax(280px, 0.8fr);
        gap: 24px;
        align-items: start;
      }

      @media (max-width: 960px) {
        .create-page__layout {
          grid-template-columns: 1fr;
        }
      }

      .create-page__card {
        padding: 28px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
        box-shadow: var(--pats-shadow-card);
      }

      .create-page__card h2 {
        margin: 0 0 6px;
        font-family: var(--pats-font-display);
        font-size: 24px;
        color: var(--pats-color-primary-container);
      }

      .create-page__card > p {
        margin: 0 0 20px;
        color: var(--pats-color-text-secondary);
        font-size: 14px;
      }

      .create-page__search-wrap {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 0 16px;
        min-height: 52px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-surface-muted);
      }

      .create-page__search-wrap kbd {
        margin-left: auto;
        padding: 4px 8px;
        border-radius: 6px;
        background: var(--pats-color-surface-container);
        font-size: 11px;
        color: var(--pats-color-text-secondary);
      }

      .create-page__search {
        flex: 1;
        border: none;
        background: transparent;
        min-height: 44px;
        font-size: 14px;
        outline: none;
      }

      .create-page__member-list {
        list-style: none;
        margin: 16px 0 0;
        padding: 0;
        display: flex;
        flex-direction: column;
        gap: 8px;
      }

      .create-page__member-list button {
        width: 100%;
        text-align: left;
        padding: 12px 16px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-surface-muted);
        cursor: pointer;
        display: flex;
        flex-direction: column;
        gap: 4px;
      }

      .create-page__member-card {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 16px;
        margin-top: 20px;
        padding: 20px;
        border-radius: var(--pats-radius-md);
        border: 1px solid var(--pats-color-border-subtle);
        background: var(--pats-color-surface-muted);
      }

      .create-page__member-card strong {
        display: block;
        font-size: 16px;
      }

      .create-page__member-card span {
        display: block;
        margin-top: 4px;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
      }

      .create-page__verified {
        padding: 6px 12px;
        border-radius: var(--pats-radius-full);
        background: rgba(46, 158, 91, 0.12);
        color: var(--pats-color-success);
        font-size: 12px;
        font-weight: 700;
        white-space: nowrap;
      }

      .create-page__products {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
        gap: 16px;
      }

      .create-page__product {
        padding: 20px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        cursor: pointer;
        text-align: left;
        display: flex;
        flex-direction: column;
        gap: 8px;
      }

      .create-page__product--selected {
        border-color: var(--pats-color-primary-container);
        background: rgba(26, 60, 110, 0.06);
      }

      .create-page__product h3 {
        margin: 0;
        font-size: 15px;
      }

      .create-page__product p {
        margin: 0;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
      }

      .create-page__form {
        display: grid;
        gap: 16px;
      }

      .create-page__field {
        display: flex;
        flex-direction: column;
        gap: 8px;
        font-size: 13px;
        font-weight: 600;
      }

      .create-page__field input,
      .create-page__field select {
        min-height: 44px;
        padding: 0 14px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        font-size: 14px;
      }

      .create-page__checkbox {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 14px;
      }

      .create-page__actions {
        display: flex;
        justify-content: flex-end;
        gap: 12px;
        margin-top: 24px;
        flex-wrap: wrap;
      }

      .create-page__primary-btn,
      .create-page__ghost-btn {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        min-height: 44px;
        padding: 0 18px;
        border-radius: var(--pats-radius-md);
        font-size: 14px;
        font-weight: 600;
        cursor: pointer;
      }

      .create-page__primary-btn {
        border: none;
        background: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary);
      }

      .create-page__primary-btn:disabled {
        opacity: 0.55;
        cursor: not-allowed;
      }

      .create-page__ghost-btn {
        border: 1px solid var(--pats-color-border-subtle);
        background: var(--pats-color-surface-container-lowest);
        color: var(--pats-color-on-surface);
      }

      .create-page__aside {
        display: flex;
        flex-direction: column;
        gap: 16px;
      }

      .create-page__preview {
        padding: 24px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary);
        display: flex;
        flex-direction: column;
        gap: 8px;
      }

      .create-page__preview-label {
        font-size: 12px;
        opacity: 0.85;
      }

      .create-page__preview strong {
        font-family: var(--pats-font-display);
        font-size: 22px;
      }

      .create-page__preview-total {
        font-size: 32px !important;
      }

      .create-page__preview footer {
        display: flex;
        flex-direction: column;
        gap: 4px;
        margin-top: 12px;
        padding-top: 12px;
        border-top: 1px solid rgba(255, 255, 255, 0.2);
        font-size: 12px;
        opacity: 0.9;
      }

      .create-page__checklist {
        padding: 20px;
        border-radius: var(--pats-radius-lg);
        border: 1px solid var(--pats-color-border-subtle);
        background: var(--pats-color-surface-container-low);
      }

      .create-page__checklist h3 {
        display: flex;
        align-items: center;
        gap: 8px;
        margin: 0 0 16px;
        font-size: 15px;
      }

      .create-page__checklist ul {
        list-style: none;
        margin: 0;
        padding: 0;
        display: grid;
        gap: 12px;
      }

      .create-page__checklist li {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
      }

      .create-page__check--done {
        color: var(--pats-color-success);
      }

      .create-page__hint,
      .create-page__error {
        font-size: 14px;
      }

      .create-page__error {
        color: var(--pats-color-error);
      }

      .create-page__preview-note {
        margin: 12px 0 0;
        font-size: 12px;
        line-height: 1.4;
        color: var(--pats-color-text-secondary);
      }
    `,
  ],
})
export class DepositCreatePageComponent {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly depositApi = inject(DepositApiService);
  private readonly memberApi = inject(MemberApiService);

  readonly products = DEPOSIT_PRODUCTS;
  readonly steps = [
    { id: 1, label: 'Select Member' },
    { id: 2, label: 'Product Type' },
    { id: 3, label: 'Financial Details' },
  ];

  readonly step = signal(1);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly memberSearch = signal('');
  readonly memberSearchLoading = signal(false);
  readonly memberResults = signal<MemberSummary[]>([]);
  readonly selectedMember = signal<MemberSummary | null>(null);
  readonly selectedProduct = signal<ProductRateInfo | null>(null);
  readonly principalAmount = signal<number | null>(null);
  readonly tenureMonths = signal<number | null>(null);

  interestPayoutMode = InterestPayoutMode.OnMaturity;
  autoRenewal = false;

  protected readonly formatInr = formatInr;

  readonly requiresTenure = computed(() => {
    const product = this.selectedProduct()?.productType;
    return (
      product === DepositProductType.RecurringDeposit ||
      product === DepositProductType.FixedDeposit
    );
  });

  readonly amountLabel = computed(() =>
    this.selectedProduct()?.productType === DepositProductType.RecurringDeposit
      ? 'Monthly Installment (₹)'
      : 'Principal Amount (₹)'
  );

  readonly previewRate = computed(() => this.selectedProduct()?.rate ?? 0);

  readonly previewProductLabel = computed(
    () => this.selectedProduct()?.label ?? 'Select a product'
  );

  readonly previewTenureLabel = computed(() => {
    if (!this.requiresTenure()) {
      return 'Flexible';
    }

    const tenure = this.tenureMonths();
    return tenure ? `${tenure} Months` : '—';
  });

  readonly previewInvestedLabel = computed(() =>
    this.selectedProduct()?.productType === DepositProductType.RecurringDeposit
      ? 'Total Installments'
      : 'Principal'
  );

  readonly previewMaturityLabel = computed(() =>
    this.selectedProduct()?.productType === DepositProductType.Savings
      ? 'Projected Value (1 year)'
      : 'Maturity Amount'
  );

  readonly preview = computed(() => {
    const product = this.selectedProduct();
    const amount = this.principalAmount();
    const tenure = this.tenureMonths();

    if (!product || amount == null || amount <= 0) {
      return estimateDepositMaturity({
        productType: DepositCalcProductType.Savings,
        amount: 0,
        annualRatePercent: 0,
        tenureMonths: null,
      });
    }

    return estimateDepositMaturity({
      productType: product.productType as unknown as DepositCalcProductType,
      amount,
      annualRatePercent: product.rate,
      tenureMonths: this.requiresTenure() ? tenure : null,
    });
  });

  async searchMembers(query: string): Promise<void> {
    this.memberSearch.set(query);
    if (query.trim().length < 2) {
      this.memberResults.set([]);
      return;
    }

    this.memberSearchLoading.set(true);
    try {
      const response = await this.memberApi.list({ search: query, pageSize: 8 });
      this.memberResults.set(response.items);
    } catch (error) {
      this.errorMessage.set(memberError(error, 'Member search failed.'));
    } finally {
      this.memberSearchLoading.set(false);
    }
  }

  selectMember(member: MemberSummary): void {
    this.selectedMember.set(member);
    this.memberResults.set([]);
    this.errorMessage.set(null);
  }

  selectProduct(product: ProductRateInfo): void {
    this.selectedProduct.set(product);
    this.principalAmount.set(null);
    this.tenureMonths.set(null);
    this.errorMessage.set(null);
  }

  onPrincipalChange(value: string | number | null): void {
    if (value === null || value === undefined || value === '') {
      this.principalAmount.set(null);
      return;
    }

    const parsed = typeof value === 'number' ? value : parseFloat(value);
    this.principalAmount.set(Number.isFinite(parsed) ? parsed : null);
  }

  onTenureChange(value: number | null): void {
    this.tenureMonths.set(value);
  }

  async submit(): Promise<void> {
    const member = this.selectedMember();
    const product = this.selectedProduct();
    const amount = this.principalAmount();
    const tenure = this.tenureMonths();

    if (!member || !product) {
      this.errorMessage.set('Select a member and product type.');
      return;
    }

    if (amount == null || amount <= 0) {
      this.errorMessage.set('Enter a valid amount.');
      return;
    }

    if (this.requiresTenure() && (tenure == null || tenure <= 0)) {
      this.errorMessage.set('Select a tenure for this product.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    const branchId =
      this.auth.user()?.branchId ?? '00000000-0000-0000-0000-000000000010';

    try {
      const detail = await this.depositApi.create({
        memberId: member.id,
        memberNumber: member.memberNumber,
        memberName: member.fullName,
        branchId,
        productType: product.productType,
        principalAmount: amount,
        tenureMonths: this.requiresTenure() ? tenure : null,
        interestPayoutMode: this.interestPayoutMode,
        autoRenewal: this.autoRenewal,
      });

      void this.router.navigate(['/deposits', detail.id]);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to open deposit account.'));
    } finally {
      this.loading.set(false);
    }
  }

  cancel(): void {
    void this.router.navigate(['/deposits']);
  }
}
