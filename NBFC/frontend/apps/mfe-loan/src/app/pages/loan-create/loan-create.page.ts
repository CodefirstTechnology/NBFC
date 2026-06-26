import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '@patsanstha/auth';
import { previewEmi, formatInr as formatInrUtil } from '@patsanstha/core-utils';
import { MemberApiService, MemberSummary, extractApiErrorMessage as memberError } from '@patsanstha/members-data-access';
import {
  LOAN_PRODUCTS,
  LoanApiService,
  LoanProductType,
  extractApiErrorMessage,
} from '@patsanstha/loans-data-access';
import { PatsButtonComponent, PatsFormFieldComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-loan-create-page',
  standalone: true,
  imports: [FormsModule, RouterLink, PatsButtonComponent, PatsFormFieldComponent],
  template: `
    <section class="create-page">
      <header class="create-page__header">
        <a routerLink="/loans" class="create-page__back">
          <span class="material-symbols-outlined">arrow_back</span>
          Back to queue
        </a>
        <h1>New Loan Application</h1>
        <p>Submit a loan application for an existing member. EMI shown is a preview only.</p>
      </header>

      @if (errorMessage()) {
        <p class="create-page__error">{{ errorMessage() }}</p>
      }

      <article class="create-page__card">
        <h2>Member</h2>
        <input class="create-page__search" type="search" placeholder="Search member…" [ngModel]="memberSearch()" (ngModelChange)="searchMembers($event)" />
        @if (memberResults().length > 0) {
          <ul class="create-page__member-list">
            @for (member of memberResults(); track member.id) {
              <li><button type="button" (click)="selectMember(member)"><strong>{{ member.fullName }}</strong><span>{{ member.memberNumber }}</span></button></li>
            }
          </ul>
        }
        @if (selectedMember(); as member) {
          <p class="create-page__selected">Selected: <strong>{{ member.fullName }}</strong> ({{ member.memberNumber }})</p>
        }
      </article>

      <article class="create-page__card">
        <h2>Product &amp; Terms</h2>
        <div class="create-page__products">
          @for (product of products; track product.productType) {
            <button type="button" class="create-page__product" [class.create-page__product--selected]="selectedProduct() === product.productType" (click)="selectedProduct.set(product.productType)">
              <span class="material-symbols-outlined">{{ product.icon }}</span>
              <strong>{{ product.label }}</strong>
              <span>{{ product.rate }}% p.a.</span>
            </button>
          }
        </div>
        <div class="create-page__form">
          <pats-form-field label="Requested Amount (₹)"><input type="number" min="1" [(ngModel)]="requestedAmount" (ngModelChange)="updatePreview()" /></pats-form-field>
          <pats-form-field label="Tenure (Months)"><input type="number" min="1" max="360" [(ngModel)]="tenureMonths" (ngModelChange)="updatePreview()" /></pats-form-field>
          <pats-form-field label="Purpose"><textarea rows="3" [(ngModel)]="purpose"></textarea></pats-form-field>
        </div>
        @if (emiPreview()) {
          <div class="create-page__preview">
            <span>EMI Preview (not final):</span>
            <strong>{{ formatInr(emiPreview()!.emiAmount) }}/month</strong>
          </div>
        }
      </article>

      <div class="create-page__actions">
        <pats-button variant="ghost" (clicked)="cancel()">Cancel</pats-button>
        <pats-button [loading]="loading()" (clicked)="submit()">Submit Application</pats-button>
      </div>
    </section>
  `,
  styles: [
    `
      .create-page { display: flex; flex-direction: column; gap: 24px; max-width: 960px; }
      .create-page__back { display: inline-flex; align-items: center; gap: 4px; color: var(--pats-color-primary-container); font-weight: 600; font-size: 14px; }
      .create-page__header h1 { margin: 0; font-family: var(--pats-font-display); font-size: 32px; }
      .create-page__card { padding: 24px; border-radius: var(--pats-radius-lg); background: var(--pats-color-surface-container-lowest); border: 1px solid var(--pats-color-border-subtle); }
      .create-page__card h2 { margin: 0 0 16px; font-family: var(--pats-font-display); color: var(--pats-color-primary-container); }
      .create-page__search { width: 100%; min-height: 44px; padding: 0 16px; border: 1px solid var(--pats-color-border-subtle); border-radius: var(--pats-radius-md); }
      .create-page__member-list { list-style: none; margin: 12px 0 0; padding: 0; display: flex; flex-direction: column; gap: 8px; }
      .create-page__member-list button { width: 100%; text-align: left; padding: 12px; border: 1px solid var(--pats-color-border-subtle); border-radius: var(--pats-radius-md); background: var(--pats-color-surface-muted); cursor: pointer; display: flex; flex-direction: column; }
      .create-page__selected { margin-top: 12px; color: var(--pats-color-secondary); }
      .create-page__products { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 12px; margin-bottom: 16px; }
      .create-page__product { padding: 16px; border: 1px solid var(--pats-color-border-subtle); border-radius: var(--pats-radius-md); background: white; cursor: pointer; display: flex; flex-direction: column; gap: 4px; text-align: left; }
      .create-page__product--selected { border-color: var(--pats-color-primary); background: var(--pats-color-surface-container-low); }
      .create-page__form { display: grid; gap: 16px; }
      .create-page__preview { margin-top: 16px; padding: 16px; border-radius: var(--pats-radius-md); background: var(--pats-color-primary-fixed); display: flex; justify-content: space-between; }
      .create-page__actions { display: flex; justify-content: flex-end; gap: 12px; }
      .create-page__error { color: var(--pats-color-error); }
    `,
  ],
})
export class LoanCreatePageComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly loanApi = inject(LoanApiService);
  private readonly memberApi = inject(MemberApiService);

  readonly products = LOAN_PRODUCTS;
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly memberSearch = signal('');
  readonly memberResults = signal<MemberSummary[]>([]);
  readonly selectedMember = signal<MemberSummary | null>(null);
  readonly selectedProduct = signal<LoanProductType>(LoanProductType.Personal);
  readonly emiPreview = signal<{ emiAmount: number } | null>(null);

  requestedAmount = 100000;
  tenureMonths = 24;
  purpose = '';

  readonly productRate = computed(() =>
    this.products.find((p) => p.productType === this.selectedProduct())?.rate ?? 12
  );

  protected readonly formatInr = formatInrUtil;

  ngOnInit(): void {
    this.updatePreview();
  }

  updatePreview(): void {
    this.emiPreview.set(
      previewEmi({
        principal: this.requestedAmount,
        annualRatePercent: this.productRate(),
        tenureMonths: this.tenureMonths,
      })
    );
  }

  async searchMembers(query: string): Promise<void> {
    this.memberSearch.set(query);
    if (query.trim().length < 2) { this.memberResults.set([]); return; }
    try {
      const response = await this.memberApi.list({ search: query, pageSize: 8 });
      this.memberResults.set(response.items);
    } catch (error) {
      this.errorMessage.set(memberError(error, 'Member search failed.'));
    }
  }

  selectMember(member: MemberSummary): void {
    this.selectedMember.set(member);
    this.errorMessage.set(null);
  }

  async submit(): Promise<void> {
    const member = this.selectedMember();
    if (!member || !this.purpose.trim()) {
      this.errorMessage.set('Select a member and enter a purpose.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);
    const branchId = this.auth.user()?.branchId ?? '00000000-0000-0000-0000-000000000010';

    try {
      const detail = await this.loanApi.create({
        memberId: member.id,
        memberNumber: member.memberNumber,
        memberName: member.fullName,
        branchId,
        productType: this.selectedProduct(),
        requestedAmount: this.requestedAmount,
        tenureMonths: this.tenureMonths,
        purpose: this.purpose.trim(),
      });
      void this.router.navigate(['/loans', detail.id]);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to submit application.'));
    } finally {
      this.loading.set(false);
    }
  }

  cancel(): void { void this.router.navigate(['/loans']); }
}
