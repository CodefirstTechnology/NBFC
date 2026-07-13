import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { HasPermissionDirective } from '@patsanstha/auth';
import {
  ReportType,
  ReportsApiService,
  extractApiErrorMessage,
} from '@patsanstha/reports-data-access';
import { REPORT_HUB_CARDS, ReportHubCard } from '../report-hub.config';
import { downloadReportPdf } from '../report-pdf.local';

@Component({
  selector: 'pats-report-list-page',
  standalone: true,
  imports: [HasPermissionDirective],
  template: `
    <section class="hub">
      <header class="hub__header">
        <div>
          <h1>
            Reports Hub
            <span class="hub__title-mr">/ अहवाल केंद्र</span>
          </h1>
          <p>
            Access comprehensive financial analytics and administrative reports for current branch operations.
          </p>
        </div>
        <div class="hub__header-actions">
          <button type="button" class="hub__btn hub__btn--outline">
            <span class="material-symbols-outlined">tune</span>
            Global Filters
          </button>
          <button type="button" class="hub__btn hub__btn--primary">
            <span class="material-symbols-outlined">schedule</span>
            Schedule All
          </button>
        </div>
      </header>

      @if (errorMessage()) {
        <p class="hub__error">{{ errorMessage() }}</p>
      }

      <div class="hub__grid">
        @for (card of cards; track card.id) {
          <article class="hub-card" [class]="'hub-card--' + card.iconTone">
            <div class="hub-card__top">
              <span class="hub-card__icon">
                <span class="material-symbols-outlined">{{ card.icon }}</span>
              </span>
              @if (card.badge) {
                <span class="hub-card__badge" [class]="'hub-card__badge--' + card.badge.tone">
                  {{ card.badge.label }}
                </span>
              }
            </div>

            <h2>{{ card.title }}</h2>
            <p class="hub-card__title-mr">{{ card.titleMr }}</p>
            <p class="hub-card__desc">{{ card.description }}</p>
            <p class="hub-card__desc-mr">{{ card.descriptionMr }}</p>

            <div class="hub-card__visual">
              @switch (card.visual) {
                @case ('bars') {
                  <div class="hub-card__bars">
                    @for (height of barHeights; track $index) {
                      <span [style.height.%]="height"></span>
                    }
                  </div>
                }
                @case ('line') {
                  <svg class="hub-card__line" viewBox="0 0 200 60" preserveAspectRatio="none">
                    <polyline points="0,45 40,35 80,40 120,20 160,28 200,10" />
                  </svg>
                }
                @case ('columns') {
                  <div class="hub-card__columns">
                    @for (height of columnHeights; track $index) {
                      <span [style.height.%]="height"></span>
                    }
                  </div>
                }
                @case ('progress') {
                  <div class="hub-card__progress">
                    <div class="hub-card__progress-track">
                      <div class="hub-card__progress-fill" [style.width]="card.visualValue"></div>
                    </div>
                    <span>{{ card.visualValue }}</span>
                  </div>
                }
                @case ('ring') {
                  <div class="hub-card__ring">
                    <svg viewBox="0 0 36 36">
                      <path
                        class="hub-card__ring-bg"
                        d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
                      <path
                        class="hub-card__ring-fill"
                        stroke-dasharray="42, 100"
                        d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
                    </svg>
                  </div>
                }
                @case ('grades') {
                  <div class="hub-card__grades">
                    @for (grade of grades; track grade) {
                      <span [class.hub-card__grade--active]="grade === 'C'">{{ grade }}</span>
                    }
                  </div>
                }
              }
            </div>

            <footer class="hub-card__footer">
              <button
                type="button"
                class="hub-card__view"
                [disabled]="loadingCardId() === card.id"
                (click)="viewReport(card)">
                @if (loadingCardId() === card.id) {
                  <span class="material-symbols-outlined hub-card__spinner">progress_activity</span>
                }
                View Report
              </button>
              <button
                type="button"
                class="hub-card__download"
                *patsHasPermission="'reports.export'"
                title="Download PDF"
                [disabled]="downloadingCardId() === card.id"
                (click)="downloadReport(card)">
                <span class="material-symbols-outlined">
                  {{ downloadingCardId() === card.id ? 'progress_activity' : 'picture_as_pdf' }}
                </span>
              </button>
            </footer>
          </article>
        }
      </div>
    </section>
  `,
  styles: [
    `
      .hub { display: flex; flex-direction: column; gap: 28px; }
      .hub__header {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 20px;
        flex-wrap: wrap;
      }
      .hub__header h1 {
        margin: 0;
        font-family: var(--pats-font-display);
        font-size: 32px;
        color: var(--pats-color-primary-container);
      }
      .hub__title-mr {
        font-size: 22px;
        font-weight: 500;
        color: var(--pats-color-text-secondary);
      }
      .hub__header p {
        margin: 10px 0 0;
        max-width: 640px;
        color: var(--pats-color-text-secondary);
        line-height: 1.5;
      }
      .hub__header-actions { display: flex; gap: 12px; flex-wrap: wrap; }
      .hub__btn {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        min-height: 44px;
        padding: 0 18px;
        border-radius: var(--pats-radius-md);
        font-size: 14px;
        font-weight: 600;
        cursor: pointer;
        transition: background 0.15s ease, border-color 0.15s ease;
      }
      .hub__btn--outline {
        border: 1px solid var(--pats-color-border-subtle);
        background: var(--pats-color-surface-container-lowest);
        color: var(--pats-color-primary-container);
      }
      .hub__btn--primary {
        border: none;
        background: var(--pats-color-primary-container);
        color: #fff;
      }
      .hub__error { color: var(--pats-color-error); }
      .hub__grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
        gap: 20px;
      }
      .hub-card {
        display: flex;
        flex-direction: column;
        gap: 8px;
        padding: 22px;
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        border: 1px solid var(--pats-color-border-subtle);
        border-top: 4px solid var(--pats-color-primary-container);
        box-shadow: var(--pats-shadow-card);
      }
      .hub-card--orange { border-top-color: #e0992c; }
      .hub-card--red { border-top-color: var(--pats-color-error-npa); }
      .hub-card--green { border-top-color: var(--pats-color-secondary); }
      .hub-card__top {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 12px;
      }
      .hub-card__icon {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 44px;
        height: 44px;
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-surface-container-low);
        color: var(--pats-color-primary-container);
      }
      .hub-card--orange .hub-card__icon { color: #c77b00; background: #fff4e5; }
      .hub-card--red .hub-card__icon { color: var(--pats-color-error-npa); background: #ffeded; }
      .hub-card--green .hub-card__icon { color: var(--pats-color-secondary); background: #e8f9ee; }
      .hub-card__badge {
        padding: 4px 10px;
        border-radius: 999px;
        font-size: 10px;
        font-weight: 700;
        letter-spacing: 0.04em;
      }
      .hub-card__badge--success { background: #e8f9ee; color: var(--pats-color-secondary); }
      .hub-card__badge--warning { background: #fff4e5; color: #c77b00; }
      .hub-card__badge--error { background: #ffeded; color: var(--pats-color-error-npa); }
      .hub-card h2 {
        margin: 4px 0 0;
        font-family: var(--pats-font-display);
        font-size: 22px;
        color: var(--pats-color-primary-container);
      }
      .hub-card__title-mr {
        margin: 0;
        font-size: 14px;
        font-weight: 600;
        color: var(--pats-color-text-secondary);
      }
      .hub-card__desc,
      .hub-card__desc-mr {
        margin: 0;
        font-size: 13px;
        line-height: 1.45;
        color: var(--pats-color-text-secondary);
      }
      .hub-card__desc-mr { font-size: 12px; opacity: 0.85; }
      .hub-card__visual {
        min-height: 72px;
        display: flex;
        align-items: flex-end;
        margin: 8px 0 4px;
      }
      .hub-card__bars,
      .hub-card__columns {
        display: flex;
        align-items: flex-end;
        gap: 8px;
        width: 100%;
        height: 56px;
      }
      .hub-card__bars span {
        flex: 1;
        border-radius: 4px 4px 0 0;
        background: var(--pats-color-primary-container);
      }
      .hub-card__bars span:nth-child(even) { background: #d7e3ff; }
      .hub-card__columns span {
        width: 14px;
        border-radius: 4px 4px 0 0;
        background: var(--pats-color-secondary);
        opacity: 0.85;
      }
      .hub-card__line {
        width: 100%;
        height: 56px;
      }
      .hub-card__line polyline {
        fill: none;
        stroke: var(--pats-color-primary-container);
        stroke-width: 3;
        stroke-linecap: round;
        stroke-linejoin: round;
      }
      .hub-card__progress {
        display: flex;
        align-items: center;
        gap: 12px;
        width: 100%;
      }
      .hub-card__progress-track {
        flex: 1;
        height: 10px;
        border-radius: 999px;
        background: #f1e4d0;
        overflow: hidden;
      }
      .hub-card__progress-fill {
        height: 100%;
        border-radius: 999px;
        background: #e0992c;
      }
      .hub-card__progress span {
        font-size: 13px;
        font-weight: 700;
        color: #c77b00;
      }
      .hub-card__ring {
        width: 56px;
        height: 56px;
      }
      .hub-card__ring svg { width: 100%; height: 100%; }
      .hub-card__ring-bg,
      .hub-card__ring-fill {
        fill: none;
        stroke-width: 3;
      }
      .hub-card__ring-bg { stroke: #ffd9d9; }
      .hub-card__ring-fill { stroke: var(--pats-color-error-npa); }
      .hub-card__grades {
        display: flex;
        gap: 8px;
      }
      .hub-card__grades span {
        width: 32px;
        height: 32px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        border-radius: var(--pats-radius-md);
        border: 1px solid var(--pats-color-border-subtle);
        font-size: 13px;
        font-weight: 700;
        color: var(--pats-color-text-secondary);
        background: var(--pats-color-surface-container-low);
      }
      .hub-card__grade--active {
        border-color: var(--pats-color-secondary);
        background: #e8f9ee;
        color: var(--pats-color-secondary);
      }
      .hub-card__footer {
        display: flex;
        gap: 10px;
        margin-top: auto;
        padding-top: 8px;
      }
      .hub-card__view {
        flex: 1;
        min-height: 44px;
        border: none;
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-primary-container);
        color: #fff;
        font-size: 14px;
        font-weight: 600;
        cursor: pointer;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 6px;
      }
      .hub-card__view:disabled { opacity: 0.6; cursor: not-allowed; }
      .hub-card__download {
        width: 44px;
        min-height: 44px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-surface-container-lowest);
        color: var(--pats-color-primary-container);
        cursor: pointer;
        display: inline-flex;
        align-items: center;
        justify-content: center;
      }
      .hub-card__download:disabled { opacity: 0.6; cursor: not-allowed; }
      .hub-card__spinner { animation: spin 1s linear infinite; }
      @keyframes spin { to { transform: rotate(360deg); } }
    `,
  ],
})
export class ReportListPageComponent {
  private readonly router = inject(Router);
  private readonly reportsApi = inject(ReportsApiService);

