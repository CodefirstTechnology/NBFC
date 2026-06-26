import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '@patsanstha/auth';
import { MemberApiService, MemberSummary, extractApiErrorMessage as memberError } from '@patsanstha/members-data-access';
import {
  LoanApiService,
  LoanApplicationSummary,
  LoanApplicationStatus,
  extractApiErrorMessage as loanError,
} from '@patsanstha/loans-data-access';
import {
  CollectionApiService,
  PAYMENT_MODES,
  PaymentMode,
  extractApiErrorMessage,
  formatInr,
} from '@patsanstha/collections-data-access';
import { PatsButtonComponent, PatsFormFieldComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-collection-create-page',
  standalone: true,
  imports: [FormsModule, RouterLink, PatsButtonComponent, PatsFormFieldComponent],
  template: `
    <section class="create-page">
      <header class="create-page__header">
        <a routerLink="/collections" class="create-page__back">
          <span class="material-symbols-outlined">arrow_back</span>
          Back to collections
        </a>
        <h1>Record Collection <span class="create-page__subtitle">/ वसुली नोंद</span></h1>
        <p>Record a loan repayment for an existing member and loan account.</p>
      </header>

      @if (errorMessage()) {
        <p class="create-page__error">{{ errorMessage() }}</p>
      }

      <article class="create-page__card">
        <h2>Member</h2>
        <input
          class="create-page__search"
          type="search"
          placeholder="Search member…"
          [ngModel]="memberSearch()"
          (ngModelChange)="searchMembers($event)" />
        @if (memberResults().length > 0) {
          <ul class="create-page__member-list">
            @for (member of memberResults(); track member.id) {
              <li>
                <button type="button" (click)="selectMember(member)">
                  <strong>{{ member.fullName }}</strong>
                  <span>{{ member.memberNumber }}</span>
                </button>
              </li>
            }
          </ul>
        }
        @if (selectedMember(); as member) {
          <p class="create-page__selected">Selected: <strong>{{ member.fullName }}</strong> ({{ member.memberNumber }})</p>
        }
      </article>

      @if (selectedMember()) {
        <article class="create-page__card">
          <h2>Loan Account</h2>
          @if (loansLoading()) {
            <p>Loading loans…</p>
          } @else if (loanResults().length === 0) {
            <p>No disbursed loans found for this member.</p>
          } @else {
            <ul class="create-page__loan-list">
              @for (loan of loanResults(); track loan.id) {
                <li>
                  <button
                    type="button"
                    class="create-page__loan"
                    [class.create-page__loan--selected]="selectedLoan()?.id === loan.id"
                    (click)="selectLoan(loan)">
                    <strong>{{ loan.loanNumber }}</strong>
                    <span>{{ formatInr(loan.requestedAmount) }} · Disbursed</span>
                  </button>
                </li>
              }
            </ul>
          }
        </article>
      }

      @if (selectedLoan()) {
        <article class="create-page__card">
          <h2>Payment Details</h2>
          <div class="create-page__payment-modes">
            @for (mode of paymentModes; track mode.mode) {
              <button
                type="button"
                class="create-page__mode"
                [class.create-page__mode--selected]="selectedPaymentMode() === mode.mode"
                (click)="selectedPaymentMode.set(mode.mode)">
                <span class="material-symbols-outlined">{{ mode.icon }}</span>
                {{ mode.label }}
              </button>
            }
          </div>
          <div class="create-page__form">
            <pats-form-field label="Amount (₹)">
              <input type="number" min="0.01" step="0.01" [(ngModel)]="amount" />
            </pats-form-field>
            <pats-form-field label="Reference Number">
              <input type="text" [(ngModel)]="referenceNumber" placeholder="Cheque / UPI / txn ref (optional)" />
            </pats-form-field>
            <pats-form-field label="Collected On">
              <input type="date" [(ngModel)]="collectedOn" />
            </pats-form-field>
          </div>
        </article>
      }

      <div class="create-page__actions">
        <pats-button variant="ghost" (clicked)="cancel()">Cancel</pats-button>
        <pats-button [loading]="loading()" [disabled]="!canSubmit()" (clicked)="submit()">Record Collection</pats-button>
      </div>
    </section>
  `,
  styles: [
    `
      .create-page { display: flex; flex-direction: column; gap: 24px; max-width: 960px; }
      .create-page__back { display: inline-flex; align-items: center; gap: 4px; color: var(--pats-color-primary-container); font-weight: 600; font-size: 14px; }
      .create-page__header h1 { margin: 0; font-family: var(--pats-font-display); font-size: 32px; }
      .create-page__subtitle { font-size: 18px; color: var(--pats-color-text-secondary); font-weight: 500; }
      .create-page__card { padding: 24px; border-radius: var(--pats-radius-lg); background: var(--pats-color-surface-container-lowest); border: 1px solid var(--pats-color-border-subtle); }
      .create-page__card h2 { margin: 0 0 16px; font-family: var(--pats-font-display); color: var(--pats-color-primary-container); }
      .create-page__search { width: 100%; min-height: 44px; padding: 0 16px; border: 1px solid var(--pats-color-border-subtle); border-radius: var(--pats-radius-md); }
      .create-page__member-list, .create-page__loan-list { list-style: none; margin: 12px 0 0; padding: 0; display: flex; flex-direction: column; gap: 8px; }
      .create-page__member-list button, .create-page__loan { width: 100%; text-align: left; padding: 12px; border: 1px solid var(--pats-color-border-subtle); border-radius: var(--pats-radius-md); background: var(--pats-color-surface-muted); cursor: pointer; display: flex; flex-direction: column; gap: 4px; }
      .create-page__loan--selected { border-color: var(--pats-color-primary); background: var(--pats-color-surface-container-low); }
      .create-page__selected { margin-top: 12px; color: var(--pats-color-secondary); }
      .create-page__payment-modes { display: grid; grid-template-columns: repeat(auto-fit, minmax(140px, 1fr)); gap: 12px; margin-bottom: 16px; }
      .create-page__mode { padding: 12px; border: 1px solid var(--pats-color-border-subtle); border-radius: var(--pats-radius-md); background: white; cursor: pointer; display: flex; flex-direction: column; align-items: center; gap: 4px; font-size: 13px; }
      .create-page__mode--selected { border-color: var(--pats-color-primary); background: var(--pats-color-surface-container-low); }
      .create-page__form { display: grid; gap: 16px; }
      .create-page__actions { display: flex; justify-content: flex-end; gap: 12px; }
      .create-page__error { color: var(--pats-color-error); }
    `,
  ],
})
export class CollectionCreatePageComponent {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly collectionApi = inject(CollectionApiService);
  private readonly memberApi = inject(MemberApiService);
  private readonly loanApi = inject(LoanApiService);

