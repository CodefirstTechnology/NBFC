import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  AdminApiService,
  RoleSummary,
  extractApiErrorMessage,
} from '@patsanstha/admin-data-access';

@Component({
  selector: 'pats-roles-page',
  standalone: true,
  imports: [RouterLink],
  template: `
    <section class="page">
      <a routerLink="/admin" class="page__back">
        <span class="material-symbols-outlined">arrow_back</span>
        Back to users
      </a>

      <header class="page__header">
        <div>
          <h1>Roles &amp; Permissions</h1>
          <p>Read-only view of system roles and available permissions.</p>
        </div>
      </header>

      @if (errorMessage()) {
        <p class="page__error">{{ errorMessage() }}</p>
      }

      @if (loading()) {
        <p>Loading roles…</p>
      } @else {
        <div class="page__grid">
          <article class="page__card">
            <h2>Roles</h2>
            @for (role of roles(); track role.id) {
              <div class="page__role">
                <h3>{{ role.name }}</h3>
                <ul class="page__permission-list">
                  @for (permission of role.permissions; track permission) {
                    <li>{{ permission }}</li>
                  }
                </ul>
              </div>
            }
          </article>

          <article class="page__card">
            <h2>All Permissions</h2>
            <ul class="page__permission-list">
              @for (permission of permissions(); track permission) {
                <li>{{ permission }}</li>
              }
            </ul>
          </article>
        </div>
      }
    </section>
  `,
  styles: [
    `
      .page { display: flex; flex-direction: column; gap: 24px; }
      .page__back { display: inline-flex; align-items: center; gap: 4px; color: var(--pats-color-primary-container); font-weight: 600; font-size: 14px; }
      .page__header h1 { margin: 0; font-family: var(--pats-font-display); font-size: 32px; color: var(--pats-color-primary); }
      .page__header p { margin: 8px 0 0; color: var(--pats-color-text-secondary); }
      .page__grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)); gap: 16px; }
      .page__card { padding: 24px; border-radius: var(--pats-radius-lg); background: var(--pats-color-surface-container-lowest); border: 1px solid var(--pats-color-border-subtle); }
      .page__card h2 { margin: 0 0 16px; font-family: var(--pats-font-display); color: var(--pats-color-primary-container); }
      .page__role { padding: 16px 0; border-bottom: 1px solid var(--pats-color-border-subtle); }
      .page__role:last-child { border-bottom: none; }
      .page__role h3 { margin: 0 0 8px; font-size: 16px; }
      .page__permission-list { margin: 0; padding-left: 20px; display: grid; gap: 4px; font-size: 14px; color: var(--pats-color-text-secondary); }
      .page__error { color: var(--pats-color-error); }
    `,
  ],
})
export class RolesPageComponent implements OnInit {
  private readonly adminApi = inject(AdminApiService);

  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly roles = signal<RoleSummary[]>([]);
  readonly permissions = signal<string[]>([]);

  ngOnInit(): void {
    void this.loadData();
  }

  private async loadData(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);
    try {
      const [roles, permissions] = await Promise.all([
        this.adminApi.getRoles(),
        this.adminApi.getPermissions(),
      ]);
      this.roles.set(roles);
      this.permissions.set(permissions);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to load roles and permissions.'));
    } finally {
      this.loading.set(false);
    }
  }
}
