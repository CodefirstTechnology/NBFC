import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';

export interface PatsTableColumn<T> {
  key: string;
  header: string;
  width?: string;
  cell?: (row: T) => string;
}

@Component({
  selector: 'pats-table',
  standalone: true,
  template: `
    <div class="pats-table-wrap">
      <table class="pats-table">
        <thead>
          <tr>
            @for (column of columns(); track column.key) {
              <th [style.width]="column.width ?? 'auto'">{{ column.header }}</th>
            }
          </tr>
        </thead>
        <tbody>
          @if (loading()) {
            <tr>
              <td [attr.colspan]="columns().length" class="pats-table__empty">Loading…</td>
            </tr>
          } @else if (!rows()?.length) {
            <tr>
              <td [attr.colspan]="columns().length" class="pats-table__empty">{{ emptyMessage() }}</td>
            </tr>
          } @else {
            @for (row of rows(); track $index) {
              <tr
                class="pats-table__row"
                [class.pats-table__row--clickable]="rowClickable()"
                (click)="rowClickable() && rowClicked.emit(row)">
                @for (column of columns(); track column.key) {
                  <td>
                    {{ column.cell ? column.cell(row) : $any(row)[column.key] }}
                  </td>
                }
              </tr>
            }
          }
        </tbody>
      </table>
    </div>
  `,
  styles: [
    `
      .pats-table-wrap {
        overflow: auto;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-lg);
        background: var(--pats-color-surface-container-lowest);
        box-shadow: var(--pats-shadow-card);
      }

      .pats-table {
        width: 100%;
        border-collapse: collapse;
      }

      .pats-table thead th {
        position: sticky;
        top: 0;
        z-index: 1;
        height: var(--pats-table-row-height);
        padding: 0 16px;
        text-align: left;
        font-size: 13px;
        font-weight: 600;
        color: var(--pats-color-on-surface-variant);
        background: var(--pats-color-surface-muted);
        border-bottom: 1px solid var(--pats-color-border-subtle);
      }

      .pats-table tbody td {
        height: var(--pats-table-row-height);
        padding: 0 16px;
        font-size: 14px;
        color: var(--pats-color-on-surface);
        border-bottom: 1px solid var(--pats-color-border-subtle);
      }

      .pats-table tbody tr:nth-child(even) td {
        background: var(--pats-color-surface-muted);
      }

      .pats-table__row--clickable {
        cursor: pointer;
      }

      .pats-table__row--clickable:hover td {
        background: rgba(26, 60, 110, 0.06);
      }

      .pats-table__empty {
        text-align: center;
        color: var(--pats-color-text-secondary);
        padding: 24px !important;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatsTableComponent<T extends object> {
  readonly columns = input.required<PatsTableColumn<T>[]>();
  readonly rows = input<T[] | null>([]);
  readonly loading = input(false);
  readonly emptyMessage = input('No records found.');
  readonly rowClickable = input(false);
  readonly rowClicked = output<T>();
}
