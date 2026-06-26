import { Component, ChangeDetectionStrategy, input } from '@angular/core';
import { StatusPillVariant } from '@patsanstha/design-tokens';

@Component({
  selector: 'pats-status-pill',
  standalone: true,
  template: `
    <span
      class="pats-pill"
      [class.pats-pill--active]="variant() === 'active'"
      [class.pats-pill--inactive]="variant() === 'inactive'"
      [class.pats-pill--pending]="variant() === 'pending'"
      [class.pats-pill--warning]="variant() === 'warning'"
      [class.pats-pill--error]="variant() === 'error'"
      [class.pats-pill--info]="variant() === 'info'">
      {{ label() }}
    </span>
  `,
  styles: [
    `
      .pats-pill {
        display: inline-flex;
        align-items: center;
        min-height: 28px;
        padding: 0 12px;
        border-radius: var(--pats-radius-full);
        border: 1px solid transparent;
        font-size: 11px;
        font-weight: 600;
        letter-spacing: 0.02em;
        text-transform: uppercase;
        white-space: nowrap;
      }

      .pats-pill--active {
        background: rgba(46, 158, 91, 0.12);
        color: #00723b;
        border-color: rgba(46, 158, 91, 0.24);
      }

      .pats-pill--inactive {
        background: rgba(116, 119, 128, 0.12);
        color: #43474f;
        border-color: rgba(116, 119, 128, 0.24);
      }

      .pats-pill--pending {
        background: rgba(64, 94, 146, 0.12);
        color: #1a3c6e;
        border-color: rgba(64, 94, 146, 0.24);
      }

      .pats-pill--warning {
        background: rgba(242, 169, 59, 0.14);
        color: #633f00;
        border-color: rgba(242, 169, 59, 0.28);
      }

      .pats-pill--error {
        background: rgba(214, 69, 69, 0.12);
        color: #93000a;
        border-color: rgba(214, 69, 69, 0.24);
      }

      .pats-pill--info {
        background: rgba(26, 60, 110, 0.08);
        color: #1a3c6e;
        border-color: rgba(26, 60, 110, 0.16);
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatsStatusPillComponent {
  readonly label = input.required<string>();
  readonly variant = input<StatusPillVariant>('info');
}
