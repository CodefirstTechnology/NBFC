import { Component, ChangeDetectionStrategy, input, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormsModule } from '@angular/forms';

@Component({
  selector: 'pats-form-field',
  standalone: true,
  imports: [FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PatsFormFieldComponent),
      multi: true,
    },
  ],
  template: `
    <div class="pats-field" [class.pats-field--error]="!!error()">
      <label class="pats-field__label" [attr.for]="inputId">
        {{ label() }}
        @if (hint()) {
          <span class="pats-field__hint">{{ hint() }}</span>
        }
      </label>
      <input
        [id]="inputId"
        class="pats-field__input"
        [type]="type()"
        [placeholder]="placeholder()"
        [disabled]="isDisabled"
        [attr.autocomplete]="autocomplete()"
        [ngModel]="value"
        (ngModelChange)="onInput($event)"
        (blur)="onTouched()" />
      @if (error()) {
        <p class="pats-field__error">{{ error() }}</p>
      }
    </div>
  `,
  styles: [
    `
      .pats-field {
        display: flex;
        flex-direction: column;
        gap: 8px;
      }

      .pats-field__label {
        display: flex;
        flex-direction: column;
        gap: 2px;
        font-size: 13px;
        font-weight: 600;
        color: var(--pats-color-on-surface-variant);
      }

      .pats-field__hint {
        font-size: 11px;
        font-weight: 500;
        color: var(--pats-color-text-secondary);
      }

      .pats-field__input {
        min-height: var(--pats-touch-target);
        padding: 0 14px;
        border: 1px solid var(--pats-color-border-subtle);
        border-radius: var(--pats-radius-md);
        background: var(--pats-color-surface-container-lowest);
        color: var(--pats-color-on-surface);
        font-family: var(--pats-font-body);
        font-size: 16px;
        transition: border-color 0.15s ease, box-shadow 0.15s ease;
      }

      .pats-field__input:focus {
        outline: none;
        border-color: var(--pats-color-primary-container);
        box-shadow: 0 0 0 2px rgba(26, 60, 110, 0.12);
      }

      .pats-field__input:disabled {
        background: var(--pats-color-surface-container-low);
        cursor: not-allowed;
      }

      .pats-field--error .pats-field__input {
        border-color: var(--pats-color-error);
      }

      .pats-field__error {
        margin: 0;
        font-size: 12px;
        color: var(--pats-color-error);
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatsFormFieldComponent implements ControlValueAccessor {
  readonly label = input.required<string>();
  readonly hint = input<string | null>(null);
  readonly placeholder = input('');
  readonly type = input<'text' | 'email' | 'password' | 'tel' | 'number' | 'date'>('text');
  readonly error = input<string | null>(null);
  readonly autocomplete = input<string | null>(null);

  readonly inputId = `pats-field-${Math.random().toString(36).slice(2, 9)}`;

  value = '';
  isDisabled = false;

  private onChange: (value: string) => void = () => undefined;
  protected onTouched: () => void = () => undefined;

  writeValue(value: string | null): void {
    this.value = value ?? '';
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
  }

  onInput(value: string): void {
    this.value = value;
    this.onChange(value);
  }
}
