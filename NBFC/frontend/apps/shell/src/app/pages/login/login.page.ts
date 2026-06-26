import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AuthService } from '@patsanstha/auth';
import { PatsButtonComponent, PatsFormFieldComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-login-page',
  standalone: true,
  imports: [ReactiveFormsModule, PatsButtonComponent, PatsFormFieldComponent],
  template: `
    <main class="login">
      <section class="login__panel">
        <div class="login__brand">
          <div class="login__brand-icon">
            <span class="material-symbols-outlined">account_balance</span>
          </div>
          <div>
            <h1>Patsanstha Login</h1>
            <p>Access your cooperative banking dashboard.</p>
          </div>
        </div>

        <form class="login__form" [formGroup]="form" (ngSubmit)="submit()">
          <pats-form-field
            label="Email / ईमेल"
            hint="Staff email address"
            type="email"
            autocomplete="email"
            placeholder="admin@patsanstha.local"
            formControlName="email"
            [error]="fieldError('email')" />

          <pats-form-field
            label="Password / पासवर्ड"
            type="password"
            autocomplete="current-password"
            placeholder="Enter your password"
            formControlName="password"
            [error]="fieldError('password')" />

          @if (errorMessage()) {
            <p class="login__error">{{ errorMessage() }}</p>
          }

          <pats-button type="submit" [loading]="loading()" [disabled]="form.invalid">
            Sign in
          </pats-button>
        </form>
      </section>

      <section class="login__hero" aria-hidden="true">
        <div class="login__hero-card">
          <span class="material-symbols-outlined">shield</span>
          <h2>Stable. Modern. Clear.</h2>
          <p>Institutional-grade operations for cooperative credit societies.</p>
        </div>
      </section>
    </main>
  `,
  styles: [
    `
      .login {
        display: grid;
        grid-template-columns: 1fr;
        min-height: 100vh;
      }

      @media (min-width: 1024px) {
        .login {
          grid-template-columns: 45% 55%;
        }
      }

      .login__panel {
        display: flex;
        flex-direction: column;
        justify-content: center;
        gap: 40px;
        padding: 32px;
        background: var(--pats-color-surface-container-lowest);
      }

      @media (min-width: 1024px) {
        .login__panel {
          padding: 96px;
        }
      }

      .login__brand {
        display: flex;
        gap: 16px;
        align-items: flex-start;
      }

      .login__brand-icon {
        width: 48px;
        height: 48px;
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary-container);
        display: grid;
        place-items: center;
        box-shadow: var(--pats-shadow-card);
      }

      .login__brand h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 32px;
        color: var(--pats-color-primary);
      }

      .login__brand p {
        margin: 4px 0 0;
        color: var(--pats-color-on-surface-variant);
      }

      .login__form {
        display: flex;
        flex-direction: column;
        gap: 20px;
        max-width: 420px;
        width: 100%;
      }

      .login__error {
        margin: 0;
        color: var(--pats-color-error);
        font-size: 14px;
      }

      .login__hero {
        display: none;
        position: relative;
        background: linear-gradient(135deg, #1a3c6e 0%, #002653 100%);
        overflow: hidden;
      }

      @media (min-width: 1024px) {
        .login__hero {
          display: grid;
          place-items: center;
        }
      }

      .login__hero-card {
        max-width: 420px;
        padding: 32px;
        border-radius: var(--pats-radius-lg);
        background: rgba(255, 255, 255, 0.08);
        color: white;
        backdrop-filter: blur(12px);
      }

      .login__hero-card h2 {
        margin: 16px 0 8px;
        font-family: var(--pats-font-display);
      }

      .login__hero-card p {
        margin: 0;
        opacity: 0.85;
      }
    `,
  ],
})
export class LoginPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  fieldError(controlName: 'email' | 'password'): string | null {
    const control = this.form.controls[controlName];
    if (!control.touched || control.valid) {
      return null;
    }

    if (controlName === 'email' && control.hasError('email')) {
      return 'Enter a valid email address.';
    }

    return 'This field is required.';
  }

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    try {
      const { email, password } = this.form.getRawValue();
      const response = await this.auth.login(email, password);

      if (response.requiresTwoFactor) {
        this.errorMessage.set('Two-factor authentication is required. Complete 2FA flow in Step 5 shell wiring.');
        return;
      }

      await this.router.navigateByUrl('/dashboard');
    } catch (error) {
      this.errorMessage.set(this.auth.extractProblemMessage(error, 'Invalid email or password.'));
    } finally {
      this.loading.set(false);
    }
  }
}
