import { Component, inject, computed } from '@angular/core';
import { AuthService } from '@patsanstha/auth';
import { PatsStatusPillComponent, PatsTableComponent } from '@patsanstha/ui-kit';

interface ModuleRow {
  module: string;
  status: string;
  variant: 'active' | 'pending' | 'inactive';
}

@Component({
  selector: 'pats-dashboard-page',
  standalone: true,
  imports: [PatsStatusPillComponent, PatsTableComponent],
  template: `
    <section class="dashboard">
      <header class="dashboard__header">
        <div>
          <h1>Executive Dashboard</h1>
          <p>Welcome back, {{ user()?.fullName }}. Branch operations overview.</p>
        </div>
        <pats-status-pill label="System Online" variant="active" />
      </header>

      <div class="dashboard__cards">
        <article class="dashboard__card dashboard__card--primary">
          <span class="material-symbols-outlined">group</span>
          <div>
            <p>Members Module</p>
            @if (auth.hasPermission('members.read')) {
              <h3>Live — list, create &amp; detail</h3>
            } @else {
              <h3>Restricted</h3>
            }
          </div>
        </article>

        <article class="dashboard__card">
          <span class="material-symbols-outlined">account_balance_wallet</span>
          <div>
            <p>Deposits</p>
            @if (auth.hasPermission('deposits.read')) {
              <h3>Live — list, create &amp; detail</h3>
            } @else {
              <h3>Restricted</h3>
            }
          </div>
        </article>

        <article class="dashboard__card">
          <span class="material-symbols-outlined">payments</span>
          <div>
            <p>Loans</p>
            @if (auth.hasPermission('loans.read')) {
              <h3>Live — apply, approve &amp; disburse</h3>
            } @else {
              <h3>Restricted</h3>
            }
          </div>
        </article>
      </div>

      <pats-table
        [columns]="moduleColumns"
        [rows]="moduleRows()"
        emptyMessage="No modules configured." />
    </section>
  `,
  styles: [
    `
      .dashboard {
        display: flex;
        flex-direction: column;
        gap: 24px;
      }

      .dashboard__header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 16px;
      }

      .dashboard__header h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 32px;
      }

      .dashboard__header p {
        margin: 8px 0 0;
        color: var(--pats-color-text-secondary);
      }

      .dashboard__cards {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
        gap: 16px;
      }

      .dashboard__card {
        display: flex;
        gap: 16px;
        align-items: flex-start;
        padding: 24px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
        box-shadow: var(--pats-shadow-card);
      }

      .dashboard__card--primary {
        border-top: 4px solid var(--pats-color-primary-container);
      }

      .dashboard__card p {
        margin: 0;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
      }

      .dashboard__card h3 {
        margin: 8px 0 0;
        font-family: var(--pats-font-display);
      }
    `,
  ],
})
export class DashboardPageComponent {
  protected readonly auth = inject(AuthService);

  readonly user = this.auth.user;

  readonly moduleColumns = [
    { key: 'module', header: 'Module' },
    { key: 'status', header: 'Build Status' },
  ];

  readonly moduleRows = computed<ModuleRow[]>(() => [
    { module: 'Identity + Auth', status: 'Complete', variant: 'active' },
    { module: 'Members / Deposits / Loans', status: 'Complete', variant: 'active' },
    { module: 'Collections / Recovery', status: 'Complete', variant: 'active' },
    { module: 'Accounting / Reporting', status: 'Complete', variant: 'active' },
    { module: 'mfe-admin', status: 'Complete', variant: 'active' },
    { module: 'Step 7 — BuildingBlocks', status: 'Complete', variant: 'active' },
    { module: 'Step 8 — Gateway + Jobs', status: 'Complete', variant: 'active' },
  ]);
}
