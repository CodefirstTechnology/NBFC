import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  AccountingApiService,
  extractApiErrorMessage,
} from '@patsanstha/accounting-data-access';
import { PatsButtonComponent, PatsFormFieldComponent } from '@patsanstha/ui-kit';

@Component({
  selector: 'pats-journal-create-page',
  standalone: true,
  imports: [FormsModule, RouterLink, PatsButtonComponent, PatsFormFieldComponent],
  template: `
    <section class="create-page">
      <header class="create-page__header">
        <a routerLink="/accounting" class="create-page__back">
          <span class="material-symbols-outlined">arrow_back</span>
          Back to journal entries
        </a>
        <h1>New Journal Entry</h1>
        <p>Create a draft journal entry. Post it from the detail view when ready.</p>
      </header>

      @if (errorMessage()) {
        <p class="create-page__error">{{ errorMessage() }}</p>
      }

      <article class="create-page__card">
        <div class="create-page__form">
          <pats-form-field label="Description">
            <input type="text" [(ngModel)]="description" />
          </pats-form-field>
          <pats-form-field label="Entry Date">
            <input type="date" [(ngModel)]="entryDate" />
          </pats-form-field>
          <pats-form-field label="Debit Account Code">
            <input type="text" [(ngModel)]="debitAccountCode" />
          </pats-form-field>
          <pats-form-field label="Credit Account Code">
            <input type="text" [(ngModel)]="creditAccountCode" />
          </pats-form-field>
          <pats-form-field label="Amount (₹)">
            <input type="number" min="0.01" step="0.01" [(ngModel)]="amount" />
          </pats-form-field>
          <pats-form-field label="Reference Type (optional)">
            <input type="text" [(ngModel)]="referenceType" />
          </pats-form-field>
          <pats-form-field label="Reference Id (optional)">
            <input type="text" [(ngModel)]="referenceId" />
          </pats-form-field>
        </div>
      </article>

      <div class="create-page__actions">
        <pats-button variant="ghost" (clicked)="cancel()">Cancel</pats-button>
        <pats-button [loading]="loading()" (clicked)="submit()">Create Draft</pats-button>
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
      .create-page__actions { display: flex; justify-content: flex-end; gap: 12px; }
      .create-page__error { color: var(--pats-color-error); }
    `,
  ],
})
export class JournalCreatePageComponent {
  private readonly router = inject(Router);
  private readonly accountingApi = inject(AccountingApiService);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  description = '';
  entryDate = new Date().toISOString().slice(0, 10);
  debitAccountCode = '';
  creditAccountCode = '';
  amount = 0;
  referenceType = '';
  referenceId = '';

  async submit(): Promise<void> {
    if (!this.description.trim() || !this.debitAccountCode.trim() || !this.creditAccountCode.trim() || this.amount <= 0) {
      this.errorMessage.set('Fill in description, accounts, and a positive amount.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    try {
      const detail = await this.accountingApi.create({
        description: this.description.trim(),
        entryDate: this.entryDate,
        debitAccountCode: this.debitAccountCode.trim(),
        creditAccountCode: this.creditAccountCode.trim(),
        amount: this.amount,
        referenceType: this.referenceType.trim() || null,
        referenceId: this.referenceId.trim() || null,
      });
      void this.router.navigate(['/accounting', detail.id]);
    } catch (error) {
      this.errorMessage.set(extractApiErrorMessage(error, 'Failed to create journal entry.'));
    } finally {
      this.loading.set(false);
    }
  }

  cancel(): void {
    void this.router.navigate(['/accounting']);
  }
}
