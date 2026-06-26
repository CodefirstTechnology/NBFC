import { Component, inject, signal, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '@patsanstha/auth';
import {
  AdminApiService,
  RoleSummary,
  extractApiErrorMessage,
} from '@patsanstha/admin-data-access';
import { PatsButtonComponent, PatsFormFieldComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-user-create-page',
  standalone: true,
  imports: [FormsModule, RouterLink, PatsButtonComponent, PatsFormFieldComponent],
  template: `
    <section class="create-page">
      <header class="create-page__header">
        <a routerLink="/admin" class="create-page__back">
          <span class="material-symbols-outlined">arrow_back</span>
          Back to users
        </a>
        <h1>Create User</h1>
        <p>Add a new user with email credentials and role assignments.</p>
      </header>

      @if (errorMessage()) {
        <p class="create-page__error">{{ errorMessage() }}</p>
      }

      <article class="create-page__card">
        <div class="create-page__form">
          <pats-form-field label="Full Name">
            <input type="text" [(ngModel)]="fullName" />
          </pats-form-field>
          <pats-form-field label="Email">
            <input type="email" [(ngModel)]="email" />
          </pats-form-field>
          <pats-form-field label="Password">
            <input type="password" [(ngModel)]="password" />
          </pats-form-field>
          <pats-form-field label="Branch Id (optional)">
            <input type="text" [(ngModel)]="branchId" />
          </pats-form-field>
        </div>

        @if (roles().length > 0) {
          <div class="create-page__roles">
            <h2>Roles</h2>
            @for (role of roles(); track role.id) {
              <label class="create-page__role">
                <input type="checkbox" [checked]="selectedRoles().includes(role.name)" (change)="toggleRole(role.name, $event)" />
                <span>{{ role.name }}</span>
              </label>
            }
          </div>
        }
      </article>

      <div class="create-page__actions">
        <pats-button variant="ghost" (clicked)="cancel()">Cancel</pats-button>
        <pats-button [loading]="loading()" (clicked)="submit()">Create User</pats-button>
      </div>
    </section>
  `,
  styles: [
    `
      .create-page { display: flex; flex-direction: column; gap: 24px; max-width: 720px; }
      .create-page__back { display: inline-flex; align-items: center; gap: 4px; color: var(--pats-color-primary-container); font-weight: 600; font-size: 14px; }
      .create-page__header h1 { margin: 0; font-family: var(--pats-font-display); font-size: 32px; }
      .create-page__header p { margin: 8px 0 0; color: var(--pats-color-text-secondary); }
      .create-page__card { padding: 24px; border-radius: var(--pats-radius-lg); background: var(--pats-color-surface-container-lowest); border: 1px solid var(--pats-color-border-subtle); display: flex; flex-direction: column; gap: 24px; }
      .create-page__form { display: grid; gap: 16px; }
      .create-page__roles h2 { margin: 0 0 12px; font-family: var(--pats-font-display); font-size: 18px; color: var(--pats-color-primary-container); }
      .create-page__role { display: flex; align-items: center; gap: 8px; padding: 8px 0; }
      .create-page__actions { display: flex; justify-content: flex-end; gap: 12px; }
      .create-page__error { color: var(--pats-color-error); }
    `,
  ],
})
export class UserCreatePageComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly adminApi = inject(AdminApiService);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly roles = signal<RoleSummary[]>([]);
  readonly selectedRoles = signal<string[]>([]);

  fullName = '';
  email = '';
  password = '';
  branchId = '';

  ngOnInit(): void {
    void this.loadRoles();
    const userBranchId = this.auth.user()?.branchId;
    if (userBranchId) {
      this.branchId = userBranchId;
    }
  }

  toggleRole(roleName: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    const current = this.selectedRoles();
    this.selectedRoles.set(
      checked ? [...current, roleName] : current.filter((r) => r !== roleName)
    );
  }

  async submit(): Promise<void> {
    if (!this.fullName.trim() || !this.email.trim() || !this.password.trim()) {
      this.errorMessage.set('Fill in name, email, and password.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    try {
      await this.adminApi.createUser({
        email: this.email.trim(),
        password: this.password,
        fullName: this.fullName.trim(),
        branchId: this.branchId.trim() || null,
        roles: this.selectedRoles(),
      });
      void this.router.navigate(['/admin']);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to create user.'));
    } finally {
      this.loading.set(false);
    }
  }

  cancel(): void {
    void this.router.navigate(['/admin']);
  }

  private async loadRoles(): Promise<void> {
    try {
      const roles = await this.adminApi.getRoles();
      this.roles.set(roles);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to load roles.'));
    }
  }
}
