import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  REPORT_TYPES,
  ReportType,
  ReportsApiService,
  extractApiErrorMessage,
} from '@patsanstha/reports-data-access';
import { PatsButtonComponent, PatsFormFieldComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-report-generate-page',
  standalone: true,
  imports: [FormsModule, RouterLink, PatsButtonComponent, PatsFormFieldComponent],
  template: `
    <section class="create-page">
      <header class="create-page__header">
        <a routerLink="/reports" class="create-page__back">
          <span class="material-symbols-outlined">arrow_back</span>
          Back to reports
        </a>
        <h1>Generate Report</h1>
        <p>Select a report type and title to generate a new snapshot.</p>
      </header>

      @if (errorMessage()) {
        <p class="create-page__error">{{ errorMessage() }}</p>
      }

      <article class="create-page__card">
        <div class="create-page__form">
          <pats-form-field label="Report Type">
            <select [(ngModel)]="reportType">
              @for (type of reportTypes; track type.reportType) {
                <option [ngValue]="type.reportType">{{ type.label }}</option>
              }
            </select>
          </pats-form-field>
          <pats-form-field label="Title">
            <input type="text" [(ngModel)]="title" />
          </pats-form-field>
        </div>
      </article>

      <div class="create-page__actions">
        <pats-button variant="ghost" (clicked)="cancel()">Cancel</pats-button>
        <pats-button [loading]="loading()" (clicked)="submit()">Generate</pats-button>
      </div>
    </section>
  `,
  styles: [
    `
      .create-page { display: flex; flex-direction: column; gap: 24px; max-width: 720px; }
      .create-page__back { display: inline-flex; align-items: center; gap: 4px; color: var(--pats-color-primary-container); font-weight: 600; font-size: 14px; }
      .create-page__header h1 { margin: 0; font-family: var(--pats-font-display); font-size: 32px; }
      .create-page__header p { margin: 8px 0 0; color: var(--pats-color-text-secondary); }
      .create-page__card { padding: 24px; border-radius: var(--pats-radius-lg); background: var(--pats-color-surface-container-lowest); border: 1px solid var(--pats-color-border-subtle); }
      .create-page__form { display: grid; gap: 16px; }
      .create-page__form select, .create-page__form input { width: 100%; min-height: 44px; padding: 0 12px; border: 1px solid var(--pats-color-border-subtle); border-radius: var(--pats-radius-md); }
      .create-page__actions { display: flex; justify-content: flex-end; gap: 12px; }
      .create-page__error { color: var(--pats-color-error); }
    `,
  ],
})
export class ReportGeneratePageComponent {
  private readonly router = inject(Router);
  private readonly reportsApi = inject(ReportsApiService);

  readonly reportTypes = REPORT_TYPES;
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  reportType: ReportType = ReportType.BranchSummary;
  title = '';

  async submit(): Promise<void> {
    if (!this.title.trim()) {
      this.errorMessage.set('Enter a report title.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    try {
      const detail = await this.reportsApi.generate({
        reportType: this.reportType,
        title: this.title.trim(),
      });
      void this.router.navigate(['/reports', detail.id]);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to generate report.'));
    } finally {
      this.loading.set(false);
    }
  }

  cancel(): void {
    void this.router.navigate(['/reports']);
  }
}
