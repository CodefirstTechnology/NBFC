import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '@patsanstha/auth';
import {
  ExecutiveDashboard,
  ReportsApiService,
  activityStatusVariant,
  extractApiErrorMessage,
  formatCompactInr,
  formatCount,
  formatRelativeTime,
  formatTrendPercent,
  memberInitials,
} from '@patsanstha/reports-data-access';
import { PatsStatusPillComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-dashboard-page',
  standalone: true,
  imports: [RouterLink, PatsStatusPillComponent],
  template: `
    <section class="dashboard">
      @if (loading()) {
        <p class="dashboard__loading">Loading dashboard…</p>
      } @else if (errorMessage()) {
        <p class="dashboard__error">{{ errorMessage() }}</p>
      } @else {
        @if (data(); as dashboard) {
        <header class="dashboard__header">
          <div>
            <div class="dashboard__title-row">
              <h1>Executive Dashboard</h1>
              <span class="dashboard__branch">
                <span class="material-symbols-outlined">location_on</span>
                Branch: {{ dashboard.branchName }}
              </span>
            </div>
            <p>Welcome back, {{ user()?.fullName }}. Here is today's society overview.</p>
          </div>
        </header>

        <div class="dashboard__kpis">
          <article class="dashboard__kpi">
            <div class="dashboard__kpi-top">
              <span>Total Members</span>
              <span class="material-symbols-outlined">group</span>
            </div>
            <strong>{{ formatCount(dashboard.kpis.totalMembers) }}</strong>
            @if (formatTrendPercent(dashboard.kpis.membersTrendPercent); as trend) {
              <p class="dashboard__trend dashboard__trend--up">
                <span class="material-symbols-outlined">trending_up</span>{{ trend }} this month
              </p>
            }
            @if (dashboard.kpis.newMembersThisWeek > 0) {
              <p class="dashboard__kpi-sub">{{ dashboard.kpis.newMembersThisWeek }} new this week</p>
            }
          </article>

          <article class="dashboard__kpi">
            <div class="dashboard__kpi-top">
              <span>Active Loans</span>
              <span class="material-symbols-outlined">account_balance_wallet</span>
            </div>
            <strong>{{ formatCompactInr(dashboard.kpis.activeLoansAmount) }}</strong>
            @if (formatTrendPercent(dashboard.kpis.activeLoansTrendPercent); as trend) {
              <p class="dashboard__trend dashboard__trend--up">
                <span class="material-symbols-outlined">trending_up</span>{{ trend }}
              </p>
            }
            <p class="dashboard__kpi-sub">Distributed across {{ formatCount(dashboard.kpis.activeLoansCount) }} accounts</p>
          </article>

          <article class="dashboard__kpi">
            <div class="dashboard__kpi-top">
              <span>Total Deposits</span>
              <span class="material-symbols-outlined">savings</span>
            </div>
            <strong>{{ formatCompactInr(dashboard.kpis.totalDepositsAmount) }}</strong>
            @if (formatTrendPercent(dashboard.kpis.depositsTrendPercent); as trend) {
              <p class="dashboard__trend dashboard__trend--up">
                <span class="material-symbols-outlined">trending_up</span>{{ trend }} Growth
              </p>
            }
          </article>

          <article class="dashboard__kpi">
            <div class="dashboard__kpi-top">
              <span>Recovery Rate</span>
              <span class="material-symbols-outlined">sync_saved_locally</span>
            </div>
            <strong>{{ dashboard.kpis.recoveryRatePercent.toFixed(1) }}%</strong>
            <p
              class="dashboard__kpi-sub"
              [class.dashboard__kpi-sub--warn]="dashboard.kpis.recoveryRatePercent < dashboard.kpis.recoveryTargetPercent">
              Target: {{ dashboard.kpis.recoveryTargetPercent.toFixed(0) }}%
            </p>
          </article>
        </div>

        <div class="dashboard__middle">
          <article class="dashboard__chart-card">
            <div class="dashboard__chart-header">
              <div>
                <h2>Collection vs Recovery</h2>
                <p>Monthly performance (last 6 months)</p>
              </div>
            </div>
            <div class="dashboard__chart-legend">
              <span><i class="dashboard__legend-dot dashboard__legend-dot--target"></i> Target Collection</span>
              <span><i class="dashboard__legend-dot dashboard__legend-dot--actual"></i> Actual Recovery</span>
            </div>
            @if (dashboard.monthlyPerformance.length === 0) {
              <p class="dashboard__empty-chart">No collection data yet.</p>
            } @else {
              <div class="dashboard__chart">
                @for (point of dashboard.monthlyPerformance; track point.month + '-' + point.year) {
                  <div class="dashboard__chart-group">
                    <div class="dashboard__bars">
                      <div
                        class="dashboard__bar dashboard__bar--target"
                        [style.height.%]="barHeight(point.targetCollection)"
                        [title]="formatCompactInr(point.targetCollection)"></div>
                      <div
                        class="dashboard__bar dashboard__bar--actual"
                        [style.height.%]="barHeight(point.actualRecovery)"
                        [title]="formatCompactInr(point.actualRecovery)"></div>
                    </div>
                    <span class="dashboard__chart-label">{{ point.monthLabel }}</span>
                  </div>
                }
              </div>
            }
          </article>

          <aside class="dashboard__actions-card">
            <h2>Quick Actions</h2>
            <nav class="dashboard__actions">
              @if (auth.hasPermission('members.create')) {
                <a routerLink="/members/new" class="dashboard__action">
                  <span class="material-symbols-outlined">person_add</span>
                  <div>
                    <strong>New Member Enrollment</strong>
                    <span>Add a new shareholder</span>
                  </div>
                </a>
              }
              @if (auth.hasPermission('deposits.create')) {
                <a routerLink="/deposits/new" class="dashboard__action">
                  <span class="material-symbols-outlined">account_balance_wallet</span>
                  <div>
                    <strong>New Deposit / FD</strong>
                    <span>Create fixed or recurring deposit</span>
                  </div>
                </a>
              }
              @if (auth.hasPermission('loans.create')) {
                <a routerLink="/loans/new" class="dashboard__action">
                  <span class="material-symbols-outlined">payments</span>
                  <div>
                    <strong>Disburse Loan</strong>
                    <span>Process loan application</span>
                  </div>
                </a>
              }
            </nav>
            <footer class="dashboard__system">
              <span class="dashboard__system-dot"></span>
              <div>
                <strong>System Status</strong>
                <span>{{ dashboard.systemStatus.message }}</span>
              </div>
            </footer>
          </aside>
        </div>

        <article class="dashboard__activity">
          <div class="dashboard__activity-header">
            <h2>Recent Activity</h2>
            <a routerLink="/collections">View All Records</a>
          </div>
          @if (dashboard.recentActivity.length === 0) {
            <p class="dashboard__empty">No recent activity yet.</p>
          } @else {
            <table class="dashboard__table">
              <thead>
                <tr>
                  <th>Member / Transaction</th>
                  <th>Type</th>
                  <th>Amount</th>
                  <th>Status</th>
                  <th>Time</th>
                </tr>
              </thead>
              <tbody>
                @for (item of dashboard.recentActivity; track item.id) {
                  <tr>
                    <td>
                      <div class="dashboard__member-cell">
                        <span class="dashboard__avatar">{{ memberInitials(item.memberName) }}</span>
                        <div>
                          <strong>{{ item.memberName }}</strong>
                          <span>#{{ item.referenceNumber }}</span>
                        </div>
                      </div>
                    </td>
                    <td>{{ item.activityType }}</td>
                    <td>{{ item.amount != null ? formatCompactInr(item.amount) : '—' }}</td>
                    <td>
                      <pats-status-pill
                        [label]="item.status"
                        [variant]="activityStatusVariant(item.status)" />
                    </td>
                    <td>{{ formatRelativeTime(item.occurredAt) }}</td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </article>

        <footer class="dashboard__footer">
          <span>© {{ currentYear }} Patsanstha Credit Management System</span>
          <span>Last updated {{ formatRelativeTime(dashboard.generatedAt) }}</span>
        </footer>
        }
      }
    </section>
  `,
  styles: [
    `
      .dashboard { display: flex; flex-direction: column; gap: 24px; }
      .dashboard__loading, .dashboard__error, .dashboard__empty, .dashboard__empty-chart {
        color: var(--pats-color-text-secondary);
      }
      .dashboard__error { color: var(--pats-color-error); }
      .dashboard__header h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 32px;
        color: var(--pats-color-primary-container);
      }
      .dashboard__header p { margin: 8px 0 0; color: var(--pats-color-text-secondary); }
      .dashboard__title-row {
        display: flex;
        flex-wrap: wrap;
        align-items: center;
        gap: 16px;
      }
      .dashboard__branch {
        display: inline-flex;
        align-items: center;
        gap: 4px;
        padding: 6px 12px;
        border-radius: 999px;
        border: 1px solid var(--pats-color-border-subtle);
        background: var(--pats-color-surface-container-low);
        font-size: 13px;
        font-weight: 600;
        color: var(--pats-color-text-secondary);
      }
      .dashboard__branch .material-symbols-outlined { font-size: 16px; }
      .dashboard__kpis {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
        gap: 16px;
      }
      .dashboard__kpi {
        padding: 20px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
        border-top: 4px solid var(--pats-color-primary-container);
        box-shadow: var(--pats-shadow-card);
      }
      .dashboard__kpi-top {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        margin-bottom: 12px;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
      }
      .dashboard__kpi strong {
        display: block;
        font-family: var(--pats-font-display);
        font-size: 28px;
        line-height: 1.2;
      }
      .dashboard__kpi-sub {
        margin: 8px 0 0;
        font-size: 12px;
        color: var(--pats-color-text-secondary);
      }
      .dashboard__kpi-sub--warn { color: var(--pats-color-error); font-weight: 600; }
      .dashboard__trend {
        display: flex;
        align-items: center;
        gap: 4px;
        margin: 8px 0 0;
        font-size: 12px;
        font-weight: 600;
      }
      .dashboard__trend--up { color: var(--pats-color-secondary); }
      .dashboard__trend .material-symbols-outlined { font-size: 14px; }
      .dashboard__middle {
        display: grid;
        grid-template-columns: 2fr 1fr;
        gap: 16px;
      }
      @media (max-width: 960px) {
        .dashboard__middle { grid-template-columns: 1fr; }
      }
      .dashboard__chart-card,
      .dashboard__actions-card,
      .dashboard__activity {
        padding: 24px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
        box-shadow: var(--pats-shadow-card);
      }
      .dashboard__chart-header h2,
      .dashboard__actions-card h2,
      .dashboard__activity h2 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 20px;
        color: var(--pats-color-primary-container);
      }
      .dashboard__chart-header p {
        margin: 4px 0 0;
        font-size: 13px;
        color: var(--pats-color-text-secondary);
      }
      .dashboard__chart-legend {
        display: flex;
        gap: 16px;
        margin: 16px 0;
        font-size: 12px;
        color: var(--pats-color-text-secondary);
      }
      .dashboard__legend-dot {
        display: inline-block;
        width: 10px;
        height: 10px;
        border-radius: 2px;
        margin-right: 6px;
      }
      .dashboard__legend-dot--target { background: var(--pats-color-primary-container); }
      .dashboard__legend-dot--actual { background: var(--pats-color-secondary); }
      .dashboard__chart {
        display: flex;
        align-items: flex-end;
        gap: 12px;
        min-height: 220px;
        padding-top: 8px;
      }
      .dashboard__chart-group {
        flex: 1;
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 8px;
      }
      .dashboard__bars {
        display: flex;
        align-items: flex-end;
        gap: 6px;
        height: 180px;
        width: 100%;
        justify-content: center;
      }
      .dashboard__bar {
        width: 18px;
        min-height: 4px;
        border-radius: 4px 4px 0 0;
        transition: height 0.2s ease;
      }
      .dashboard__bar--target { background: var(--pats-color-primary-container); }
      .dashboard__bar--actual { background: var(--pats-color-secondary); }
      .dashboard__chart-label { font-size: 12px; color: var(--pats-color-text-secondary); }
      .dashboard__actions-card {
        background: linear-gradient(180deg, var(--pats-color-primary-container) 0%, #15325a 100%);
        color: white;
        border: none;
      }
      .dashboard__actions-card h2 { color: white; }
      .dashboard__actions { display: flex; flex-direction: column; gap: 12px; margin-top: 16px; }
      .dashboard__action {
        display: flex;
        gap: 12px;
        align-items: flex-start;
        padding: 14px;
        border-radius: var(--pats-radius-md);
        background: rgba(255, 255, 255, 0.08);
        color: white;
        text-decoration: none;
        transition: background 0.15s ease;
      }
      .dashboard__action:hover { background: rgba(255, 255, 255, 0.14); }
      .dashboard__action strong { display: block; font-size: 14px; }
      .dashboard__action span { display: block; font-size: 12px; opacity: 0.85; }
      .dashboard__system {
        display: flex;
        gap: 10px;
        align-items: flex-start;
        margin-top: 20px;
        padding-top: 16px;
        border-top: 1px solid rgba(255, 255, 255, 0.15);
        font-size: 12px;
      }
      .dashboard__system strong { display: block; font-size: 13px; }
      .dashboard__system-dot {
        width: 10px;
        height: 10px;
        margin-top: 4px;
        border-radius: 50%;
        background: #71dc92;
        flex-shrink: 0;
      }
      .dashboard__activity-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 16px;
      }
      .dashboard__activity-header a {
        font-size: 13px;
        font-weight: 600;
        color: var(--pats-color-primary-container);
        text-decoration: none;
      }
      .dashboard__table {
        width: 100%;
        border-collapse: collapse;
        font-size: 14px;
      }
      .dashboard__table th {
        text-align: left;
        padding: 12px 8px;
        border-bottom: 1px solid var(--pats-color-border-subtle);
        color: var(--pats-color-text-secondary);
        font-size: 12px;
        font-weight: 600;
      }
      .dashboard__table td {
        padding: 14px 8px;
        border-bottom: 1px solid var(--pats-color-border-subtle);
        vertical-align: middle;
      }
      .dashboard__member-cell {
        display: flex;
        align-items: center;
        gap: 12px;
      }
      .dashboard__avatar {
        width: 36px;
        height: 36px;
        border-radius: 50%;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        background: var(--pats-color-surface-container-low);
        color: var(--pats-color-primary-container);
        font-size: 12px;
        font-weight: 700;
      }
      .dashboard__member-cell strong { display: block; }
      .dashboard__member-cell span { display: block; font-size: 12px; color: var(--pats-color-text-secondary); }
      .dashboard__footer {
        display: flex;
        justify-content: space-between;
        gap: 12px;
        flex-wrap: wrap;
        font-size: 12px;
        color: var(--pats-color-text-secondary);
        padding-top: 8px;
      }
    `,
  ],
})
export class DashboardPageComponent implements OnInit {
  protected readonly auth = inject(AuthService);
  private readonly reportsApi = inject(ReportsApiService);

