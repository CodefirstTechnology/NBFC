import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AuthService } from '@patsanstha/auth';
import { PatsButtonComponent } from '@patsanstha/ui-kit';

interface RoleOption {
  label: string;
  icon: string;
}

@Component({
  selector: 'pats-login-page',
  standalone: true,
  imports: [ReactiveFormsModule, PatsButtonComponent],
  template: `
    <main class="login">
      <section class="login__panel">
        <button type="button" class="login__lang" aria-label="Switch language">
          <span class="material-symbols-outlined">translate</span>
          EN / मराठी
        </button>

        <div class="login__scale">
          <div class="login__content">
          <header class="login__brand">
            <div class="login__brand-icon">
              <span class="material-symbols-outlined">account_balance</span>
            </div>
            <div>
              <h1>Patsanstha Login</h1>
              <p>Access your cooperative banking dashboard.</p>
            </div>
          </header>

          <form class="login__form" [formGroup]="form" (ngSubmit)="submit()" autocomplete="off">
            <label class="login__field">
              <span class="login__label">Username / वापरकर्ता नाव</span>
              <span class="login__input-wrap">
                <span class="material-symbols-outlined login__input-icon">person</span>
                <input
                  class="login__input"
                  type="text"
                  name="pats-login-username"
                  autocomplete="off"
                  autocapitalize="off"
                  spellcheck="false"
                  placeholder="Enter username"
                  formControlName="email" />
              </span>
              @if (fieldError('email')) {
                <span class="login__field-error">{{ fieldError('email') }}</span>
              }
            </label>

            <label class="login__field">
              <span class="login__label-row">
                <span class="login__label">Password / संकेतशब्द</span>
                <a class="login__forgot" href="#" (click)="$event.preventDefault()">Forgot Password?</a>
              </span>
              <span class="login__input-wrap">
                <span class="material-symbols-outlined login__input-icon">lock</span>
                <input
                  class="login__input login__input--password"
                  [type]="showPassword() ? 'text' : 'password'"
                  name="pats-login-password"
                  autocomplete="new-password"
                  placeholder="Enter password"
                  formControlName="password" />
                <button
                  type="button"
                  class="login__toggle-password"
                  (click)="showPassword.set(!showPassword())"
                  [attr.aria-label]="showPassword() ? 'Hide password' : 'Show password'">
                  <span class="material-symbols-outlined">
                    {{ showPassword() ? 'visibility_off' : 'visibility' }}
                  </span>
                </button>
              </span>
              @if (fieldError('password')) {
                <span class="login__field-error">{{ fieldError('password') }}</span>
              }
            </label>

            @if (errorMessage()) {
              <p class="login__error">{{ errorMessage() }}</p>
            }

            <pats-button class="login__submit" type="submit" [loading]="loading()" [disabled]="form.invalid">
              Login / प्रवेश करा
            </pats-button>
          </form>

          <section class="login__roles" aria-label="Multi-role access">
            <h2>Multi-Role Access</h2>
            <div class="login__roles-grid">
              @for (role of roles; track role.label) {
                <button
                  type="button"
                  class="login__role"
                  [class.login__role--active]="selectedRole() === role.label"
                  (click)="selectRole(role)">
                  <span class="login__role-icon">
                    <span class="material-symbols-outlined">{{ role.icon }}</span>
                  </span>
                  <span>{{ role.label }}</span>
                </button>
              }
            </div>
          </section>

          <footer class="login__footer">
            © {{ currentYear }} Sahakari Bank. Secure Banking Environment.
          </footer>
          </div>
        </div>
      </section>

      <section class="login__hero" aria-hidden="true">
        <div class="login__hero-glow login__hero-glow--primary"></div>
        <div class="login__hero-glow login__hero-glow--secondary"></div>

        <div class="login__hero-card">
          <img
            class="login__hero-image"
            src="/login-hero.png"
            alt="Cooperative banking ecosystem illustration" />
        </div>
      </section>
    </main>
  `,
  styles: [
    `
      :host {
        display: block;
        height: 100dvh;
        max-height: 100dvh;
        overflow: hidden;
      }

      .login {
        position: fixed;
        inset: 0;
        display: grid;
        grid-template-columns: 1fr;
        height: 100dvh;
        max-height: 100dvh;
        overflow: hidden;
        background: var(--pats-color-background);
      }

      @media (min-width: 1024px) {
        .login {
          grid-template-columns: 45% 55%;
        }
      }

      .login__panel {
        position: relative;
        display: flex;
        align-items: center;
        justify-content: center;
        height: 100dvh;
        max-height: 100dvh;
        padding: 32px 32px 24px;
        background: var(--pats-color-surface-container-lowest);
        box-sizing: border-box;
        overflow: hidden;
      }

      @media (min-width: 1024px) {
        .login__panel {
          padding: 32px 96px;
        }
      }

      .login__lang {
        position: absolute;
        top: 32px;
        right: 32px;
        display: inline-flex;
        align-items: center;
        gap: 4px;
        border: none;
        background: transparent;
        color: var(--pats-color-primary);
        font-size: 13px;
        font-weight: 600;
        cursor: pointer;
        z-index: 1;
      }

      .login__lang .material-symbols-outlined {
        font-size: 18px;
      }

      .login__scale {
        width: 100%;
        max-width: 448px;
      }

      @media (max-height: 860px) {
        .login__scale {
          zoom: 0.94;
        }
      }

      @media (max-height: 800px) {
        .login__scale {
          zoom: 0.88;
        }
      }

      @media (max-height: 740px) {
        .login__scale {
          zoom: 0.82;
        }
      }

      @media (max-height: 680px) {
        .login__scale {
          zoom: 0.76;
        }
      }

      .login__content {
        width: 100%;
        display: flex;
        flex-direction: column;
        gap: 40px;
      }

      .login__brand {
        display: flex;
        flex-direction: column;
        align-items: flex-start;
        gap: 16px;
      }

      .login__brand-icon {
        width: 48px;
        height: 48px;
        flex-shrink: 0;
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary-container);
        display: grid;
        place-items: center;
        box-shadow: var(--pats-shadow-card);
      }

      .login__brand-icon .material-symbols-outlined {
        font-size: 28px;
      }

      .login__brand h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 32px;
        color: var(--pats-color-primary);
        letter-spacing: -0.02em;
        line-height: 1.2;
      }

      .login__brand p {
        margin: 4px 0 0;
        color: var(--pats-color-on-surface-variant);
        font-size: 16px;
        line-height: 1.5;
      }

      .login__form {
        display: flex;
        flex-direction: column;
        gap: 24px;
      }

      .login__field {
        display: flex;
        flex-direction: column;
        gap: 8px;
      }

      .login__label,
      .login__label-row {
        font-size: 13px;
        font-weight: 600;
        color: var(--pats-color-on-surface-variant);
      }

      .login__label-row {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 8px;
      }

      .login__forgot {
        color: var(--pats-color-primary);
        text-decoration: none;
        font-size: 13px;
        font-weight: 600;
        white-space: nowrap;
      }

      .login__forgot:hover {
        text-decoration: underline;
      }

      .login__input-wrap {
        position: relative;
        display: block;
      }

      .login__input-icon {
        position: absolute;
        left: 10px;
        top: 50%;
        transform: translateY(-50%);
        color: var(--pats-color-outline);
        font-size: 18px;
        pointer-events: none;
      }

      .login__input {
        width: 100%;
        height: 44px;
        min-height: 44px;
        padding: 0 14px 0 42px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-surface-muted);
        color: var(--pats-color-on-surface);
        font-size: 14px;
        outline: none;
        box-sizing: border-box;
        transition: border-color 0.15s ease, box-shadow 0.15s ease;
      }

      .login__input:focus {
        border-color: var(--pats-color-primary-container);
        box-shadow: 0 0 0 3px rgba(26, 60, 110, 0.12);
      }

      .login__input--password {
        padding-right: 38px;
      }

      .login__toggle-password {
        position: absolute;
        right: 6px;
        top: 50%;
        transform: translateY(-50%);
        border: none;
        background: transparent;
        color: var(--pats-color-outline);
        cursor: pointer;
        display: grid;
        place-items: center;
        padding: 4px;
      }

      .login__toggle-password .material-symbols-outlined {
        font-size: 18px;
      }

      .login__field-error,
      .login__error {
        margin: 0;
        color: var(--pats-color-error);
        font-size: 12px;
      }

      .login__submit {
        display: block;
        width: 100%;
      }

      .login__submit ::ng-deep .pats-btn {
        width: 100%;
        min-height: 52px;
        padding: 0 20px;
        font-family: var(--pats-font-display);
        font-size: 20px;
        font-weight: 700;
        background: var(--pats-color-primary);
        color: var(--pats-color-on-primary);
        box-shadow: 0 10px 24px rgba(26, 60, 110, 0.2);
      }

      .login__submit ::ng-deep .pats-btn:not(:disabled):active {
        transform: scale(0.98);
      }

      .login__roles {
        padding-top: 32px;
        border-top: 1px solid var(--pats-color-border-subtle);
      }

      .login__roles h2 {
        margin: 0 0 24px;
        font-size: 13px;
        font-weight: 600;
        letter-spacing: 0.08em;
        text-transform: uppercase;
        color: var(--pats-color-outline);
      }

      .login__roles-grid {
        display: grid;
        grid-template-columns: repeat(5, minmax(0, 1fr));
        gap: 8px;
      }

      .login__role {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 8px;
        border: none;
        background: transparent;
        cursor: pointer;
        padding: 0;
        font-size: 11px;
        font-weight: 500;
        color: var(--pats-color-on-surface-variant);
        transition: color 0.15s ease;
      }

      .login__role-icon {
        width: 48px;
        height: 48px;
        border-radius: 12px;
        background: var(--pats-color-surface-container-low);
        color: var(--pats-color-on-surface-variant);
        display: grid;
        place-items: center;
        transition: background 0.15s ease, color 0.15s ease;
      }

      .login__role-icon .material-symbols-outlined {
        font-size: 22px;
      }

      .login__role:hover .login__role-icon,
      .login__role--active .login__role-icon {
        background: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary-container);
      }

      .login__role:hover,
      .login__role--active {
        color: var(--pats-color-primary);
      }

      .login__footer {
        text-align: center;
        font-size: 14px;
        color: var(--pats-color-outline);
        line-height: 1.5;
        padding-top: 16px;
      }

      .login__hero {
        display: none;
        position: relative;
        height: 100dvh;
        max-height: 100dvh;
        overflow: hidden;
        background: var(--pats-color-surface);
        box-sizing: border-box;
        align-items: center;
        justify-content: center;
      }

      @media (min-width: 1024px) {
        .login__hero {
          display: flex;
        }
      }

      .login__hero-glow {
        position: absolute;
        width: 60%;
        height: 60%;
        border-radius: 50%;
        filter: blur(120px);
        opacity: 0.4;
        pointer-events: none;
      }

      .login__hero-glow--primary {
        top: -10%;
        left: -10%;
        background: var(--pats-color-primary-fixed-dim, #abc7ff);
      }

      .login__hero-glow--secondary {
        bottom: -10%;
        right: -10%;
        background: var(--pats-color-secondary-fixed-dim, #71dc92);
      }

      .login__hero-card {
        position: relative;
        z-index: 1;
        width: min(88%, 520px);
        max-height: calc(100dvh - 48px);
        padding: 20px 28px 24px;
        border-radius: 20px;
        background: #ffffff;
        box-shadow: 0 24px 48px rgba(15, 23, 42, 0.12);
        display: flex;
        align-items: center;
        justify-content: center;
        box-sizing: border-box;
      }

      .login__hero-image {
        display: block;
        width: 100%;
        height: auto;
        max-height: calc(100dvh - 120px);
        object-fit: contain;
      }
    `,
  ],
})
export class LoginPageComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly currentYear = new Date().getFullYear();
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showPassword = signal(false);
  readonly selectedRole = signal<string | null>(null);

  readonly roles: RoleOption[] = [
    { label: 'Admin', icon: 'admin_panel_settings' },
    { label: 'Manager', icon: 'account_tree' },
    { label: 'Teller', icon: 'payments' },
    { label: 'Loan Officer', icon: 'assignment_ind' },
    { label: 'Member', icon: 'person_outline' },
  ];

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  ngOnInit(): void {
    document.documentElement.classList.add('login-route');
    document.body.classList.add('login-route');
  }

  ngOnDestroy(): void {
    document.documentElement.classList.remove('login-route');
    document.body.classList.remove('login-route');
  }

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

  selectRole(role: RoleOption): void {
    this.selectedRole.set(role.label);
    this.errorMessage.set(null);
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
        this.errorMessage.set('Two-factor authentication is required.');
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
