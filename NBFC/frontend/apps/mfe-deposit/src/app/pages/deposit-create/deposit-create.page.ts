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
} from '@patsanstha/deposits-data-access';
import { PatsButtonComponent, PatsFormFieldComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-deposit-create-page',
  standalone: true,
  imports: [FormsModule, RouterLink, PatsButtonComponent, PatsFormFieldComponent],
  template: `
    <section class="create-page">
      <header class="create-page__header">
        <a routerLink="/deposits" class="create-page__back">
          <span class="material-symbols-outlined">arrow_back</span>
          Back to directory
        </a>
        <h1>New Deposit Account <span class="create-page__subtitle">/ नवीन ठेव खाते</span></h1>
        <p>Open a savings, RD, or FD account for an existing member.</p>
      </header>

      <div class="create-page__steps">
        <span [class.create-page__step--active]="step() >= 1">1. Select Member</span>
        <span [class.create-page__step--active]="step() >= 2">2. Product Type</span>
        <span [class.create-page__step--active]="step() >= 3">3. Financial Details</span>
      </div>

      @if (errorMessage()) {
        <p class="create-page__error">{{ errorMessage() }}</p>
      }

      @if (step() === 1) {
        <article class="create-page__card">
          <h2>Identify Member</h2>
          <p>Search by member number, name, or mobile.</p>
          <input
            class="create-page__search"
            type="search"
            placeholder="Search member…"
            [ngModel]="memberSearch()"
            (ngModelChange)="searchMembers($event)" />

          @if (memberSearchLoading()) {
            <p>Searching…</p>
          }

          @if (memberResults().length > 0) {
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
            <div class="create-page__selected">
              <span class="material-symbols-outlined">verified</span>
              <div>
                <strong>{{ member.fullName }}</strong>
                <span>{{ member.memberNumber }}</span>
              </div>
            </div>
            <div class="create-page__actions">
              <pats-button [disabled]="!selectedMember()" (clicked)="step.set(2)">
                Next: Product Type
              </pats-button>
            </div>
          }
        </article>
      }

      @if (step() === 2) {
        <article class="create-page__card">
          <h2>Choose Account Type</h2>
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
            <pats-button variant="ghost" (clicked)="step.set(1)">Back</pats-button>
            <pats-button [disabled]="!selectedProduct()" (clicked)="step.set(3)">
              Next: Financial Details
            </pats-button>
          </div>
        </article>
      }

      @if (step() === 3) {
        <article class="create-page__card">
          <h2>Financial Setup</h2>
          <div class="create-page__form">
            <pats-form-field label="Principal Amount (₹)">
              <input type="number" min="1" step="0.01" [(ngModel)]="principalAmount" />
            </pats-form-field>

            @if (requiresTenure()) {
              <pats-form-field label="Tenure (Months)">
                <select [(ngModel)]="tenureMonths">
                  <option [ngValue]="12">12 Months</option>
                  <option [ngValue]="24">24 Months</option>
                  <option [ngValue]="36">36 Months</option>
                  <option [ngValue]="60">60 Months</option>
                </select>
              </pats-form-field>
            }

            <pats-form-field label="Interest Payout">
              <select [(ngModel)]="interestPayoutMode">
                <option [ngValue]="0">On Maturity</option>
                <option [ngValue]="1">Monthly</option>
              </select>
            </pats-form-field>

            <label class="create-page__checkbox">
              <input type="checkbox" [(ngModel)]="autoRenewal" />
              Auto-renew principal + interest on maturity
            </label>
          </div>

          <div class="create-page__actions">
            <pats-button variant="ghost" type="button" (clicked)="cancel()">Cancel</pats-button>
            <pats-button variant="ghost" (clicked)="step.set(2)">Back</pats-button>
            <pats-button [loading]="loading()" (clicked)="submit()">Open Account</pats-button>
          </div>
        </article>
      }
    </section>
  `,
  styles: [
    `
      .create-page {
        display: flex;
        flex-direction: column;
        gap: 24px;
        width: 100%;
        max-width: var(--pats-form-max-width);
      }

      .create-page__back {
        display: inline-flex;
        align-items: center;
        gap: 4px;
        color: var(--pats-color-primary-container);
        font-size: 14px;
        font-weight: 600;
        margin-bottom: 8px;
      }

      .create-page__header h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 32px;
      }

      .create-page__subtitle {
        font-size: 18px;
        color: var(--pats-color-text-secondary);
      }

      .create-page__steps {
        display: flex;
        gap: 16px;
        font-size: 13px;
        font-weight: 600;
        color: var(--pats-color-text-secondary);
      }

      .create-page__step--active {
        color: var(--pats-color-primary-container);
      }

      .create-page__card {
        padding: 24px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
        box-shadow: var(--pats-shadow-card);
      }

      .create-page__card h2 {
        margin: 0 0 8px;
        font-family: var(--pats-font-display);
        color: var(--pats-color-primary-container);
      }

      .create-page__search {
        width: 100%;
        min-height: 44px;
        padding: 0 16px;
        margin-top: 16px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
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

      .create-page__selected {
        display: flex;
        align-items: center;
        gap: 12px;
        margin-top: 16px;
        padding: 16px;
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-surface-muted);
        color: var(--pats-color-secondary);
      }

      .create-page__products {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        gap: 16px;
        margin-top: 16px;
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
        border-color: var(--pats-color-primary);
        background: var(--pats-color-surface-container-low);
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
        margin-top: 16px;
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
      }

      .create-page__error {
        color: var(--pats-color-error);
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
  readonly step = signal(1);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly memberSearch = signal('');
  readonly memberSearchLoading = signal(false);
  readonly memberResults = signal<MemberSummary[]>([]);
  readonly selectedMember = signal<MemberSummary | null>(null);
  readonly selectedProduct = signal<ProductRateInfo | null>(null);

  principalAmount = 50000;
  tenureMonths = 36;
  interestPayoutMode = InterestPayoutMode.OnMaturity;
  autoRenewal = false;

  readonly requiresTenure = computed(() => {
    const product = this.selectedProduct()?.productType;
    return (
      product === DepositProductType.RecurringDeposit ||
      product === DepositProductType.FixedDeposit
    );
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
    this.errorMessage.set(null);
  }

  selectProduct(product: ProductRateInfo): void {
    this.selectedProduct.set(product);
    this.errorMessage.set(null);
  }

  async submit(): Promise<void> {
    const member = this.selectedMember();
    const product = this.selectedProduct();

    if (!member || !product) {
      this.errorMessage.set('Select a member and product type.');
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
        principalAmount: this.principalAmount,
        tenureMonths: this.requiresTenure() ? this.tenureMonths : null,
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