  readonly user = this.auth.user;
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly data = signal<ExecutiveDashboard | null>(null);
  readonly currentYear = new Date().getFullYear();

  readonly chartMax = computed(() => {
    const dashboard = this.data();
    if (!dashboard) return 1;

    const values = dashboard.monthlyPerformance.flatMap((p) => [p.targetCollection, p.actualRecovery]);
    return Math.max(...values, 1);
  });

  protected readonly formatCount = formatCount;
  protected readonly formatCompactInr = formatCompactInr;
  protected readonly formatTrendPercent = formatTrendPercent;
  protected readonly formatRelativeTime = formatRelativeTime;
  protected readonly memberInitials = memberInitials;
  protected readonly activityStatusVariant = activityStatusVariant;

  ngOnInit(): void {
    void this.loadDashboard();
  }

  barHeight(value: number): number {
    const max = this.chartMax();
    return Math.max((value / max) * 100, value > 0 ? 6 : 0);
  }

  private async loadDashboard(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set(null);

    try {
      const branchId = this.auth.user()?.branchId ?? undefined;
      const dashboard = await this.reportsApi.getExecutiveDashboard(branchId ?? undefined);
      this.data.set(dashboard);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to load dashboard.'));
    } finally {
      this.loading.set(false);
    }
  }
}