  readonly cards = REPORT_HUB_CARDS;
  readonly barHeights = [55, 72, 48, 80, 62, 90, 70];
  readonly columnHeights = [40, 65, 50, 80, 55, 70, 45];
  readonly grades = ['A', 'B', 'C', 'D', 'E'];

  readonly errorMessage = signal<string | null>(null);
  readonly loadingCardId = signal<string | null>(null);
  readonly downloadingCardId = signal<string | null>(null);

  async viewReport(card: ReportHubCard): Promise<void> {
    this.errorMessage.set(null);
    this.loadingCardId.set(card.id);

    try {
      if (card.route) {
        await this.router.navigate([card.route]);
        return;
      }

      if (card.reportType === undefined) {
        return;
      }

      const snapshot = await this.findOrCreateSnapshot(card.reportType, card.title);
      await this.router.navigate(['/reports', snapshot.id]);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to open report.'));
    } finally {
      this.loadingCardId.set(null);
    }
  }

  async downloadReport(card: ReportHubCard): Promise<void> {
    if (card.reportType === undefined) {
      if (card.route) {
        await this.router.navigate([card.route]);
        return;
      }

      this.errorMessage.set('PDF download is not available for this report yet.');
      return;
    }

    this.errorMessage.set(null);
    this.downloadingCardId.set(card.id);

    try {
      const snapshot = await this.findOrCreateSnapshot(card.reportType, card.title);
      const detail = await this.reportsApi.getById(snapshot.id);
      downloadReportPdf(detail);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to download PDF.'));
    } finally {
      this.downloadingCardId.set(null);
    }
  }

  private async findOrCreateSnapshot(reportType: ReportType, title: string) {
    const existing = await this.reportsApi.list({ reportType, pageSize: 1 });
    if (existing.items.length > 0) {
      return existing.items[0];
    }

    return this.reportsApi.generate({ reportType, title });
  }
}
