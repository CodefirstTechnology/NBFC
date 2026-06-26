import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '@patsanstha/auth';
import { CreateMemberRequest, MemberApiService, extractApiErrorMessage } from '@patsanstha/members-data-access';
import { PatsButtonComponent, PatsFormFieldComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-member-create-page',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, PatsButtonComponent, PatsFormFieldComponent],
  template: `
    <section class="create-page">
      <header class="create-page__header">
        <a routerLink="/members" class="create-page__back">
          <span class="material-symbols-outlined">arrow_back</span>
          Back to directory
        </a>
        <h1>Member Onboarding <span class="create-page__subtitle">/ सभासद नोंदणी</span></h1>
        <p>Step 1 — Personal information. All fields are validated server-side.</p>
      </header>

      <form class="create-page__form" [formGroup]="form" (ngSubmit)="submit()">
        <div class="create-page__grid">
          <pats-form-field label="Full Name / पूर्ण नाव" formControlName="fullName" />
          <pats-form-field label="Gender / लिंग" formControlName="gender" />
          <pats-form-field label="Date of Birth" type="date" formControlName="dateOfBirth" />
          <pats-form-field label="Mobile / मोबाईल" type="tel" formControlName="mobileNumber" />
          <pats-form-field label="Email (optional)" type="email" formControlName="email" />
          <pats-form-field label="Address Line 1" formControlName="addressLine1" />
          <pats-form-field label="Address Line 2 (optional)" formControlName="addressLine2" />
          <pats-form-field label="City / शहर" formControlName="city" />
          <pats-form-field label="State / राज्य" formControlName="state" />
          <pats-form-field label="PIN Code" formControlName="pinCode" />
          <pats-form-field label="Aadhaar / आधार" formControlName="aadhaar" />
          <pats-form-field label="PAN" formControlName="pan" />
          <pats-form-field label="Nominee Name (optional)" formControlName="nomineeName" />
          <pats-form-field label="Nominee Relation (optional)" formControlName="nomineeRelation" />
        </div>

        @if (errorMessage()) {
          <p class="create-page__error">{{ errorMessage() }}</p>
        }

        <div class="create-page__actions">
          <pats-button variant="ghost" type="button" (clicked)="cancel()">Cancel</pats-button>
          <pats-button type="submit" [loading]="loading()" [disabled]="form.invalid">Create Member</pats-button>
        </div>
      </form>
    </section>
  `,
  styles: [
    `
      .create-page {
        display: flex;
        flex-direction: column;
        gap: 24px;
      }

      .create-page__header h1 {
        margin: 12px 0 0;
        font-family: var(--pats-font-display);
        font-size: 32px;
        color: var(--pats-color-primary);
      }

      .create-page__subtitle {
        font-size: 20px;
        font-weight: 400;
        color: var(--pats-color-on-surface-variant);
      }

      .create-page__header p {
        margin: 8px 0 0;
        color: var(--pats-color-text-secondary);
      }

      .create-page__back {
        display: inline-flex;
        align-items: center;
        gap: 4px;
        color: var(--pats-color-primary-container);
        font-size: 14px;
        font-weight: 600;
      }

      .create-page__form {
        padding: 32px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
        box-shadow: var(--pats-shadow-card);
      }

      .create-page__grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
        gap: 20px;
      }

      .create-page__error {
        margin: 16px 0 0;
        color: var(--pats-color-error);
      }

      .create-page__actions {
        display: flex;
        justify-content: flex-end;
        gap: 12px;
        margin-top: 24px;
      }
    `,
  ],
})
export class MemberCreatePageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly memberApi = inject(MemberApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    fullName: ['', Validators.required],
    gender: ['', Validators.required],
    dateOfBirth: ['', Validators.required],
    mobileNumber: ['', Validators.required],
    email: [''],
    addressLine1: ['', Validators.required],
    addressLine2: [''],
    city: ['', Validators.required],
    state: ['', Validators.required],
    pinCode: ['', Validators.required],
    aadhaar: ['', Validators.required],
    pan: ['', Validators.required],
    nomineeName: [''],
    nomineeRelation: [''],
  });

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    const values = this.form.getRawValue();
    const branchId =
      this.auth.user()?.branchId?.trim() ||
      '00000000-0000-0000-0000-000000000010';

    const request: CreateMemberRequest = {
      branchId,
      fullName: values.fullName,
      dateOfBirth: values.dateOfBirth,
      gender: values.gender,
      mobileNumber: values.mobileNumber,
      email: values.email || null,
      addressLine1: values.addressLine1,
      addressLine2: values.addressLine2 || null,
      city: values.city,
      state: values.state,
      pinCode: values.pinCode,
      aadhaar: values.aadhaar,
      pan: values.pan.toUpperCase(),
      nomineeName: values.nomineeName || null,
      nomineeRelation: values.nomineeRelation || null,
    };

    try {
      const created = await this.memberApi.create(request);
      await this.router.navigate(['/members', created.id]);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Unable to create member.'));
    } finally {
      this.loading.set(false);
    }
  }

  cancel(): void {
    void this.router.navigate(['/members']);
  }
}
