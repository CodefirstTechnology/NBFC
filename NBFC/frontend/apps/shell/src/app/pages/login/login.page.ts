import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AuthService } from '@patsanstha/auth';
import { PatsButtonComponent } from '@patsanstha/ui-kit';

interface RoleOption {
  label: string;
  icon: string;
  email: string;
  password: string;
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

          <form class="login__form" [formGroup]="form" (ngSubmit)="submit()">
            <label class="login__field">
              <span class="login__label">Username / वापरकर्ता नाव</span>
              <span class="login__input-wrap">
                <span class="material-symbols-outlined login__input-icon">person</span>
                <input
                  class="login__input"
                  type="email"
                  autocomplete="email"
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
                  autocomplete="current-password"
                  placeholder="••••••••"
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

            <pats-button type="submit" [loading]="loading()" [disabled]="form.invalid">
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
      </section>

      <section class="login__hero" aria-hidden="true">
        <div class="login__hero-glow login__hero-glow--primary"></div>
        <div class="login__hero-glow login__hero-glow--secondary"></div>

        <div class="login__hero-inner">
          <img
            class="login__hero-image"
            src="/login-hero.png"
            alt="Cooperative banking ecosystem illustration" />
          <div class="login__hero-brand">
            <p class="login__hero-title">COOPERATIVE BANKING</p>
            <p class="login__hero-subtitle">FINTECH LOGIN</p>
          </div>
        </div>

