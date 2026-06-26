import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '@patsanstha/auth';
import {
  LoanApiService,
  LoanApplicationDetail,
  LoanApplicationStatus,
  extractApiErrorMessage,
  formatInr,
  loanProductLabel,
  loanStatusLabel,
  loanStatusVariant,
} from '@patsanstha/loans-data-access';
import { PatsButtonComponent, PatsFormFieldComponent, PatsStatusPillComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-loan-detail-page',
  standalone: true,
  imports: [RouterLink, FormsModule, HasPermissionDirective, PatsButtonComponent, PatsFormFieldComponent, PatsStatusPillComponent],
  template: `
    <section class="detail-page">
      <a routerLink="/loans" class="detail-page__back">
        <span class="material-symbols-outlined">arrow_back</span>
        Back to queue
      </a>

      @if (loading()) {
        <p>Loading application…</p>
      } @else if (errorMessage()) {
        <p class="detail-page__error">{{ errorMessage() }}</p>
      } @else {
        @if (loan(); as l) {
          <header class="detail-page__header">
            <div>
              <p class="detail-page__eyebrow">{{ l.loanNumber }}</p>
              <h1>{{ l.memberName }}</h1>
              <p>{{ loanProductLabel(l.productType) }} · {{ l.memberNumber }}</p>
            </div>
            <pats-status-pill [label]="statusLabel()" [variant]="statusVariant()" />
          </header>

          <div class="detail-page__grid">
            <article class="detail-page__card">
              <h2>Application</h2>
              <dl>
                <dt>Requested</dt><dd>{{ formatInr(l.requestedAmount) }}</dd>
                <dt>Approved</dt><dd>{{ l.approvedAmount != null ? formatInr(l.approvedAmount) : '—' }}</dd>
                <dt>Interest Rate</dt><dd>{{ l.interestRate }}% p.a.</dd>
                <dt>Tenure</dt><dd>{{ l.tenureMonths }} months</dd>
                <dt>EMI</dt><dd>{{ l.emiAmount != null ? formatInr(l.emiAmount) : '—' }}</dd>
                <dt>Purpose</dt><dd>{{ l.purpose }}</dd>
              </dl>
            </article>

            <article class="detail-page__card">
              <h2>Timeline</h2>
              <dl>
                <dt>Applied</dt><dd>{{ l.appliedOn }}</dd>
                <dt>Approved</dt><dd>{{ l.approvedOn ?? '—' }}</dd>
                <dt>Disbursed</dt><dd>{{ l.disbursedOn ?? '—' }}</dd>
                @if (l.rejectionReason) {
                  <dt>Rejection Reason</dt><dd>{{ l.rejectionReason }}</dd>
                }
                @if (l.outstandingPrincipal != null) {
                  <dt>Outstanding</dt><dd>{{ formatInr(l.outstandingPrincipal) }}</dd>
                }
              </dl>
            </article>
          </div>

          @if (l.status === submittedStatus) {
            <div class="detail-page__actions">
              <pats-form-field label="Approved Amount (₹)">
                <input type="number" [(ngModel)]="approvedAmount" />
              </pats-form-field>
              <pats-button *patsHasPermission="'loans.approve'" variant="secondary" [loading]="actionLoading()" (clicked)="approve()">Approve</pats-button>
              <input class="detail-page__reject-input" placeholder="Rejection reason" [(ngModel)]="rejectReason" />
              <pats-button *patsHasPermission="'loans.approve'" variant="ghost" [loading]="actionLoading()" (clicked)="reject()">Reject</pats-button>
            </div>
          }

          @if (l.status === approvedStatus) {
            <div class="detail-page__actions">
              <pats-button *patsHasPermission="'loans.disburse'" [loading]="actionLoading()" (clicked)="disburse()">Disburse Loan</pats-button>
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
      .detail-page__actions { display: flex; flex-wrap: wrap; align-items: center; gap: 12px; padding: 16px; border-radius: var(--pats-radius-lg); background: var(--pats-color-surface-muted); }
      .detail-page__reject-input { flex: 1; min-width: 200px; min-height: 44px; padding: 0 12px; border: 1px solid var(--pats-color-border-subtle); border-radius: var(--pats-radius-md); }
      .detail-page__error { color: var(--pats-color-error); }
    `,
  ],
})
export class LoanDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly loanApi = inject(LoanApiService);

  readonly loading = signal(true);
  readonly actionLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly loan = signal<LoanApplicationDetail | null>(null);
  readonly statusLabel = signal('');
  readonly statusVariant = signal<'active' | 'inactive' | 'pending' | 'warning' | 'error'>('pending');

  readonly submittedStatus = LoanApplicationStatus.Submitted;
  readonly approvedStatus = LoanApplicationStatus.Approved;

  protected readonly formatInr = formatInr;
  protected readonly loanProductLabel = loanProductLabel;

  approvedAmount = 0;
  rejectReason = '';

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { this.errorMessage.set('Loan id missing.'); this.loading.set(false); return; }
    void this.loadLoan(id);
  }

  async approve(): Promise<void> {
    const l = this.loan();
    if (!l) return;
    this.actionLoading.set(true);
    try {
      const updated = await this.loanApi.approve(l.id, this.approvedAmount);
      this.setLoan(updated);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Approval failed.'));
    } finally {
      this.actionLoading.set(false);
    }
  }

  async reject(): Promise<void> {
    const l = this.loan();
    if (!l || !this.rejectReason.trim()) { this.errorMessage.set('Enter a rejection reason.'); return; }
    this.actionLoading.set(true);
    try {
      const updated = await this.loanApi.reject(l.id, this.rejectReason.trim());
      this.setLoan(updated);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Rejection failed.'));
    } finally {
      this.actionLoading.set(false);
    }
  }

  async disburse(): Promise<void> {
    const l = this.loan();
    if (!l) return;
    this.actionLoading.set(true);
    try {
      const updated = await this.loanApi.disburse(l.id);
      this.setLoan(updated);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Disbursement failed.'));
    } finally {
      this.actionLoading.set(false);
    }
  }

  private async loadLoan(id: string): Promise<void> {
    this.loading.set(true);
    try {
      const detail = await this.loanApi.getById(id);
      this.setLoan(detail);
      this.approvedAmount = detail.requestedAmount;
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Loan not found.'));
    } finally {
      this.loading.set(false);
    }
  }

  private setLoan(detail: LoanApplicationDetail): void {
    this.loan.set(detail);
    this.statusLabel.set(loanStatusLabel(detail.status));
    this.statusVariant.set(loanStatusVariant(detail.status));
    this.errorMessage.set(null);
  }
}
