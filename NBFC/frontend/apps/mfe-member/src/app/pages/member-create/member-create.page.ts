import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '@patsanstha/auth';
import {
  EmploymentType,
  KycVerificationStatus,
  MemberApiService,
  MemberDocumentType,
  SharePaymentMode,
  extractApiErrorMessage,
  kycStatusLabel,
} from '@patsanstha/members-data-access';
import {
  MEMBER_AADHAAR_PATTERN,
  MEMBER_MOBILE_PATTERN,
  MEMBER_PAN_PATTERN,
  MEMBER_PIN_PATTERN,
  normalizeDigits,
  normalizeMobileNumber,
  normalizePan,
} from './member-create.validation';

const STEPS = [
  { id: 1, label: 'Personal Info', labelMr: 'वैयक्तिक माहिती' },
  { id: 2, label: 'KYC Details', labelMr: 'केवायसी तपशील' },
  { id: 3, label: 'Employment', labelMr: 'रोजगार' },
  { id: 4, label: 'Preview', labelMr: 'पूर्वावलोकन' },
];

@Component({
  selector: 'pats-member-create-page',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="onboarding-page">
      <a routerLink="/members" class="onboarding-page__back">
        <span class="material-symbols-outlined">arrow_back</span>
        Back to directory
      </a>

      <main class="wizard-card">
        <header class="wizard-card__header">
          <div class="wizard-card__header-top">
            <div>
              <h1>Member Onboarding</h1>
              <p class="wizard-card__subtitle">
                <span class="material-symbols-outlined">how_to_reg</span>
                New Account Creation Wizard
              </p>
            </div>
            <div class="wizard-card__progress-pill">
              <span>Progress: {{ progressPercent() }}%</span>
              <div class="wizard-card__progress-track">
                <div class="wizard-card__progress-fill" [style.width.%]="progressPercent()"></div>
              </div>
            </div>
          </div>

          <nav class="wizard-card__stepper" aria-label="Onboarding steps">
            <div class="wizard-card__stepper-line"></div>
            @for (step of steps; track step.id) {
              <div
                class="wizard-card__step"
                [class.wizard-card__step--active]="currentStep() === step.id"
                [class.wizard-card__step--done]="currentStep() > step.id">
                <div class="wizard-card__step-circle">
                  @if (currentStep() > step.id) {
                    <span class="material-symbols-outlined">check</span>
                  } @else {
                    {{ step.id }}
                  }
                </div>
                <div class="wizard-card__step-text">
                  <span>{{ step.label }}</span>
                  <span class="wizard-card__step-mr">{{ step.labelMr }}</span>
                </div>
              </div>
            }
          </nav>
        </header>

        <div class="wizard-card__body">
          @if (errorMessage()) {
            <p class="wizard-card__error">{{ errorMessage() }}</p>
          }

          @if (currentStep() === 1) {
            <form [formGroup]="personalForm" class="step-form">
              <div class="step-form__personal">
                <div class="step-form__photo-col">
                  <label class="step-form__photo">
                    <input type="file" accept="image/jpeg,image/png" (change)="onPhotoSelected($event)" hidden />
                    @if (photoPreview()) {
                      <img [src]="photoPreview()!" alt="Member photo" />
                    } @else {
                      <span class="material-symbols-outlined step-form__photo-icon">add_a_photo</span>
                      <span class="step-form__photo-label">Member Photo</span>
                      <span class="step-form__photo-mr">फोटो अपलोड</span>
                    }
                  </label>
                  <p class="step-form__photo-hint">JPG/PNG, Max 2MB</p>
                  @if (photoSaved()) {
                    <p class="step-form__photo-hint step-form__photo-hint--saved">Photo saved locally</p>
                  }
                </div>

                <div class="step-form__fields">
                  <label class="field field--full">
                    <span class="field__label">Full Name</span>
                    <span class="field__label-mr">पूर्ण नाव</span>
                    <input type="text" formControlName="fullName" placeholder="Enter member's full name" />
                  </label>

                  <label class="field">
                    <span class="field__label">Date of Birth</span>
                    <span class="field__label-mr">जन्म तारीख</span>
                    <input type="date" formControlName="dateOfBirth" />
                  </label>

                  <label class="field">
                    <span class="field__label">Gender</span>
                    <span class="field__label-mr">लिंग</span>
                    <select formControlName="gender">
                      <option value="" disabled>Select Gender</option>
                      <option value="Male">Male / पुरुष</option>
                      <option value="Female">Female / महिला</option>
                      <option value="Other">Other / इतर</option>
                    </select>
                  </label>
                </div>
              </div>

              <div class="step-form__contact">
                <label class="field">
                  <span class="field__label">Contact Number</span>
                  <span class="field__label-mr">संपर्क क्रमांक</span>
                  <div class="field__phone">
                    <span class="field__phone-prefix">+91</span>
                    <input type="tel" formControlName="mobileNumber" placeholder="9876543210" />
                  </div>
                </label>

                <label class="field">
                  <span class="field__label">Email Address</span>
                  <span class="field__label-mr">ईमेल पत्ता</span>
                  <input type="email" formControlName="email" placeholder="example@mail.com" />
                </label>

                <label class="field field--full">
                  <span class="field__label">Permanent Address</span>
                  <span class="field__label-mr">कायमचा पत्ता</span>
                  <textarea
                    formControlName="permanentAddress"
                    rows="3"
                    placeholder="Enter house number, street, area, city, pincode"></textarea>
                </label>
              </div>
            </form>
          }

          @if (currentStep() === 2) {
            <form [formGroup]="kycForm" class="step-form step-form--kyc">
              <section class="kyc-section">
                <div class="kyc-section__head">
                  <span class="material-symbols-outlined kyc-section__icon">fingerprint</span>
                  <div>
                    <h2>Aadhaar Verification</h2>
                    <p>आधार कार्ड पडताळणी</p>
                  </div>
                </div>
                <label class="field">
                  <span class="field__label">Aadhaar Number</span>
                  <span class="field__label-mr">१२ अंकी आधार क्रमांक</span>
                  <input type="text" formControlName="aadhaar" placeholder="XXXX XXXX XXXX" maxlength="12" />
                </label>
                <button type="button" class="btn-verify" [disabled]="verifyingAadhaar()" (click)="verifyAadhaar()">
                  Verify via eKYC (OTP)
                </button>
                @if (aadhaarMessage()) {
                  <p class="msg-info">{{ aadhaarMessage() }}</p>
                }
                <label class="upload-zone">
                  <input type="file" accept="image/jpeg,image/png,application/pdf" (change)="onAadhaarDocSelected($event)" hidden />
                  <span class="material-symbols-outlined">cloud_upload</span>
                  <span>Upload Aadhaar Card / स्कॅन कॉपी</span>
                  <span class="upload-zone__hint">PDF or JPG, Max 5MB</span>
                  @if (docAadhaar()) { <span class="upload-zone__file">{{ docAadhaar()!.name }}</span> }
                  @if (savedDocName(MemberDocumentType.AadhaarCard); as savedName) {
                    <span class="upload-zone__saved">Saved locally: {{ savedName }}</span>
                  }
                </label>
              </section>

              <section class="kyc-section">
                <div class="kyc-section__head">
                  <span class="material-symbols-outlined kyc-section__icon">badge</span>
                  <div>
                    <h2>PAN Verification</h2>
                    <p>पॅन कार्ड पडताळणी</p>
                  </div>
                </div>
                <label class="field">
                  <span class="field__label">PAN Number</span>
                  <span class="field__label-mr">पॅन क्रमांक</span>
                  <input type="text" formControlName="pan" placeholder="ABCDE1234F" />
                </label>
                <button type="button" class="btn-verify" [disabled]="verifyingPan()" (click)="verifyPan()">
                  Verify PAN
                </button>
                @if (panMessage()) {
                  <p [class]="panVerified() ? 'msg-success' : 'msg-info'">{{ panMessage() }}</p>
                }
                <label class="upload-zone">
                  <input type="file" accept="image/jpeg,image/png,application/pdf" (change)="onPanDocSelected($event)" hidden />
                  <span class="material-symbols-outlined">upload_file</span>
                  <span>Upload PAN Card / स्कॅन कॉपी</span>
                  @if (docPan()) { <span class="upload-zone__file">{{ docPan()!.name }}</span> }
                  @if (savedDocName(MemberDocumentType.PanCard); as savedName) {
                    <span class="upload-zone__saved">Saved locally: {{ savedName }}</span>
                  }
                </label>
              </section>
            </form>
          }

          @if (currentStep() === 3) {
            <form [formGroup]="employmentForm" class="step-form step-form--stacked">
              <div class="section-head">
                <h2>Employment Details</h2>
                <p>रोजगार तपशील</p>
              </div>
              <div class="step-form__grid">
                <label class="field">
                  <span class="field__label">Employment Type</span>
                  <select formControlName="employmentType">
                    <option value="">Select type</option>
                    <option value="0">Salaried / पगारदार</option>
                    <option value="1">Self Employed / स्वयंरोजगार</option>
                    <option value="2">Business / व्यवसाय</option>
                    <option value="3">Retired / निवृत्त</option>
                    <option value="4">Other / इतर</option>
                  </select>
                </label>
                <label class="field">
                  <span class="field__label">Occupation</span>
                  <span class="field__label-mr">व्यवसाय</span>
                  <input type="text" formControlName="occupation" />
                </label>
                <label class="field">
                  <span class="field__label">Employer / Business Name</span>
                  <input type="text" formControlName="employerName" />
                </label>
                <label class="field">
                  <span class="field__label">Monthly Income (₹)</span>
                  <input type="number" formControlName="monthlyIncome" />
                </label>
              </div>

              <div class="section-head">
                <h2>Share Capital Details</h2>
                <p>शेअर भांडवल तपशील</p>
              </div>
              <div class="share-panel">
                <label class="field">
                  <span class="field__label">Number of Shares</span>
                  <span class="field__label-mr">शेअर्सची संख्या</span>
                  <input type="number" formControlName="numberOfShares" min="1" />
                </label>
                <label class="field">
                  <span class="field__label">Face Value (₹)</span>
                  <span class="field__label-mr">दर्शनी मूल्य</span>
                  <input type="number" formControlName="shareFaceValue" readonly />
                </label>
                <label class="field field--highlight">
                  <span class="field__label">Total Amount (₹)</span>
                  <span class="field__label-mr">एकूण रक्कम</span>
                  <input type="number" formControlName="totalShareAmount" readonly />
                </label>
              </div>
              <label class="field">
                <span class="field__label">Payment Mode</span>
                <span class="field__label-mr">पेमेंट मोड</span>
                <select formControlName="sharePaymentMode">
                  <option value="">Select payment mode</option>
                  <option value="0">Cash / नकद</option>
                  <option value="1">Bank Transfer / बँक हस्तांतरण</option>
                  <option value="2">Cheque / धनादेश</option>
                </select>
              </label>

              <div class="section-head section-head--accent">
                <h2>Nominee Details</h2>
                <p>वारसदार तपशील</p>
              </div>
              <div class="step-form__grid">
                <label class="field">
                  <span class="field__label">Nominee Full Name</span>
                  <span class="field__label-mr">वारसदाराचे पूर्ण नाव</span>
                  <input type="text" formControlName="nomineeName" placeholder="Enter full legal name" />
                </label>
                <label class="field">
                  <span class="field__label">Relationship</span>
                  <span class="field__label-mr">नाते</span>
                  <select formControlName="nomineeRelation">
                    <option value="">Select relationship</option>
                    <option value="Spouse">Spouse / जोडीदार</option>
                    <option value="Son">Son / मुलगा</option>
                    <option value="Daughter">Daughter / मुलगी</option>
                    <option value="Father">Father / वडील</option>
                    <option value="Mother">Mother / आई</option>
                    <option value="Other">Other / इतर</option>
                  </select>
                </label>
                <label class="field">
                  <span class="field__label">Date of Birth</span>
                  <span class="field__label-mr">जन्म तारीख</span>
                  <input type="date" formControlName="nomineeDateOfBirth" />
                </label>
                <label class="field">
                  <span class="field__label">Share (%)</span>
                  <input type="number" formControlName="nomineeSharePercent" min="1" max="100" />
                </label>
              </div>
            </form>
          }

          @if (currentStep() === 4) {
            <div class="step-form step-form--review">
              <h2>Review Your Application</h2>
              <p class="step-form__review-lead">
                Please verify all the details provided before submitting your membership application.
              </p>
              <div class="review-grid">
                <article class="review-card">
                  <header>
                    <span class="material-symbols-outlined">person</span>
                    <div>
                      <h3>Personal Information</h3>
                      <span>वैयक्तिक माहिती</span>
                    </div>
                  </header>
                  <dl>
                    <dt>Full Name</dt><dd>{{ personalForm.value.fullName }}</dd>
                    <dt>Contact Number</dt><dd>+91 {{ personalForm.value.mobileNumber }}</dd>
                    <dt>Email</dt><dd>{{ personalForm.value.email || '—' }}</dd>
                  </dl>
                </article>
                <article class="review-card">
                  <header>
                    <span class="material-symbols-outlined">family_history</span>
                    <div>
                      <h3>Nominee Details</h3>
                      <span>वारसदार तपशील</span>
                    </div>
                  </header>
                  <dl>
                    <dt>Nominee Name</dt><dd>{{ employmentForm.value.nomineeName }}</dd>
                    <dt>Relationship</dt><dd>{{ employmentForm.value.nomineeRelation }}</dd>
                    <dt>Share (%)</dt><dd>{{ employmentForm.value.nomineeSharePercent }}%</dd>
                  </dl>
                </article>
                <article class="review-card review-card--wide">
                  <header>
                    <span class="material-symbols-outlined">description</span>
                    <div>
                      <h3>KYC Documents</h3>
                      <span>केवायसी कागदपत्रे</span>
                    </div>
                  </header>
                  <ul class="review-docs">
                    @if (docAadhaar()) { <li>{{ docAadhaar()!.name }}</li> }
                    @if (docPan()) { <li>{{ docPan()!.name }}</li> }
                    @if (photoFile()) { <li>{{ photoFile()!.name }}</li> }
                    <li>Aadhaar: {{ kycStatusLabel(aadhaarStatus()) }}</li>
                    <li>PAN: {{ kycStatusLabel(panStatus()) }}</li>
                  </ul>
                </article>
              </div>
              <label class="terms-check">
                <input type="checkbox" [checked]="termsAccepted()" (change)="termsAccepted.set($any($event.target).checked)" />
                <span>
                  I agree to the terms and conditions of the Sahakari Bank.
                  <em>मी सहकारी बँकेच्या अटी आणि शर्तींशी सहमत आहे.</em>
                </span>
              </label>
            </div>
          }
        </div>

        <footer class="wizard-card__footer">
          @if (currentStep() > 1) {
            <button type="button" class="btn-ghost" (click)="prevStep()">
              <span class="material-symbols-outlined">arrow_back</span>
              Back
            </button>
          } @else {
            <button type="button" class="btn-outline" (click)="cancel()">Cancel</button>
          }

          <div class="wizard-card__footer-actions">
            <button type="button" class="btn-outline" [disabled]="savingDraft()" (click)="saveDraft()">
              <span class="material-symbols-outlined">drafts</span>
              Save Draft
            </button>
            @if (currentStep() < 4) {
              <button type="button" class="btn-primary" [disabled]="loading()" (click)="nextStep()">
                Next Step
                <span class="material-symbols-outlined">arrow_forward</span>
              </button>
            } @else {
              <button type="button" class="btn-primary" [disabled]="loading() || !termsAccepted()" (click)="submit()">
                Submit Application
              </button>
            }
          </div>
        </footer>
      </main>
    </div>
  `,
  styles: [
    `
      .onboarding-page {
        position: relative;
        display: flex;
        flex-direction: column;
        align-items: stretch;
        width: 100%;
        margin: 0;
        padding: 8px 0 32px;
      }

      .onboarding-page__back {
        align-self: flex-start;
        display: inline-flex;
        align-items: center;
        gap: 4px;
        margin-bottom: 16px;
        color: var(--pats-color-primary-container);
        font-size: 14px;
        font-weight: 600;
      }

      .wizard-card {
        width: 100%;
        overflow: hidden;
        border: 1px solid var(--pats-color-border-subtle);
        border-top: 4px solid var(--pats-color-primary-container);
        border-radius: var(--pats-radius-lg);
        background: rgba(255, 255, 255, 0.92);
        box-shadow: var(--pats-shadow-card);
      }

      .wizard-card__header {
        padding: 24px 32px;
        background: var(--pats-color-surface-container-low);
        border-bottom: 1px solid var(--pats-color-border-subtle);
      }

      .wizard-card__header-top {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        gap: 24px;
        margin-bottom: 28px;
        flex-wrap: wrap;
      }

      .wizard-card__header h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 32px;
        line-height: 1.3;
        color: var(--pats-color-primary);
      }

      .wizard-card__subtitle {
        display: flex;
        align-items: center;
        gap: 6px;
        margin: 6px 0 0;
        font-size: 14px;
        color: var(--pats-color-on-surface-variant);
      }

      .wizard-card__subtitle .material-symbols-outlined {
        font-size: 18px;
      }

      .wizard-card__progress-pill {
        display: flex;
        flex-direction: column;
        gap: 8px;
        min-width: 140px;
        padding: 10px 16px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: 999px;
        background: #fff;
        box-shadow: 0 1px 2px rgba(0, 0, 0, 0.04);
        font-size: 13px;
        font-weight: 600;
        color: var(--pats-color-primary);
      }

      .wizard-card__progress-track {
        height: 8px;
        border-radius: 999px;
        background: var(--pats-color-surface-container-highest);
        overflow: hidden;
      }

      .wizard-card__progress-fill {
        height: 100%;
        background: var(--pats-color-primary);
        transition: width 0.35s ease;
      }

      .wizard-card__stepper {
        position: relative;
        display: grid;
        grid-template-columns: repeat(4, 1fr);
        gap: 8px;
      }

      .wizard-card__stepper-line {
        position: absolute;
        top: 20px;
        left: 12%;
        right: 12%;
        height: 2px;
        background: var(--pats-color-surface-container-highest);
        z-index: 0;
      }

      .wizard-card__step {
        position: relative;
        z-index: 1;
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 8px;
        text-align: center;
        opacity: 0.55;
      }

      .wizard-card__step--active,
      .wizard-card__step--done {
        opacity: 1;
      }

      .wizard-card__step-circle {
        width: 40px;
        height: 40px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 50%;
        font-weight: 700;
        background: var(--pats-color-surface-container-high);
        color: var(--pats-color-on-surface-variant);
      }

      .wizard-card__step--active .wizard-card__step-circle {
        background: var(--pats-color-primary-container);
        color: #fff;
        box-shadow: 0 0 0 4px rgba(26, 60, 110, 0.12);
      }

      .wizard-card__step--done .wizard-card__step-circle {
        background: var(--pats-color-secondary);
        color: #fff;
      }

      .wizard-card__step-text {
        font-size: 13px;
        font-weight: 600;
        color: var(--pats-color-on-surface-variant);
      }

      .wizard-card__step--active .wizard-card__step-text {
        color: var(--pats-color-primary-container);
      }

      .wizard-card__step-mr {
        display: block;
        margin-top: 2px;
        font-size: 11px;
        font-weight: 500;
      }

      .wizard-card__body {
        padding: 32px 32px 8px;
      }

      .wizard-card__error {
        margin: 0 0 16px;
        padding: 12px 16px;
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-error-container);
        color: var(--pats-color-error);
        font-size: 14px;
      }

      .step-form__personal {
        display: grid;
        grid-template-columns: 160px 1fr;
        gap: 24px;
        margin-bottom: 8px;
      }

      .step-form__photo-col {
        display: flex;
        flex-direction: column;
        align-items: center;
      }

      .step-form__photo {
        width: 160px;
        height: 160px;
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        gap: 6px;
        padding: 16px;
        border: 2px dashed var(--pats-color-outline-variant);
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        cursor: pointer;
        text-align: center;
        transition: border-color 0.2s;
      }

      .step-form__photo:hover {
        border-color: var(--pats-color-primary-container);
      }

      .step-form__photo img {
        width: 100%;
        height: 100%;
        object-fit: cover;
        border-radius: var(--pats-radius-md);
      }

      .step-form__photo-icon {
        font-size: 40px;
        color: var(--pats-color-outline);
      }

      .step-form__photo-label {
        font-size: 12px;
        font-weight: 600;
        color: var(--pats-color-outline);
      }

      .step-form__photo-mr {
        font-size: 11px;
        color: var(--pats-color-on-surface-variant);
        opacity: 0.8;
      }

      .step-form__photo-hint {
        margin: 8px 0 0;
        font-size: 11px;
        color: var(--pats-color-on-surface-variant);
        text-align: center;
      }

      .step-form__photo-hint--saved {
        color: var(--pats-color-secondary);
        font-weight: 600;
      }

      .step-form__fields,
      .step-form__contact,
      .step-form__grid {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 20px;
      }

      .step-form__contact {
        margin-top: 24px;
        padding-top: 24px;
        border-top: 1px solid var(--pats-color-surface-container-highest);
      }

      .field {
        display: flex;
        flex-direction: column;
        gap: 6px;
      }

      .field--full {
        grid-column: 1 / -1;
      }

      .field__label {
        font-size: 13px;
        font-weight: 600;
        color: var(--pats-color-on-surface);
      }

      .field__label-mr {
        font-size: 11px;
        font-weight: 500;
        color: var(--pats-color-on-surface-variant);
        margin-top: -4px;
      }

      .field input,
      .field select,
      .field textarea {
        width: 100%;
        padding: 12px 14px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        background: #fff;
        font-family: var(--pats-font-body);
        font-size: 16px;
        color: var(--pats-color-on-surface);
        transition: border-color 0.15s, box-shadow 0.15s;
      }

      .field input:focus,
      .field select:focus,
      .field textarea:focus {
        outline: none;
        border-color: var(--pats-color-primary);
        box-shadow: 0 0 0 2px rgba(0, 38, 83, 0.12);
      }

      .field textarea {
        resize: vertical;
        min-height: 96px;
      }

      .field__phone {
        display: flex;
      }

      .field__phone-prefix {
        display: inline-flex;
        align-items: center;
        padding: 0 12px;
        border: 1px solid var(--pats-color-border-subtle);
        border-right: none;
        border-radius: var(--pats-radius-md) 0 0 var(--pats-radius-md);
        background: var(--pats-color-surface-muted);
        color: var(--pats-color-on-surface-variant);
        font-size: 14px;
        font-weight: 600;
      }

      .field__phone input {
        border-radius: 0 var(--pats-radius-md) var(--pats-radius-md) 0;
      }

      .step-form--kyc {
        display: flex;
        flex-direction: column;
        gap: 40px;
      }

      .kyc-section__head {
        display: flex;
        align-items: center;
        gap: 12px;
        margin-bottom: 20px;
      }

      .kyc-section__icon {
        width: 36px;
        height: 36px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-primary-fixed);
        color: var(--pats-color-primary);
      }

      .kyc-section__head h2 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 18px;
        color: var(--pats-color-on-surface);
      }

      .kyc-section__head p {
        margin: 2px 0 0;
        font-size: 12px;
        color: var(--pats-color-on-surface-variant);
      }

      .btn-verify {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        margin-top: 12px;
        padding: 10px 18px;
        border: none;
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-primary-container);
        color: #fff;
        font-size: 14px;
        font-weight: 600;
        cursor: pointer;
      }

      .btn-verify:disabled {
        opacity: 0.6;
        cursor: not-allowed;
      }

      .upload-zone {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 8px;
        margin-top: 16px;
        padding: 28px 20px;
        border: 2px dashed var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-surface-muted);
        text-align: center;
        cursor: pointer;
        font-size: 14px;
        color: var(--pats-color-on-surface-variant);
      }

      .upload-zone__hint,
      .upload-zone__file,
      .upload-zone__saved {
        font-size: 12px;
      }

      .upload-zone__saved {
        color: var(--pats-color-secondary);
        font-weight: 600;
      }

      .upload-zone__file {
        color: var(--pats-color-secondary);
        font-weight: 600;
      }

      .msg-info {
        margin: 12px 0 0;
        padding: 12px 16px;
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-primary-fixed);
        font-size: 14px;
      }

      .msg-success {
        margin: 12px 0 0;
        padding: 12px 16px;
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-secondary-container);
        color: var(--pats-color-on-secondary-container);
        font-size: 14px;
      }

      .step-form--stacked {
        display: flex;
        flex-direction: column;
        gap: 20px;
      }

      .section-head {
        padding-left: 16px;
        border-left: 4px solid var(--pats-color-primary-container);
      }

      .section-head--accent {
        border-left-color: var(--pats-color-on-tertiary-container);
      }

      .section-head h2 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 18px;
        color: var(--pats-color-primary-container);
      }

      .section-head p {
        margin: 4px 0 0;
        font-size: 12px;
        color: var(--pats-color-on-surface-variant);
      }

      .share-panel {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 16px;
        padding: 20px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-surface-muted);
      }

      .field--highlight input {
        background: var(--pats-color-primary-fixed);
        font-weight: 700;
        color: var(--pats-color-primary);
      }

      .step-form--review h2 {
        margin: 0 0 8px;
        font-family: var(--pats-font-display);
        font-size: 28px;
        color: var(--pats-color-primary);
      }

      .step-form__review-lead {
        margin: 0 0 24px;
        color: var(--pats-color-on-surface-variant);
      }

      .review-grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 16px;
      }

      .review-card {
        padding: 20px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        background: #fff;
      }

      .review-card--wide {
        grid-column: 1 / -1;
      }

      .review-card header {
        display: flex;
        align-items: flex-start;
        gap: 10px;
        margin-bottom: 16px;
        color: var(--pats-color-primary-container);
      }

      .review-card header h3 {
        margin: 0;
        font-size: 16px;
      }

      .review-card header span {
        display: block;
        font-size: 11px;
        color: var(--pats-color-on-surface-variant);
      }

      .review-card dl {
        margin: 0;
        display: grid;
        gap: 12px;
      }

      .review-card dt {
        font-size: 11px;
        text-transform: uppercase;
        letter-spacing: 0.04em;
        color: var(--pats-color-on-surface-variant);
      }

      .review-card dd {
        margin: 2px 0 0;
        font-weight: 500;
      }

      .review-docs {
        margin: 0;
        padding-left: 20px;
      }

      .terms-check {
        display: flex;
        gap: 12px;
        align-items: flex-start;
        margin-top: 24px;
        padding: 20px;
        border-radius: var(--pats-radius-md);
        background: rgba(26, 60, 110, 0.05);
        cursor: pointer;
      }

      .terms-check input {
        width: 20px;
        height: 20px;
        margin-top: 2px;
      }

      .terms-check em {
        display: block;
        margin-top: 4px;
        font-style: normal;
        font-size: 13px;
        color: var(--pats-color-on-surface-variant);
      }

      .wizard-card__footer {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 16px;
        padding: 20px 32px;
        background: var(--pats-color-surface-container-low);
        border-top: 1px solid var(--pats-color-border-subtle);
        flex-wrap: wrap;
      }

      .wizard-card__footer-actions {
        display: flex;
        gap: 12px;
        flex-wrap: wrap;
      }

      .btn-ghost,
      .btn-outline,
      .btn-primary {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 8px;
        min-height: 44px;
        padding: 0 20px;
        border-radius: var(--pats-radius-md);
        font-size: 14px;
        font-weight: 600;
        cursor: pointer;
        transition: background 0.15s, transform 0.1s;
      }

      .btn-ghost {
        border: 1px solid transparent;
        background: transparent;
        color: var(--pats-color-primary);
      }

      .btn-outline {
        border: 1px solid var(--pats-color-primary);
        background: transparent;
        color: var(--pats-color-primary);
      }

      .btn-outline:hover:not(:disabled) {
        background: rgba(0, 38, 83, 0.05);
      }

      .btn-primary {
        border: none;
        padding: 0 28px;
        background: var(--pats-color-primary);
        color: #fff;
        box-shadow: 0 8px 20px rgba(0, 38, 83, 0.18);
      }

      .btn-primary:hover:not(:disabled) {
        background: var(--pats-color-primary-container);
      }

      .btn-primary:disabled,
      .btn-outline:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }

      @media (max-width: 768px) {
        .wizard-card__header,
        .wizard-card__body,
        .wizard-card__footer {
          padding-left: 20px;
          padding-right: 20px;
        }

        .wizard-card__stepper-line,
        .wizard-card__step-text {
          display: none;
        }

        .step-form__personal,
        .step-form__fields,
        .step-form__contact,
        .step-form__grid,
        .share-panel,
        .review-grid {
          grid-template-columns: 1fr;
        }

        .wizard-card__footer {
          flex-direction: column;
          align-items: stretch;
        }

        .wizard-card__footer-actions {
          flex-direction: column;
        }
      }
    `,
  ],
})
export class MemberCreatePageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly memberApi = inject(MemberApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly steps = STEPS;
  readonly kycStatusLabel = kycStatusLabel;
  readonly MemberDocumentType = MemberDocumentType;
  readonly currentStep = signal(1);
  readonly loading = signal(false);
  readonly savingDraft = signal(false);
  readonly verifyingAadhaar = signal(false);
  readonly verifyingPan = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly draftMemberId = signal<string | null>(null);
  readonly photoFile = signal<File | null>(null);
  readonly photoPreview = signal<string | null>(null);
  readonly photoSaved = signal(false);
  readonly docAadhaar = signal<File | null>(null);
  readonly docPan = signal<File | null>(null);
  readonly savedDocuments = signal<
    Array<{ type: MemberDocumentType; fileName: string; storageKey: string; fileUrl: string }>
  >([]);
  readonly aadhaarMessage = signal<string | null>(null);
  readonly panMessage = signal<string | null>(null);
  readonly aadhaarStatus = signal(KycVerificationStatus.Pending);
  readonly panStatus = signal(KycVerificationStatus.Pending);
  readonly panVerified = signal(false);
  readonly termsAccepted = signal(false);

  readonly progressPercent = computed(() => Math.round((this.currentStep() / 4) * 100));

  readonly personalForm = this.fb.nonNullable.group({
    fullName: ['', Validators.required],
    dateOfBirth: ['', Validators.required],
    gender: ['', Validators.required],
    mobileNumber: ['', [Validators.required, Validators.pattern(MEMBER_MOBILE_PATTERN)]],
    email: [''],
    permanentAddress: ['', Validators.required],
  });

  readonly kycForm = this.fb.nonNullable.group({
    aadhaar: ['', [Validators.required, Validators.pattern(MEMBER_AADHAAR_PATTERN)]],
    pan: ['', [Validators.required, Validators.pattern(MEMBER_PAN_PATTERN)]],
  });

  readonly employmentForm = this.fb.nonNullable.group({
    employmentType: ['', Validators.required],
    occupation: ['', Validators.required],
    employerName: [''],
    monthlyIncome: [''],
    numberOfShares: [10, [Validators.required, Validators.min(1)]],
    shareFaceValue: [{ value: 100, disabled: true }],
    totalShareAmount: [{ value: 1000, disabled: true }],
    sharePaymentMode: ['', Validators.required],
    nomineeName: ['', Validators.required],
    nomineeRelation: ['', Validators.required],
    nomineeDateOfBirth: ['', Validators.required],
    nomineeSharePercent: [100, [Validators.required, Validators.min(1), Validators.max(100)]],
  });

  constructor() {
    this.employmentForm.controls.numberOfShares.valueChanges.subscribe((shares) => {
      const total = (shares ?? 0) * 100;
      this.employmentForm.controls.totalShareAmount.setValue(total);
    });
  }

  onPhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    if (file.size > 2 * 1024 * 1024) {
      this.errorMessage.set('Photo must be 2MB or smaller.');
      return;
    }
    this.photoFile.set(file);
    this.photoPreview.set(URL.createObjectURL(file));
    this.errorMessage.set(null);
    void this.tryUploadDocument(MemberDocumentType.Photo, file, () => this.photoFile.set(null));
  }

  onAadhaarDocSelected(event: Event): void {
    this.onDocSelected(event, this.docAadhaar, MemberDocumentType.AadhaarCard);
  }

  onPanDocSelected(event: Event): void {
    this.onDocSelected(event, this.docPan, MemberDocumentType.PanCard);
  }

  savedDocName(type: MemberDocumentType): string | null {
    return this.savedDocuments().find((doc) => doc.type === type)?.fileName ?? null;
  }

  onDocSelected(
    event: Event,
    target: { set: (file: File | null) => void },
    documentType: MemberDocumentType
  ): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    if (file.size > 5 * 1024 * 1024) {
      this.errorMessage.set('Document must be 5MB or smaller.');
      return;
    }
    target.set(file);
    this.errorMessage.set(null);
    void this.tryUploadDocument(documentType, file, () => target.set(null));
  }

  async verifyAadhaar(): Promise<void> {
    const aadhaar = normalizeDigits(this.kycForm.controls.aadhaar.value);
    this.kycForm.controls.aadhaar.setValue(aadhaar);
    if (this.kycForm.controls.aadhaar.invalid) {
      this.kycForm.controls.aadhaar.markAsTouched();
      return;
    }
    const memberId = await this.ensureDraft(2);
    if (!memberId) return;

    this.verifyingAadhaar.set(true);
    try {
      const result = await this.memberApi.verifyAadhaar(memberId, aadhaar);
      this.aadhaarStatus.set(result.status);
      this.aadhaarMessage.set(result.message);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Aadhaar verification failed.'));
    } finally {
      this.verifyingAadhaar.set(false);
    }
  }

  async verifyPan(): Promise<void> {
    const pan = normalizePan(this.kycForm.controls.pan.value);
    this.kycForm.controls.pan.setValue(pan);
    if (this.kycForm.controls.pan.invalid) {
      this.kycForm.controls.pan.markAsTouched();
      return;
    }
    const memberId = await this.ensureDraft(2);
    if (!memberId) return;

    this.verifyingPan.set(true);
    try {
      const result = await this.memberApi.verifyPan(memberId, pan);
      this.panStatus.set(result.status);
      this.panVerified.set(result.status === KycVerificationStatus.Verified);
      this.panMessage.set(result.message);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'PAN verification failed.'));
    } finally {
      this.verifyingPan.set(false);
    }
  }

  async saveDraft(): Promise<void> {
    this.savingDraft.set(true);
    this.errorMessage.set(null);
    try {
      await this.persistDraft(this.currentStep());
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Unable to save draft.'));
    } finally {
      this.savingDraft.set(false);
    }
  }

  async nextStep(): Promise<void> {
    this.errorMessage.set(null);
    if (!this.validateCurrentStep()) return;

    this.loading.set(true);
    try {
      await this.persistDraft(this.currentStep());
      await this.uploadPendingDocuments();
      this.currentStep.update((s) => Math.min(s + 1, 4));
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Unable to proceed to next step.'));
    } finally {
      this.loading.set(false);
    }
  }

  prevStep(): void {
    this.currentStep.update((s) => Math.max(s - 1, 1));
  }

  async submit(): Promise<void> {
    if (!this.termsAccepted()) return;
    this.loading.set(true);
    this.errorMessage.set(null);
    try {
      const memberId = await this.persistDraft(4);
      if (!memberId) return;
      await this.uploadPendingDocuments();
      const submitted = await this.memberApi.submitOnboarding(memberId);
      await this.router.navigate(['/members', submitted.id]);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Unable to submit application.'));
    } finally {
      this.loading.set(false);
    }
  }

  cancel(): void {
    void this.router.navigate(['/members']);
  }

  private validateCurrentStep(): boolean {
    if (this.currentStep() === 1) {
      this.personalForm.markAllAsTouched();
      if (this.personalForm.invalid) return false;
      const pin = this.extractPinCode(this.personalForm.value.permanentAddress ?? '');
      if (!MEMBER_PIN_PATTERN.test(pin)) {
        this.errorMessage.set('Include a valid 6-digit PIN code in the permanent address.');
        return false;
      }
      return true;
    }
    if (this.currentStep() === 2) {
      this.kycForm.markAllAsTouched();
      return this.kycForm.valid;
    }
    if (this.currentStep() === 3) {
      this.employmentForm.markAllAsTouched();
      return this.employmentForm.valid;
    }
    return true;
  }

  private async ensureDraft(step: number): Promise<string | null> {
    if (this.draftMemberId()) return this.draftMemberId();
    try {
      return await this.persistDraft(step);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Unable to create draft.'));
      return null;
    }
  }

  private async persistDraft(step: number): Promise<string> {
    const branchId = this.auth.user()?.branchId?.trim() || '00000000-0000-0000-0000-000000000010';
    const personal = this.personalForm.getRawValue();
    const kyc = this.kycForm.getRawValue();
    const employment = this.employmentForm.getRawValue();
    const address = this.parseAddress(personal.permanentAddress ?? '');

    const saved = await this.memberApi.saveDraft({
      memberId: this.draftMemberId(),
      branchId,
      onboardingStep: step,
      fullName: personal.fullName?.trim() || null,
      dateOfBirth: personal.dateOfBirth || null,
      gender: personal.gender?.trim() || null,
      mobileNumber: personal.mobileNumber ? normalizeMobileNumber(personal.mobileNumber) : null,
      email: personal.email?.trim() || null,
      addressLine1: address.addressLine1,
      city: address.city,
      state: address.state,
      pinCode: address.pinCode,
      aadhaar: kyc.aadhaar ? normalizeDigits(kyc.aadhaar) : null,
      pan: kyc.pan ? normalizePan(kyc.pan) : null,
      nomineeName: employment.nomineeName?.trim() || null,
      nomineeRelation: employment.nomineeRelation || null,
      nomineeDateOfBirth: employment.nomineeDateOfBirth || null,
      nomineeSharePercent: employment.nomineeSharePercent ?? null,
      nomineeAddressSameAsMember: true,
      numberOfShares: employment.numberOfShares ?? null,
      shareFaceValue: 100,
      sharePaymentMode: employment.sharePaymentMode !== '' ? Number(employment.sharePaymentMode) as SharePaymentMode : null,
      employmentType: employment.employmentType !== '' ? Number(employment.employmentType) as EmploymentType : null,
      occupation: employment.occupation?.trim() || null,
      employerName: employment.employerName?.trim() || null,
      monthlyIncome: employment.monthlyIncome ? Number(employment.monthlyIncome) : null,
    });

    this.draftMemberId.set(saved.id);
    return saved.id;
  }

  private async tryUploadDocument(
    documentType: MemberDocumentType,
    file: File,
    clearPending: () => void
  ): Promise<void> {
    const step = documentType === MemberDocumentType.Photo ? 1 : 2;
    const memberId = this.draftMemberId() ?? (await this.ensureDraft(step));
    if (!memberId) {
      return;
    }

    try {
      const detail = await this.memberApi.uploadDocument(memberId, documentType, file);
      this.applyUploadedDocuments(detail);
      clearPending();
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to save document locally.'));
    }
  }

  private applyUploadedDocuments(detail: {
    photoUrl: string | null;
    documents: Array<{ documentType: MemberDocumentType; fileName: string; storageKey: string; fileUrl: string }>;
  }): void {
    const photoUrl = this.memberApi.resolveMemberFileUrl(detail.photoUrl);
    if (photoUrl) {
      this.photoPreview.set(photoUrl);
      this.photoSaved.set(true);
    }

    this.savedDocuments.set(
      detail.documents.map((doc) => ({
        type: doc.documentType,
        fileName: doc.fileName,
        storageKey: doc.storageKey,
        fileUrl: this.memberApi.resolveMemberFileUrl(doc.fileUrl) ?? doc.fileUrl,
      }))
    );
  }

  private async uploadPendingDocuments(): Promise<void> {
    const memberId = this.draftMemberId();
    if (!memberId) return;

    const uploads: Promise<unknown>[] = [];
    const photo = this.photoFile();
    if (photo) {
      uploads.push(
        this.memberApi.uploadDocument(memberId, MemberDocumentType.Photo, photo).then((detail) => {
          this.applyUploadedDocuments(detail);
          this.photoFile.set(null);
        })
      );
    }
    const aadhaarDoc = this.docAadhaar();
    if (aadhaarDoc) {
      uploads.push(
        this.memberApi
          .uploadDocument(memberId, MemberDocumentType.AadhaarCard, aadhaarDoc)
          .then((detail) => {
            this.applyUploadedDocuments(detail);
            this.docAadhaar.set(null);
          })
      );
    }
    const panDoc = this.docPan();
    if (panDoc) {
      uploads.push(
        this.memberApi.uploadDocument(memberId, MemberDocumentType.PanCard, panDoc).then((detail) => {
          this.applyUploadedDocuments(detail);
          this.docPan.set(null);
        })
      );
    }

    if (uploads.length > 0) {
      await Promise.all(uploads);
    }
  }

  private parseAddress(permanentAddress: string): {
    addressLine1: string;
    city: string;
    state: string;
    pinCode: string;
  } {
    const pinCode = this.extractPinCode(permanentAddress);
    const parts = permanentAddress.split(',').map((p) => p.trim()).filter(Boolean);
    const city = parts.length >= 2 ? parts[parts.length - 2] : 'Maharashtra';
    const state = parts.length >= 1 ? parts[parts.length - 1].replace(pinCode, '').trim() || 'Maharashtra' : 'Maharashtra';

    return {
      addressLine1: permanentAddress.trim(),
      city: city.replace(/\d{6}/, '').trim() || 'Pune',
      state: state || 'Maharashtra',
      pinCode: pinCode || '411001',
    };
  }

  private extractPinCode(address: string): string {
    const match = address.match(/\b(\d{6})\b/);
    return match?.[1] ?? '';
  }
}
