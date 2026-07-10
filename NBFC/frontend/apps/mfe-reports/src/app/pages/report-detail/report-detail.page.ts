import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  ReportSnapshotDetail,
  ReportsApiService,
  downloadReportPdf,
  extractApiErrorMessage,
  reportStatusLabel,
  reportStatusVariant,
  reportTypeLabel,
} from '@patsanstha/reports-data-access';
import { PatsButtonComponent, PatsStatusPillComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-report-detail-page',
  standalone: true,
  imports: [RouterLink, PatsButtonComponent, PatsStatusPillComponent],
  template: `
    <section class="detail-page">
      <a routerLink="/reports" class="detail-page__back">
        <span class="material-symbols-outlined">arrow_back</span>
        Back to reports
      </a>

      @if (loading()) {
        <p>Loading report…</p>
      } @else if (errorMessage()) {
        <p class="detail-page__error">{{ errorMessage() }}</p>
      } @else {
        @if (report(); as r) {
          <header class="detail-page__header">
            <div>
              <p class="detail-page__eyebrow">{{ typeLabel() }}</p>
              <h1>{{ r.title }}</h1>
              <p>Generated {{ r.generatedAt }}</p>
            </div>
            <div class="detail-page__header-actions">
              <pats-status-pill [label]="statusLabel()" [variant]="statusVariant()" />
              <div class="detail-page__downloads">
                <pats-button
                  variant="secondary"
                  size="sm"
                  icon="picture_as_pdf"
                  [loading]="downloadingPdf()"
                  [disabled]="downloadingPdf()"
                  (clicked)="downloadPdf()">
                  Download PDF
                </pats-button>
              </div>
            </div>
          </header>

          @if (downloadError()) {
            <p class="detail-page__error">{{ downloadError() }}</p>
          }

          <article class="detail-page__card">
            <h2>Result</h2>
            <pre class="detail-page__json">{{ formattedResult() }}</pre>
          </article>
        }
      }
    </section>
  `,
  styles: [
    `
      .detail-page { display: flex; flex-direction: column; gap: 24px; }
      .detail-page__back { display: inline-flex; align-items: center; gap: 4px; color: var(--pats-color-primary-container); font-weight: 600; font-size: 14px; }
      .detail-page__header { display: flex; justify-content: space-between; gap: 16px; padding: 24px; border-radius: var(--pats-radius-lg); background: var(--pats-color-surface-container-lowest); border: 1px solid var(--pats-color-border-subtle); border-top: 4px solid var(--pats-color-primary-container); }
      .detail-page__header-actions { display: flex; flex-direction: column; align-items: flex-end; gap: 12px; }
      .detail-page__downloads { display: flex; flex-wrap: wrap; justify-content: flex-end; gap: 8px; }
      .detail-page__eyebrow { margin: 0 0 4px; font-size: 13px; font-weight: 600; text-transform: uppercase; color: var(--pats-color-text-secondary); }
      .detail-page__header h1 { margin: 0; font-family: var(--pats-font-display); font-size: 32px; }
      .detail-page__card { padding: 24px; border-radius: var(--pats-radius-lg); background: var(--pats-color-surface-container-lowest); border: 1px solid var(--pats-color-border-subtle); }
      .detail-page__card h2 { margin: 0 0 16px; font-family: var(--pats-font-display); color: var(--pats-color-primary-container); }
      .detail-page__json { margin: 0; padding: 16px; border-radius: var(--pats-radius-md); background: var(--pats-color-surface-muted); overflow: auto; font-size: 13px; line-height: 1.5; white-space: pre-wrap; word-break: break-word; }
      .detail-page__error { color: var(--pats-color-error); }
    `,
  ],
})
export class ReportDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly reportsApi = inject(ReportsApiService);

  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly downloadError = signal<string | null>(null);
  readonly downloadingPdf = signal(false);
  readonly report = signal<ReportSnapshotDetail | null>(null);
  readonly statusLabel = signal('');
  readonly statusVariant = signal<'active' | 'inactive' | 'pending' | 'warning' | 'error'>('pending');
  readonly typeLabel = signal('');

  readonly formattedResult = computed(() => {
    const r = this.report();
    if (!r?.resultJson) return '';
    try {
      return JSON.stringify(JSON.parse(r.resultJson), null, 2);
    } catch {
      return r.resultJson;
    }
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage.set('Report id missing.');
      this.loading.set(false);
      return;
    }
    void this.loadReport(id);
  }

  downloadPdf(): void {
    const report = this.report();
    if (!report) return;

    this.downloadingPdf.set(true);
    this.downloadError.set(null);

    try {
      downloadReportPdf(report);
    } catch (error) {
      this.downloadError.set(extractApiErrorMessage(error, 'Failed to generate PDF.'));
    } finally {
      this.downloadingPdf.set(false);
    }
  }

  private async loadReport(id: string): Promise<void> {
    this.loading.set(true);
    try {
      const detail = await this.reportsApi.getById(id);
      this.report.set(detail);
      this.statusLabel.set(reportStatusLabel(detail.status));
      this.statusVariant.set(reportStatusVariant(detail.status));
      this.typeLabel.set(reportTypeLabel(detail.reportType));
      this.errorMessage.set(null);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Report not found.'));
    } finally {
      this.loading.set(false);
    }
  }
}