        <div class="login__badges">
          <article class="login__badge">
            <span class="material-symbols-outlined login__badge-icon">verified_user</span>
            <span>Trusted Security</span>
          </article>
          <article class="login__badge">
            <span class="material-symbols-outlined login__badge-icon">groups</span>
            <span>Community Focus</span>
          </article>
          <article class="login__badge">
            <span class="material-symbols-outlined login__badge-icon">speed</span>
            <span>Swift Access</span>
          </article>
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
        padding: 32px 24px;
        background: var(--pats-color-surface-container-lowest);
      }

      @media (min-width: 1024px) {
        .login__panel {
          padding: 48px 96px;
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
        color: var(--pats-color-primary-container);
        font-size: 13px;
        font-weight: 600;
        cursor: pointer;
      }

      .login__lang .material-symbols-outlined {
        font-size: 18px;
      }

      .login__content {
        width: 100%;
        max-width: 420px;
        display: flex;
        flex-direction: column;
        gap: 40px;
      }

      .login__brand {
        display: flex;
        flex-direction: column;
        gap: 16px;
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
        letter-spacing: -0.01em;
      }

      .login__brand p {
        margin: 4px 0 0;
        color: var(--pats-color-on-surface-variant);
        font-size: 16px;
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
        gap: 12px;
      }

      .login__forgot {
        color: var(--pats-color-primary-container);
        text-decoration: none;
        font-size: 13px;
        font-weight: 600;
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
        left: 12px;
        top: 50%;
        transform: translateY(-50%);
        color: var(--pats-color-outline);
        font-size: 20px;
        pointer-events: none;
      }

      .login__input {
        width: 100%;
        min-height: 48px;
        padding: 0 16px 0 44px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-surface-muted);
        color: var(--pats-color-on-surface);
        font-size: 14px;
        outline: none;
        transition: border-color 0.15s ease, box-shadow 0.15s ease;
      }

      .login__input:focus {
        border-color: var(--pats-color-primary-container);
        box-shadow: 0 0 0 3px rgba(26, 60, 110, 0.12);
      }

      .login__input--password {
        padding-right: 44px;
      }

      .login__toggle-password {
        position: absolute;
        right: 8px;
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

      .login__field-error,
      .login__error {
        margin: 0;
        color: var(--pats-color-error);
        font-size: 13px;
      }

      .login__roles {
        padding-top: 32px;
        border-top: 1px solid var(--pats-color-border-subtle);
      }

      .login__roles h2 {
        margin: 0 0 24px;
        font-size: 11px;
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

      .login__role:hover .login__role-icon,
      .login__role--active .login__role-icon {
        background: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary-container);
      }

      .login__role:hover,
      .login__role--active {
        color: var(--pats-color-primary-container);
      }

      .login__footer {
        text-align: center;
        font-size: 14px;
        color: var(--pats-color-outline);
      }

      .login__hero {
        display: none;
        position: relative;
        overflow: hidden;
        background: var(--pats-color-surface);
        align-items: center;
        justify-content: center;
      }

      @media (min-width: 1024px) {
        .login__hero {
          display: flex;
          flex-direction: column;
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

      .login__hero-inner {
        position: relative;
        z-index: 1;
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        flex: 1;
        width: 100%;
        padding: 48px 64px 160px;
      }

      .login__hero-image {
        width: 100%;
        max-width: 560px;
        object-fit: contain;
        filter: drop-shadow(0 24px 48px rgba(0, 38, 83, 0.12));
        animation: login-drift 20s infinite alternate ease-in-out;
      }

      .login__hero-brand {
        margin-top: 24px;
        text-align: center;
      }

      .login__hero-title {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: clamp(28px, 3vw, 42px);
        font-weight: 800;
        letter-spacing: 0.04em;
        color: var(--pats-color-primary);
      }

      .login__hero-subtitle {
        margin: 8px 0 0;
        font-size: 14px;
        font-weight: 600;
        letter-spacing: 0.2em;
        color: var(--pats-color-primary-container);
      }

      .login__badges {
        position: absolute;
        bottom: 48px;
        left: 50%;
        transform: translateX(-50%);
        z-index: 2;
        width: calc(100% - 64px);
        max-width: 640px;
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 16px;
      }

      .login__badge {
        display: flex;
        align-items: center;
        gap: 10px;
        padding: 16px;
        border-radius: var(--pats-radius-lg);
        background: rgba(244, 243, 250, 0.85);
        backdrop-filter: blur(8px);
        border: 1px solid var(--pats-color-border-subtle);
        box-shadow: var(--pats-shadow-card);
        font-size: 13px;
        font-weight: 700;
        color: var(--pats-color-on-surface);
      }

      .login__badge-icon {
        color: var(--pats-color-secondary);
        font-variation-settings: 'FILL' 1;
      }

      @keyframes login-drift {
        0% {
          transform: translateY(0) scale(1);
        }
        100% {
          transform: translateY(-16px) scale(1.02);
        }
      }
    `,
  ],
})
export class LoginPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly currentYear = new Date().getFullYear();
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showPassword = signal(false);
  readonly selectedRole = signal<string | null>('Admin');

  readonly roles: RoleOption[] = [
    {
      label: 'Admin',
      icon: 'admin_panel_settings',
      email: 'admin@patsanstha.local',
      password: 'ChangeMe@123',
    },
    {
      label: 'Manager',
      icon: 'account_tree',
      email: 'admin@patsanstha.local',
      password: 'ChangeMe@123',
    },
    {
      label: 'Teller',
      icon: 'payments',
      email: 'admin@patsanstha.local',
      password: 'ChangeMe@123',
    },
    {
      label: 'Loan Officer',
      icon: 'assignment_ind',
      email: 'admin@patsanstha.local',
      password: 'ChangeMe@123',
    },
    {
      label: 'Member',
      icon: 'person_outline',
      email: 'admin@patsanstha.local',
      password: 'ChangeMe@123',
    },
  ];

  readonly form = this.fb.nonNullable.group({
    email: ['admin@patsanstha.local', [Validators.required, Validators.email]],
    password: ['ChangeMe@123', [Validators.required, Validators.minLength(8)]],
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

  selectRole(role: RoleOption): void {
    this.selectedRole.set(role.label);
    this.form.patchValue({ email: role.email, password: role.password });
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
