import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '@patsanstha/auth';
import { previewEmi } from '@patsanstha/core-utils';
import {
  MemberApiService,
  MemberDetail,
  MemberSummary,
  KycVerificationStatus,
  kycStatusLabel,
  extractApiErrorMessage as memberError,
} from '@patsanstha/members-data-access';
import {
  LOAN_PRODUCTS,
  LoanApiService,
  LoanProductInfo,
  getLoanProductMaxTenure,
  extractApiErrorMessage,
  formatInr,
} from '@patsanstha/loans-data-access';
import { mergeLoanProductRates } from './loan-product-rates';

@Component({
  selector: 'pats-loan-create-page',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <section class="loan-wizard">
      <a routerLink="/loans" class="loan-wizard__back">
        <span class="material-symbols-outlined">arrow_back</span>
        Back to queue
      </a>

      <article class="loan-wizard__card">
        <nav class="loan-wizard__stepper" aria-label="Loan application steps">
          <div class="loan-wizard__stepper-line"></div>
          @for (item of steps; track item.id) {
            <div
              class="loan-wizard__step"
              [class.loan-wizard__step--active]="step() === item.id"
              [class.loan-wizard__step--done]="step() > item.id">
              <div class="loan-wizard__step-circle">{{ item.id }}</div>
              <span>{{ item.label }}</span>
            </div>
          }
        </nav>

        @if (errorMessage()) {
          <p class="loan-wizard__error">{{ errorMessage() }}</p>
        }

        @if (step() === 1) {
          <header class="loan-wizard__heading">
            <h1>Select Loan Product</h1>
            <p>कर्ज उत्पादन निवडा</p>
          </header>

          <div class="loan-wizard__products">
            @for (product of products(); track product.productType) {
              <button
                type="button"
                class="loan-wizard__product"
                [class.loan-wizard__product--selected]="selectedProduct()?.productType === product.productType"
                (click)="selectProduct(product)">
                @if (selectedProduct()?.productType === product.productType) {
                  <span class="loan-wizard__product-check material-symbols-outlined">check_circle</span>
                }
                <span class="loan-wizard__product-icon loan-wizard__product-icon--{{ product.iconTone }}">
                  <span class="material-symbols-outlined">{{ product.icon }}</span>
                </span>
                <div class="loan-wizard__product-title">
                  <strong>{{ product.label }}</strong>
                  <span>{{ product.labelMr }}</span>
                </div>
                <div class="loan-wizard__product-meta">
                  <div>
                    <small>INTEREST</small>
                    <strong>{{ product.rate }}% p.a.</strong>
                  </div>
                  <div>
                    <small>MAX TENURE</small>
                    <strong>{{ product.maxTenureMonths }} Months</strong>
                  </div>
                </div>
              </button>
            }
          </div>
        }

        @if (step() === 2) {
          <header class="loan-wizard__heading">
            <h1>Member KYC</h1>
            <p>सभासद KYC — Identify and verify the applicant</p>
          </header>

          <div class="loan-wizard__search-wrap">
            <span class="material-symbols-outlined">search</span>
            <input
              class="loan-wizard__search"
              type="search"
              placeholder="Search by Member ID, Aadhaar, or Name"
              [ngModel]="memberSearch()"
              (ngModelChange)="searchMembers($event)" />
          </div>

          @if (memberSearchLoading()) {
            <p class="loan-wizard__hint">Searching members…</p>
          }

          @if (memberResults().length > 0 && !selectedMember()) {
            <ul class="loan-wizard__member-list">
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
            <div class="loan-wizard__member-card">
              <div>
                <strong>{{ member.fullName }}</strong>
                <span>{{ member.memberNumber }} · {{ member.mobileNumber }}</span>
              </div>
              <span class="loan-wizard__verified">Verified Member</span>
            </div>
          }

          @if (memberDetail(); as detail) {
            <ul class="loan-wizard__kyc-list">
              <li [class.loan-wizard__kyc--ok]="detail.aadhaarVerificationStatus === kycVerified">
                <span class="material-symbols-outlined">
                  {{ detail.aadhaarVerificationStatus === kycVerified ? 'check_circle' : 'radio_button_unchecked' }}
                </span>
                Aadhaar: {{ detail.aadhaarMasked ?? 'Not on file' }}
                ({{ kycStatusLabel(detail.aadhaarVerificationStatus) }})
              </li>
              <li [class.loan-wizard__kyc--ok]="detail.panVerificationStatus === kycVerified">
                <span class="material-symbols-outlined">
                  {{ detail.panVerificationStatus === kycVerified ? 'check_circle' : 'radio_button_unchecked' }}
                </span>
                PAN: {{ detail.panMasked ?? 'Not on file' }}
                ({{ kycStatusLabel(detail.panVerificationStatus) }})
              </li>
              <li [class.loan-wizard__kyc--ok]="!!detail.photoUrl">
                <span class="material-symbols-outlined">
                  {{ detail.photoUrl ? 'check_circle' : 'radio_button_unchecked' }}
                </span>
                Member photo on file
              </li>
            </ul>
          }
        }

        @if (step() === 3) {
          <header class="loan-wizard__heading">
            <h1>Loan Details</h1>
            <p>कर्ज तपशील — Amount, tenure, and purpose</p>
          </header>

          @if (selectedProduct(); as product) {
            <div class="loan-wizard__selected-product">
              <span class="material-symbols-outlined">{{ product.icon }}</span>
              {{ product.label }} · {{ product.rate }}% p.a.
            </div>
          }

          <div class="loan-wizard__form">
            <label class="loan-wizard__field">
              <span>Requested Amount (₹)</span>
              <input
                type="number"
                min="1"
                placeholder="Enter loan amount"
                [(ngModel)]="requestedAmount"
                (ngModelChange)="updatePreview()" />
            </label>
            <label class="loan-wizard__field">
              <span>Tenure (Months)</span>
              <select [(ngModel)]="tenureMonths" (ngModelChange)="updatePreview()">
                <option [ngValue]="null">Select tenure</option>
                @for (months of tenureOptions(); track months) {
                  <option [ngValue]="months">{{ months }} Months</option>
                }
              </select>
              <small>Maximum {{ productMaxTenure() }} months for {{ selectedProduct()?.label ?? 'this product' }}</small>
            </label>
            <label class="loan-wizard__field">
              <span>Purpose of Loan</span>
              <textarea rows="4" [(ngModel)]="purpose" placeholder="Describe how the loan will be used…"></textarea>
              <small>{{ purpose.trim().length }}/10 characters minimum</small>
            </label>
          </div>

          @if (emiPreview(); as preview) {
            <div class="loan-wizard__emi-preview">
              <div>
                <span>Monthly EMI</span>
                <strong>{{ formatInr(preview.emiAmount) }}</strong>
              </div>
              <div>
                <span>Interest ({{ productRate() }}% p.a.)</span>
                <strong>{{ formatInr(preview.totalInterest) }}</strong>
              </div>
              <div>
                <span>Total payable</span>
                <strong>{{ formatInr(preview.totalPayable) }}</strong>
              </div>
              <div>
                <span>Principal</span>
                <strong>{{ formatInr(requestedAmount || 0) }}</strong>
              </div>
            </div>
            <p class="loan-wizard__emi-note">
              Calculated as reducing-balance EMI on ₹{{ requestedAmount || 0 }} at {{ productRate() }}% p.a. for {{ tenureMonths }} months.
            </p>
          } @else {
            <p class="loan-wizard__emi-note">
              Enter amount and tenure to see estimated EMI based on {{ productRate() }}% p.a.
            </p>
          }
        }

        @if (step() === 4) {
          <header class="loan-wizard__heading">
            <h1>Verify Eligibility</h1>
            <p>पात्रता तपासा — Confirm documents and consent</p>
          </header>

          <ul class="loan-wizard__verify-list">
            <li [class.loan-wizard__kyc--ok]="!!selectedMember()">
              <span class="material-symbols-outlined">{{ selectedMember() ? 'check_circle' : 'radio_button_unchecked' }}</span>
              Member identity confirmed
            </li>
            <li [class.loan-wizard__kyc--ok]="kycComplete()">
              <span class="material-symbols-outlined">{{ kycComplete() ? 'check_circle' : 'radio_button_unchecked' }}</span>
              KYC documents verified
            </li>
            <li [class.loan-wizard__kyc--ok]="(requestedAmount ?? 0) > 0 && (tenureMonths ?? 0) > 0">
              <span class="material-symbols-outlined">{{ (requestedAmount ?? 0) > 0 && (tenureMonths ?? 0) > 0 ? 'check_circle' : 'radio_button_unchecked' }}</span>
              Loan amount and tenure within product limits
            </li>
            <li [class.loan-wizard__kyc--ok]="purpose.trim().length >= 10">
              <span class="material-symbols-outlined">{{ purpose.trim().length >= 10 ? 'check_circle' : 'radio_button_unchecked' }}</span>
              Purpose of loan documented
            </li>
          </ul>

          <label class="loan-wizard__consent">
            <input type="checkbox" [(ngModel)]="consentGiven" />
            I confirm that the information provided is accurate and authorize credit assessment for this application.
          </label>
        }

        @if (step() === 5) {
          <header class="loan-wizard__heading">
            <h1>Review Application</h1>
            <p>अर्जाचे पुनरावलोकन — Submit when all details are correct</p>
          </header>

          <dl class="loan-wizard__review">
            <div>
              <dt>Member</dt>
              <dd>{{ selectedMember()?.fullName ?? '—' }} ({{ selectedMember()?.memberNumber ?? '—' }})</dd>
            </div>
            <div>
              <dt>Product</dt>
              <dd>{{ selectedProduct()?.label ?? '—' }}</dd>
            </div>
            <div>
              <dt>Requested Amount</dt>
              <dd>{{ requestedAmount != null ? formatInr(requestedAmount) : '—' }}</dd>
            </div>
            <div>
              <dt>Tenure</dt>
              <dd>{{ tenureMonths != null ? tenureMonths + ' months' : '—' }}</dd>
            </div>
            <div>
              <dt>Interest Rate</dt>
              <dd>{{ selectedProduct()?.rate ?? productRate() }}% p.a.</dd>
            </div>
            <div>
              <dt>Estimated EMI</dt>
              <dd>{{ emiPreview() ? formatInr(emiPreview()!.emiAmount) + '/month' : '—' }}</dd>
            </div>
            <div>
              <dt>Total Interest</dt>
              <dd>{{ emiPreview() ? formatInr(emiPreview()!.totalInterest) : '—' }}</dd>
            </div>
            <div>
              <dt>Total Payable</dt>
              <dd>{{ emiPreview() ? formatInr(emiPreview()!.totalPayable) : '—' }}</dd>
            </div>
            <div class="loan-wizard__review-full">
              <dt>Purpose</dt>
              <dd>{{ purpose.trim() || '—' }}</dd>
            </div>
          </dl>
        }

        @if (getBlockedReason(); as reason) {
          <p class="loan-wizard__blocked" role="status">{{ reason }}</p>
        }

        <footer class="loan-wizard__footer">
          @if (step() > 1) {
            <button type="button" class="loan-wizard__ghost-btn" (click)="goBack()">Back / मागे</button>
          }
          <div class="loan-wizard__footer-spacer"></div>
          @if (step() < 5) {
            <button
              type="button"
              class="loan-wizard__primary-btn"
              [disabled]="!canProceed()"
              (click)="goNext()">
              Next / पुढे
              <span class="material-symbols-outlined">arrow_forward</span>
            </button>
          } @else {
            <button
              type="button"
              class="loan-wizard__primary-btn"
              [disabled]="loading()"
              (click)="submit()">
              {{ loading() ? 'Submitting…' : 'Submit Application / अर्ज सादर करा' }}
            </button>
          }
        </footer>
      </article>

      <div class="loan-wizard__trust">
        <article>
          <span class="material-symbols-outlined">shield</span>
          <div>
            <strong>Secure Process</strong>
            <p>Advanced encryption for all your financial data.</p>
          </div>
        </article>
        <article>
          <span class="material-symbols-outlined">support_agent</span>
          <div>
            <strong>24/7 Support</strong>
            <p>Our dedicated team is here to help you at every step.</p>
          </div>
        </article>
        <article>
          <span class="material-symbols-outlined">speed</span>
          <div>
            <strong>Quick Approval</strong>
            <p>Get instant feedback on your loan eligibility.</p>
          </div>
        </article>
      </div>
    </section>
  `,
  styles: [
    `
      .loan-wizard {
        display: flex;
        flex-direction: column;
        gap: 24px;
        width: 100%;
      }

      .loan-wizard__back {
        display: inline-flex;
        align-items: center;
        gap: 4px;
        color: var(--pats-color-primary-container);
        font-size: 14px;
        font-weight: 600;
      }

      .loan-wizard__card {
        padding: 32px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
        box-shadow: var(--pats-shadow-card);
      }

      .loan-wizard__stepper {
        position: relative;
        display: grid;
        grid-template-columns: repeat(5, minmax(0, 1fr));
        gap: 8px;
        margin-bottom: 32px;
        padding-bottom: 8px;
      }

      .loan-wizard__stepper-line {
        position: absolute;
        top: 18px;
        left: 8%;
        right: 8%;
        height: 2px;
        background: var(--pats-color-border-subtle);
      }

      .loan-wizard__step {
        position: relative;
        z-index: 1;
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 8px;
        font-size: 11px;
        font-weight: 700;
        letter-spacing: 0.04em;
        color: var(--pats-color-text-secondary);
      }

      .loan-wizard__step-circle {
        width: 36px;
        height: 36px;
        border-radius: 50%;
        display: grid;
        place-items: center;
        background: var(--pats-color-surface-container);
        border: 2px solid var(--pats-color-border-subtle);
        font-size: 14px;
      }

      .loan-wizard__step--active,
      .loan-wizard__step--done {
        color: var(--pats-color-primary-container);
      }

      .loan-wizard__step--active .loan-wizard__step-circle,
      .loan-wizard__step--done .loan-wizard__step-circle {
        background: var(--pats-color-primary-container);
        border-color: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary);
      }

      .loan-wizard__heading h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 28px;
        color: var(--pats-color-primary);
      }

      .loan-wizard__heading p {
        margin: 6px 0 24px;
        color: var(--pats-color-text-secondary);
        font-size: 14px;
      }

      .loan-wizard__products {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 20px;
      }

      @media (max-width: 768px) {
        .loan-wizard__products {
          grid-template-columns: 1fr;
        }

        .loan-wizard__stepper {
          grid-template-columns: repeat(5, minmax(0, 1fr));
        }

        .loan-wizard__step span {
          font-size: 9px;
        }
      }

      .loan-wizard__product {
        position: relative;
        padding: 24px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        text-align: left;
        cursor: pointer;
        display: flex;
        flex-direction: column;
        gap: 16px;
        min-height: 180px;
      }

      .loan-wizard__product--selected {
        border-color: var(--pats-color-primary-container);
        box-shadow: 0 0 0 1px var(--pats-color-primary-container);
      }

      .loan-wizard__product-check {
        position: absolute;
        top: 16px;
        right: 16px;
        color: var(--pats-color-primary-container);
        font-size: 24px;
      }

      .loan-wizard__product-icon {
        width: 44px;
        height: 44px;
        border-radius: var(--pats-radius-md);
        display: grid;
        place-items: center;
      }

      .loan-wizard__product-icon--blue {
        background: rgba(26, 60, 110, 0.1);
        color: var(--pats-color-primary-container);
      }

      .loan-wizard__product-icon--gold {
        background: rgba(242, 169, 59, 0.15);
        color: #b45309;
      }

      .loan-wizard__product-icon--green {
        background: rgba(46, 158, 91, 0.12);
        color: var(--pats-color-success);
      }

      .loan-wizard__product-title strong {
        display: block;
        font-size: 18px;
        color: var(--pats-color-primary);
      }

      .loan-wizard__product-title span {
        display: block;
        margin-top: 4px;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
      }

      .loan-wizard__product-meta {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 12px;
        margin-top: auto;
      }

      .loan-wizard__product-meta small {
        display: block;
        font-size: 10px;
        font-weight: 700;
        letter-spacing: 0.04em;
        color: var(--pats-color-text-secondary);
      }

      .loan-wizard__product-meta strong {
        display: block;
        margin-top: 4px;
        font-size: 16px;
        color: var(--pats-color-primary);
      }

      .loan-wizard__search-wrap {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 0 16px;
        min-height: 52px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-surface-muted);
        margin-bottom: 16px;
      }

      .loan-wizard__search {
        flex: 1;
        border: none;
        background: transparent;
        min-height: 44px;
        font-size: 14px;
        outline: none;
      }

      .loan-wizard__member-list {
        list-style: none;
        margin: 0 0 16px;
        padding: 0;
        display: flex;
        flex-direction: column;
        gap: 8px;
      }

      .loan-wizard__member-list button {
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

      .loan-wizard__member-card {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 16px;
        padding: 20px;
        border-radius: var(--pats-radius-md);
        border: 1px solid var(--pats-color-border-subtle);
        background: var(--pats-color-surface-muted);
        margin-bottom: 16px;
      }

      .loan-wizard__member-card strong {
        display: block;
      }

      .loan-wizard__member-card span {
        display: block;
        margin-top: 4px;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
      }

      .loan-wizard__verified {
        padding: 6px 12px;
        border-radius: var(--pats-radius-full);
        background: rgba(46, 158, 91, 0.12);
        color: var(--pats-color-success);
        font-size: 12px;
        font-weight: 700;
        white-space: nowrap;
      }

      .loan-wizard__kyc-list,
      .loan-wizard__verify-list {
        list-style: none;
        margin: 0;
        padding: 0;
        display: grid;
        gap: 12px;
      }

      .loan-wizard__kyc-list li,
      .loan-wizard__verify-list li {
        display: flex;
        align-items: center;
        gap: 10px;
        padding: 14px 16px;
        border-radius: var(--pats-radius-md);
        border: 1px solid var(--pats-color-border-subtle);
        font-size: 14px;
        color: var(--pats-color-text-secondary);
      }

      .loan-wizard__kyc--ok {
        color: var(--pats-color-success);
        background: rgba(46, 158, 91, 0.06);
      }

      .loan-wizard__selected-product {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        padding: 10px 14px;
        margin-bottom: 20px;
        border-radius: var(--pats-radius-full);
        background: rgba(26, 60, 110, 0.08);
        color: var(--pats-color-primary-container);
        font-size: 13px;
        font-weight: 600;
      }

      .loan-wizard__form {
        display: grid;
        gap: 16px;
      }

      .loan-wizard__field {
        display: flex;
        flex-direction: column;
        gap: 8px;
        font-size: 13px;
        font-weight: 600;
      }

      .loan-wizard__field input,
      .loan-wizard__field textarea,
      .loan-wizard__field select {
        min-height: 44px;
        padding: 12px 14px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        font-size: 14px;
        font-family: inherit;
      }

      .loan-wizard__field small {
        font-weight: 400;
        color: var(--pats-color-text-secondary);
      }

      .loan-wizard__emi-preview {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 16px;
        margin-top: 20px;
        padding: 20px;
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary);
      }

      .loan-wizard__emi-preview span {
        display: block;
        font-size: 12px;
        opacity: 0.9;
      }

      .loan-wizard__emi-preview strong {
        display: block;
        margin-top: 4px;
        font-family: var(--pats-font-display);
        font-size: 20px;
      }

      .loan-wizard__emi-note {
        margin: 10px 0 0;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
      }

      .loan-wizard__consent {
        display: flex;
        align-items: flex-start;
        gap: 10px;
        margin-top: 20px;
        font-size: 14px;
        line-height: 1.5;
        color: var(--pats-color-text-secondary);
      }

      .loan-wizard__review {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 16px;
        margin: 0;
      }

      .loan-wizard__review-full {
        grid-column: 1 / -1;
      }

      .loan-wizard__review dt {
        font-size: 11px;
        font-weight: 700;
        text-transform: uppercase;
        letter-spacing: 0.04em;
        color: var(--pats-color-text-secondary);
      }

      .loan-wizard__review dd {
        margin: 4px 0 0;
        font-size: 15px;
        font-weight: 600;
      }

      .loan-wizard__footer {
        display: flex;
        align-items: center;
        gap: 12px;
        margin-top: 32px;
        padding-top: 24px;
        border-top: 1px solid var(--pats-color-border-subtle);
      }

      .loan-wizard__footer-spacer {
        flex: 1;
      }

      .loan-wizard__primary-btn,
      .loan-wizard__ghost-btn {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        min-height: 48px;
        padding: 0 24px;
        border-radius: var(--pats-radius-md);
        font-size: 14px;
        font-weight: 600;
        cursor: pointer;
      }

      .loan-wizard__primary-btn {
        border: none;
        background: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary);
      }

      .loan-wizard__primary-btn:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }

      .loan-wizard__ghost-btn {
        border: 1px solid var(--pats-color-border-subtle);
        background: var(--pats-color-surface-container-lowest);
        color: var(--pats-color-on-surface);
      }

      .loan-wizard__trust {
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 24px;
      }

      @media (max-width: 900px) {
        .loan-wizard__trust {
          grid-template-columns: 1fr;
        }
      }

      .loan-wizard__trust article {
        display: flex;
        gap: 12px;
        align-items: flex-start;
      }

      .loan-wizard__trust .material-symbols-outlined {
        color: var(--pats-color-primary-container);
        font-size: 28px;
      }

      .loan-wizard__trust strong {
        display: block;
        font-size: 14px;
        margin-bottom: 4px;
      }

      .loan-wizard__trust p {
        margin: 0;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
        line-height: 1.5;
      }

      .loan-wizard__error {
        color: var(--pats-color-error);
        margin-bottom: 16px;
      }

      .loan-wizard__blocked {
        margin: 0 0 12px;
        padding: 12px 16px;
        border-radius: var(--pats-radius-md);
        background: rgba(242, 169, 59, 0.12);
        color: #92400e;
        font-size: 14px;
      }

      .loan-wizard__hint {
        color: var(--pats-color-text-secondary);
        font-size: 14px;
      }
    `,
  ],
})
export class LoanCreatePageComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly loanApi = inject(LoanApiService);
  private readonly memberApi = inject(MemberApiService);

  readonly products = signal<LoanProductInfo[]>(LOAN_PRODUCTS);
  readonly steps = [
    { id: 1, label: 'PRODUCT' },
    { id: 2, label: 'KYC' },
    { id: 3, label: 'DETAILS' },
    { id: 4, label: 'VERIFY' },
    { id: 5, label: 'REVIEW' },
  ];

  readonly step = signal(1);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly memberSearch = signal('');
  readonly memberSearchLoading = signal(false);
  readonly memberResults = signal<MemberSummary[]>([]);
  readonly selectedMember = signal<MemberSummary | null>(null);
  readonly memberDetail = signal<MemberDetail | null>(null);
  readonly selectedProduct = signal<LoanProductInfo | null>(null);
  readonly emiPreview = signal<{
    emiAmount: number;
    totalInterest: number;
    totalPayable: number;
  } | null>(null);

  readonly kycVerified = KycVerificationStatus.Verified;

  requestedAmount: number | null = null;
  tenureMonths: number | null = null;
  purpose = '';
  consentGiven = false;

  protected readonly formatInr = formatInr;
  protected readonly kycStatusLabel = kycStatusLabel;

  readonly productRate = computed(() => this.selectedProduct()?.rate ?? 12);

  readonly productMaxTenure = computed(() => {
    const product = this.selectedProduct();
    if (!product) {
      return 60;
    }

    return product.maxTenureMonths ?? getLoanProductMaxTenure(product.productType);
  });

  readonly tenureOptions = computed(() => {
    const max = this.productMaxTenure();
    const candidates = [6, 12, 18, 24, 36, 48, 60, 72, 84, 96, 120];
    const options = candidates.filter((months) => months <= max);
    return options.length > 0 ? options : [max];
  });

  readonly kycComplete = computed(() => {
    const detail = this.memberDetail();
    if (!detail) {
      return false;
    }

    return (
      detail.aadhaarVerificationStatus === KycVerificationStatus.Verified &&
      detail.panVerificationStatus === KycVerificationStatus.Verified
    );
  });

  ngOnInit(): void {
    void this.loadProductRates();
  }

  updatePreview(): void {
    const amount = Number(this.requestedAmount) || 0;
    const tenure = Number(this.tenureMonths) || 0;

    if (amount <= 0 || tenure <= 0) {
      this.emiPreview.set(null);
      return;
    }

    const preview = previewEmi({
      principal: amount,
      annualRatePercent: this.productRate(),
      tenureMonths: tenure,
    });

    this.emiPreview.set({
      emiAmount: preview.emiAmount,
      totalInterest: preview.totalInterest,
      totalPayable: preview.totalPayment,
    });
  }

  selectProduct(product: LoanProductInfo): void {
    this.selectedProduct.set(product);
    const maxTenure = product.maxTenureMonths ?? getLoanProductMaxTenure(product.productType);

    if (this.tenureMonths != null && this.tenureMonths > maxTenure) {
      this.tenureMonths = maxTenure;
    }

    this.updatePreview();
    this.errorMessage.set(null);
  }

  private async loadProductRates(): Promise<void> {
    try {
      const apiProducts = await this.loanApi.getProducts();
      const merged = mergeLoanProductRates(LOAN_PRODUCTS, apiProducts);
      this.products.set(merged);

      const selected = this.selectedProduct();
      if (selected) {
        const refreshed =
          merged.find((p: LoanProductInfo) => p.productType === selected.productType) ?? null;
        if (refreshed) {
          this.selectProduct(refreshed);
        }
      }
    } catch {
      // Keep local catalog rates if products API is unavailable.
      this.updatePreview();
    }
  }

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

  async selectMember(member: MemberSummary): Promise<void> {
    this.selectedMember.set(member);
    this.memberResults.set([]);
    this.memberSearch.set(member.fullName);
    this.errorMessage.set(null);

    try {
      const detail = await this.memberApi.getById(member.id);
      this.memberDetail.set(detail);
    } catch (error) {
      this.memberDetail.set(null);
      this.errorMessage.set(memberError(error, 'Failed to load member KYC details.'));
    }
  }

  canProceed(): boolean {
    switch (this.step()) {
      case 1:
        return !!this.selectedProduct();
      case 2:
        return !!this.selectedMember();
      case 3:
        return (
          (this.requestedAmount ?? 0) > 0 &&
          (this.tenureMonths ?? 0) > 0 &&
          (this.tenureMonths ?? 0) <= this.productMaxTenure() &&
          this.purpose.trim().length >= 10
        );
      case 4:
        return this.consentGiven && !!this.selectedMember();
      default:
        return true;
    }
  }

  getBlockedReason(): string | null {
    if (this.canProceed()) {
      return null;
    }

    switch (this.step()) {
      case 1:
        return 'Select a loan product card to continue.';
      case 2:
        return 'Search for a member, then click their name in the results list (typing alone is not enough).';
      case 3: {
        if ((this.requestedAmount ?? 0) <= 0) {
          return 'Enter a requested amount greater than zero.';
        }

        if ((this.tenureMonths ?? 0) <= 0 || (this.tenureMonths ?? 0) > this.productMaxTenure()) {
          return `Select a tenure up to ${this.productMaxTenure()} months for this product.`;
        }

        if (this.purpose.trim().length < 10) {
          return `Purpose must be at least 10 characters (currently ${this.purpose.trim().length}/10).`;
        }

        return 'Complete all loan detail fields.';
      }
      case 4:
        return this.consentGiven
          ? 'Member must be selected on the KYC step.'
          : 'Check the consent box to authorize credit assessment.';
      default:
        return null;
    }
  }

  goNext(): void {
    if (!this.canProceed()) {
      this.errorMessage.set(this.validationMessage());
      return;
    }

    this.errorMessage.set(null);

    if (this.step() === 2) {
      const maxTenure = this.productMaxTenure();
      if (this.tenureMonths != null && this.tenureMonths > maxTenure) {
        this.tenureMonths = maxTenure;
      }
      this.updatePreview();
    }

    this.step.update((value) => Math.min(5, value + 1));
  }

  goBack(): void {
    this.errorMessage.set(null);
    this.step.update((value) => Math.max(1, value - 1));
  }

  validationMessage(): string {
    switch (this.step()) {
      case 1:
        return 'Select a loan product to continue.';
      case 2:
        return 'Search and select a member for KYC verification.';
      case 3:
        return 'Enter amount, tenure within product limits, and purpose (min 10 characters).';
      case 4:
        return 'Confirm the consent checkbox to continue.';
      default:
        return 'Complete all required fields.';
    }
  }

  async submit(): Promise<void> {
    const member = this.selectedMember();
    const product = this.selectedProduct();

    if (!member || !product) {
      this.errorMessage.set('Complete all wizard steps before submitting.');
      return;
    }

    const amount = this.requestedAmount;
    const tenure = this.tenureMonths;
    if (amount == null || amount <= 0 || tenure == null || tenure <= 0) {
      this.errorMessage.set('Enter amount and tenure before submitting.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    const branchId =
      this.auth.user()?.branchId?.trim() || '00000000-0000-0000-0000-000000000010';

    try {
      const detail = await this.loanApi.create({
        memberId: member.id,
        memberNumber: member.memberNumber,
        memberName: member.fullName,
        branchId,
        productType: product.productType,
        requestedAmount: amount,
        tenureMonths: tenure,
        purpose: this.purpose.trim(),
      });

      void this.router.navigate(['/loans', detail.id]);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to submit application.'));
    } finally {
      this.loading.set(false);
    }
  }

  cancel(): void {
    void this.router.navigate(['/loans']);
  }
}