  readonly paymentModes = PAYMENT_MODES;
  readonly loading = signal(false);
  readonly loansLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly memberSearch = signal('');
  readonly memberResults = signal<MemberSummary[]>([]);
  readonly selectedMember = signal<MemberSummary | null>(null);
  readonly loanResults = signal<LoanApplicationSummary[]>([]);
  readonly selectedLoan = signal<LoanApplicationSummary | null>(null);
  readonly selectedPaymentMode = signal<PaymentMode>(PaymentMode.Cash);

  amount = 0;
  referenceNumber = '';
  collectedOn = new Date().toISOString().slice(0, 10);

  protected readonly formatInr = formatInr;

  async searchMembers(query: string): Promise<void> {
    this.memberSearch.set(query);
    if (query.trim().length < 2) {
      this.memberResults.set([]);
      return;
    }
    try {
      const response = await this.memberApi.list({ search: query, pageSize: 8 });
      this.memberResults.set(response.items);
    } catch (error) {
      this.errorMessage.set(memberError(error, 'Member search failed.'));
    }
  }

  async selectMember(member: MemberSummary): Promise<void> {
    this.selectedMember.set(member);
    this.selectedLoan.set(null);
    this.errorMessage.set(null);
    await this.loadLoans(member.id);
  }

  selectLoan(loan: LoanApplicationSummary): void {
    this.selectedLoan.set(loan);
    this.errorMessage.set(null);
  }

  canSubmit(): boolean {
    return !!this.selectedMember() && !!this.selectedLoan() && this.amount > 0 && !!this.collectedOn;
  }

  async submit(): Promise<void> {
    const member = this.selectedMember();
    const loan = this.selectedLoan();
    if (!member || !loan || !this.canSubmit()) {
      this.errorMessage.set('Select a member, loan, and enter a valid amount.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);
    const branchId = this.auth.user()?.branchId ?? '00000000-0000-0000-0000-000000000010';

    try {
      const detail = await this.collectionApi.create({
        memberId: member.id,
        memberNumber: member.memberNumber,
        memberName: member.fullName,
        loanApplicationId: loan.id,
        loanNumber: loan.loanNumber,
        branchId,
        amount: this.amount,
        paymentMode: this.selectedPaymentMode(),
        referenceNumber: this.referenceNumber.trim() || null,
        collectedOn: this.collectedOn,
      });
      void this.router.navigate(['/collections', detail.id]);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to record collection.'));
    } finally {
      this.loading.set(false);
    }
  }

  cancel(): void { void this.router.navigate(['/collections']); }

  private async loadLoans(memberId: string): Promise<void> {
    this.loansLoading.set(true);
    try {
      const response = await this.loanApi.list({
        memberId,
        pageSize: 50,
        status: LoanApplicationStatus.Disbursed,
      });
      this.loanResults.set(response.items);
    } catch (error) {
      this.errorMessage.set(loanError(error, 'Failed to load member loans.'));
      this.loanResults.set([]);
    } finally {
      this.loansLoading.set(false);
    }
  }
}
