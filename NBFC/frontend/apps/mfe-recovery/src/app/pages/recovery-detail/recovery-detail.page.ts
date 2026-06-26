import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '@patsanstha/auth';
import {
  RecoveryApiService,
  RecoveryCaseDetail,
  RecoveryCaseStatus,
  extractApiErrorMessage,
  formatInr,
  recoveryStatusLabel,
  recoveryStatusVariant,
} from '@patsanstha/recovery-data-access';
import { PatsButtonComponent, PatsFormFieldComponent, PatsStatusPillComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-recovery-detail-page',
  standalone: true,
  imports: [RouterLink, FormsModule, HasPermissionDirective, PatsButtonComponent, PatsFormFieldComponent, PatsStatusPillComponent],
  template: `
    <section class="detail-page">
      <a routerLink="/recovery" class="detail-page__back">
        <span class="material-symbols-outlined">arrow_back</span>
        Back to recovery cases
      </a>

      @if (loading()) {
        <p>Loading case…</p>
      } @else if (errorMessage()) {
        <p class="detail-page__error">{{ errorMessage() }}</p>
      } @else {
        @if (recoveryCase(); as c) {
          <header class="detail-page__header">
            <div>
              <p class="detail-page__eyebrow">{{ c.caseNumber }}</p>
              <h1>{{ c.memberName }}</h1>
              <p>Loan {{ c.loanNumber }} · {{ c.memberNumber }}</p>
            </div>
            <pats-status-pill [label]="statusLabel()" [variant]="statusVariant()" />
          </header>

          <div class="detail-page__amount">
            <span>Outstanding Amount</span>
            <strong>{{ formatInr(c.outstandingAmount) }}</strong>
            <small>{{ c.daysPastDue }} days past due</small>
          </div>

          <div class="detail-page__grid">
            <article class="detail-page__card">
              <h2>Case Info</h2>
              <dl>
                <dt>Opened On</dt><dd>{{ c.openedOn }}</dd>
                <dt>Resolved On</dt><dd>{{ c.resolvedOn ?? '—' }}</dd>
                <dt>Created</dt><dd>{{ c.createdAt }}</dd>
                <dt>Modified</dt><dd>{{ c.modifiedAt ?? '—' }}</dd>
              </dl>
            </article>

            <article class="detail-page__card">
              <h2>Current Notes</h2>
              <p>{{ c.notes ?? 'No notes recorded.' }}</p>
            </article>
          </div>

          <article class="detail-page__card detail-page__update">
            <h2>Update Case</h2>
            <div class="detail-page__form">
              <pats-form-field label="Status">
                <select [(ngModel)]="editStatus">
                  <option [ngValue]="0">Open</option>
                  <option [ngValue]="1">In Progress</option>
                  <option [ngValue]="2">Resolved</option>
                  <option [ngValue]="3">Written Off</option>
                </select>
              </pats-form-field>
              <pats-form-field label="Notes">
                <textarea rows="4" [(ngModel)]="editNotes" placeholder="Recovery action notes…"></textarea>
              </pats-form-field>
            </div>
            <div class="detail-page__actions">
              <pats-button
                *patsHasPermission="'recovery.manage'"
                [loading]="actionLoading()"
                (clicked)="save()">
                Save Changes
              </pats-button>
            </div>
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
      .detail-page__eyebrow { margin: 0 0 4px; font-size: 13px; font-weight: 600; text-transform: uppercase; color: var(--pats-color-text-secondary); }
      .detail-page__header h1 { margin: 0; font-family: var(--pats-font-display); font-size: 32px; }
      .detail-page__amount { padding: 24px; border-radius: var(--pats-radius-lg); background: var(--pats-color-primary); color: var(--pats-color-on-primary); display: flex; flex-direction: column; gap: 8px; }
      .detail-page__amount strong { font-family: var(--pats-font-display); font-size: 36px; }
      .detail-page__amount small { opacity: 0.85; }
      .detail-page__grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 16px; }
      .detail-page__card { padding: 24px; border-radius: var(--pats-radius-lg); background: var(--pats-color-surface-container-lowest); border: 1px solid var(--pats-color-border-subtle); }
      .detail-page__card h2 { margin: 0 0 16px; font-family: var(--pats-font-display); color: var(--pats-color-primary-container); }
      .detail-page__update .detail-page__form { display: grid; gap: 16px; margin-bottom: 16px; }
      dl { margin: 0; display: grid; gap: 12px; }
      dt { font-size: 11px; font-weight: 600; text-transform: uppercase; color: var(--pats-color-text-secondary); }
      dd { margin: 0; font-size: 15px; }
      .detail-page__actions { display: flex; flex-wrap: wrap; gap: 12px; }
      .detail-page__error { color: var(--pats-color-error); }
    `,
  ],
})
export class RecoveryDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly recoveryApi = inject(RecoveryApiService);

  readonly loading = signal(true);
  readonly actionLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly recoveryCase = signal<RecoveryCaseDetail | null>(null);
  readonly statusLabel = signal('');
  readonly statusVariant = signal<'active' | 'inactive' | 'pending' | 'warning' | 'error'>('warning');

  editStatus: RecoveryCaseStatus = RecoveryCaseStatus.Open;
  editNotes = '';

  protected readonly formatInr = formatInr;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { this.errorMessage.set('Case id missing.'); this.loading.set(false); return; }
    void this.loadCase(id);
  }

  async save(): Promise<void> {
    const c = this.recoveryCase();
    if (!c) return;
    this.actionLoading.set(true);
    try {
      const updated = await this.recoveryApi.update(c.id, {
        status: this.editStatus,
        notes: this.editNotes.trim() || null,
      });
      this.setCase(updated);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to update recovery case.'));
    } finally {
      this.actionLoading.set(false);
    }
  }

  private async loadCase(id: string): Promise<void> {
    this.loading.set(true);
    try {
      const detail = await this.recoveryApi.getById(id);
      this.setCase(detail);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Recovery case not found.'));
    } finally {
      this.loading.set(false);
    }
  }

  private setCase(detail: RecoveryCaseDetail): void {
    this.recoveryCase.set(detail);
    this.statusLabel.set(recoveryStatusLabel(detail.status));
    this.statusVariant.set(recoveryStatusVariant(detail.status));
    this.editStatus = detail.status;
    this.editNotes = detail.notes ?? '';
    this.errorMessage.set(null);
  }
}
