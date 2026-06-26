import { Component, inject, computed } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '@patsanstha/auth';
import { PatsButtonComponent } from '@patsanstha/ui-kit';

interface NavItem {
  label: string;
  route: string;
  icon: string;
  permission?: string;
}

@Component({
  selector: 'pats-shell-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, PatsButtonComponent],
  template: `
    <div class="shell">
      <aside class="shell__sidebar">
        <div class="shell__brand">
          <div class="shell__brand-icon">
            <span class="material-symbols-outlined">account_balance</span>
          </div>
          <div>
            <h1>Patsanstha</h1>
            <p>Credit Management</p>
          </div>
        </div>

        <nav class="shell__nav">
          @for (item of visibleNav(); track item.route) {
            <a
              [routerLink]="item.route"
              routerLinkActive="shell__nav-link--active"
              class="shell__nav-link">
              <span class="material-symbols-outlined">{{ item.icon }}</span>
              <span>{{ item.label }}</span>
            </a>
          }
        </nav>
      </aside>

      <div class="shell__main">
        <header class="shell__header">
          <div>
            <p class="shell__eyebrow">Cooperative Credit Society</p>
            <h2>{{ user()?.fullName ?? 'Staff User' }}</h2>
          </div>
          <div class="shell__header-actions">
            <span class="shell__role">{{ primaryRole() }}</span>
            <pats-button variant="ghost" icon="logout" (clicked)="logout()">Sign out</pats-button>
          </div>
        </header>

        <main class="shell__content">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
  styles: [
    `
      .shell {
        display: grid;
        grid-template-columns: var(--pats-sidebar-width) 1fr;
        min-height: 100vh;
        background: var(--pats-color-background);
      }

      .shell__sidebar {
        display: flex;
        flex-direction: column;
        gap: 16px;
        padding: 24px 16px;
        background: var(--pats-color-surface);
        border-right: 1px solid var(--pats-color-border-subtle);
      }

      .shell__brand {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 0 8px 16px;
      }

      .shell__brand-icon {
        width: 40px;
        height: 40px;
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary);
        display: grid;
        place-items: center;
      }

      .shell__brand h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 20px;
        font-weight: 700;
        color: var(--pats-color-primary-container);
      }

      .shell__brand p {
        margin: 0;
        font-size: 11px;
        color: var(--pats-color-on-surface-variant);
      }

      .shell__nav {
        display: flex;
        flex-direction: column;
        gap: 4px;
      }

      .shell__nav-link {
        display: flex;
        align-items: center;
        gap: 12px;
        min-height: var(--pats-touch-target);
        padding: 0 16px;
        border-radius: var(--pats-radius-md);
        color: var(--pats-color-on-surface-variant);
        font-size: 13px;
        font-weight: 600;
      }

      .shell__nav-link:hover {
        background: rgba(26, 60, 110, 0.08);
      }

      .shell__nav-link--active {
        color: var(--pats-color-primary-container);
        background: rgba(26, 60, 110, 0.08);
        border-right: 4px solid var(--pats-color-primary-container);
      }

      .shell__main {
        display: flex;
        flex-direction: column;
        min-width: 0;
      }

      .shell__header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 16px;
        padding: 24px 32px;
        background: var(--pats-color-surface-container-lowest);
        border-bottom: 1px solid var(--pats-color-border-subtle);
      }

      .shell__eyebrow {
        margin: 0 0 4px;
        font-size: 11px;
        font-weight: 600;
        color: var(--pats-color-text-secondary);
        text-transform: uppercase;
        letter-spacing: 0.04em;
      }

      .shell__header h2 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 24px;
        font-weight: 600;
      }

      .shell__header-actions {
        display: flex;
        align-items: center;
        gap: 12px;
      }

      .shell__role {
        padding: 6px 12px;
        border-radius: var(--pats-radius-full);
        background: rgba(26, 60, 110, 0.08);
        color: var(--pats-color-primary-container);
        font-size: 11px;
        font-weight: 600;
        text-transform: uppercase;
      }

      .shell__content {
        flex: 1;
        padding: 32px;
        max-width: var(--pats-max-content-width);
        width: 100%;
      }
    `,
  ],
})
export class ShellLayoutComponent {
  private readonly auth = inject(AuthService);

  readonly user = this.auth.user;

  readonly navItems: NavItem[] = [
    { label: 'Dashboard', route: '/dashboard', icon: 'dashboard' },
    { label: 'Members', route: '/members', icon: 'group', permission: 'members.read' },
    { label: 'Deposits', route: '/deposits', icon: 'account_balance_wallet', permission: 'deposits.read' },
    { label: 'Loans', route: '/loans', icon: 'payments', permission: 'loans.read' },
    { label: 'Collections', route: '/collections', icon: 'request_quote', permission: 'collections.read' },
    { label: 'Recovery', route: '/recovery', icon: 'gavel', permission: 'recovery.read' },
    { label: 'Accounting', route: '/accounting', icon: 'account_balance', permission: 'accounting.read' },
    { label: 'Reports', route: '/reports', icon: 'assessment', permission: 'reports.read' },
    { label: 'Admin', route: '/admin', icon: 'admin_panel_settings', permission: 'admin.users.manage' },
  ];

  readonly visibleNav = computed(() =>
    this.navItems.filter(
      (item) => !item.permission || this.auth.hasPermission(item.permission)
    )
  );

  readonly primaryRole = computed(() => this.user()?.roles[0] ?? 'Staff');

  async logout(): Promise<void> {
    await this.auth.logout();
    window.location.assign('/login');
  }
}
