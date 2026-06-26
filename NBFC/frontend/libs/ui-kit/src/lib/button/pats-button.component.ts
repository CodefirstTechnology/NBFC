import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';

export type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'success';
export type ButtonSize = 'md' | 'sm';

@Component({
  selector: 'pats-button',
  standalone: true,
  template: `
    <button
      [attr.type]="type()"
      class="pats-btn"
      [class.pats-btn--primary]="variant() === 'primary'"
      [class.pats-btn--secondary]="variant() === 'secondary'"
      [class.pats-btn--ghost]="variant() === 'ghost'"
      [class.pats-btn--success]="variant() === 'success'"
      [class.pats-btn--sm]="size() === 'sm'"
      [disabled]="disabled() || loading()"
      (click)="onClick($event)">
      @if (loading()) {
        <span class="material-symbols-outlined pats-btn__spinner">progress_activity</span>
      }
      @if (icon()) {
        <span class="material-symbols-outlined">{{ icon() }}</span>
      }
      <span><ng-content /></span>
    </button>
  `,
  styles: [
    `
      .pats-btn {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 8px;
        min-height: var(--pats-touch-target);
        padding: 0 20px;
        border-radius: var(--pats-radius-md);
        border: 1px solid transparent;
        font-family: var(--pats-font-body);
        font-size: 13px;
        font-weight: 600;
        letter-spacing: 0.01em;
        cursor: pointer;
        transition: background 0.15s ease, border-color 0.15s ease, box-shadow 0.15s ease;
      }

      .pats-btn:disabled {
        opacity: 0.55;
        cursor: not-allowed;
      }

      .pats-btn--sm {
        min-height: 36px;
        padding: 0 14px;
        font-size: 12px;
      }

      .pats-btn--primary {
        background: var(--pats-color-primary-container);
        color: var(--pats-color-on-primary);
      }

      .pats-btn--primary:not(:disabled):hover {
        box-shadow: var(--pats-shadow-card-hover);
      }

      .pats-btn--secondary {
        background: transparent;
        color: var(--pats-color-primary-container);
        border-color: var(--pats-color-primary-container);
      }

      .pats-btn--ghost {
        background: transparent;
        color: var(--pats-color-on-surface-variant);
        border-color: var(--pats-color-border-subtle);
      }

      .pats-btn--success {
        background: var(--pats-color-success);
        color: var(--pats-color-on-primary);
      }

      .pats-btn__spinner {
        animation: spin 1s linear infinite;
      }

      @keyframes spin {
        from {
          transform: rotate(0deg);
        }
        to {
          transform: rotate(360deg);
        }
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatsButtonComponent {
  readonly variant = input<ButtonVariant>('primary');
  readonly size = input<ButtonSize>('md');
  readonly type = input<'button' | 'submit' | 'reset'>('button');
  readonly icon = input<string | null>(null);
  readonly disabled = input(false);
  readonly loading = input(false);
  readonly clicked = output<MouseEvent>();

  onClick(event: MouseEvent): void {
    if (!this.disabled() && !this.loading()) {
      this.clicked.emit(event);
    }
  }
}
