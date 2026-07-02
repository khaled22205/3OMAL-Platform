import { Component, input, computed, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';

export interface SelectOption {
  value: string | number;
  label: string;
}

@Component({
  selector: 'app-select',
  standalone: true,
  imports: [CommonModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectComponent),
      multi: true,
    },
  ],
  template: `
    <div class="flex flex-col gap-1.5">
      @if (label()) {
        <label class="text-sm font-bold text-slate-700 dark:text-slate-300 text-right">
          {{ label() }}
          @if (required()) { <span class="text-rose-500">*</span> }
        </label>
      }
      <div class="relative">
        <select
          [value]="value"
          [disabled]="disabled()"
          (change)="onSelect($event)"
          (blur)="onTouched()"
          class="w-full px-4 py-3 bg-slate-50 dark:bg-slate-900 border rounded-xl outline-none transition-all text-right font-bold text-slate-800 dark:text-white appearance-none cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
          [ngClass]="{
            'border-slate-200 dark:border-slate-700 focus:border-primary dark:focus:border-primary': !error(),
            'border-rose-300 dark:border-rose-700 focus:border-rose-500': !!error(),
          }"
        >
          @if (placeholder()) {
            <option value="" disabled selected>{{ placeholder() }}</option>
          }
          @for (opt of options(); track opt.value) {
            <option [value]="opt.value">{{ opt.label }}</option>
          }
        </select>
        <svg
          class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400 pointer-events-none"
          fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"
        >
          <path stroke-linecap="round" stroke-linejoin="round" d="M19 9l-7 7-7-7" />
        </svg>
      </div>
      @if (error()) {
        <p class="text-xs text-rose-500 text-right">{{ error() }}</p>
      }
    </div>
  `,
})
export class SelectComponent implements ControlValueAccessor {
  readonly label = input<string>();
  readonly placeholder = input<string>();
  readonly options = input<SelectOption[]>([]);
  readonly required = input(false);
  readonly disabled = input(false);
  readonly error = input<string>();

  protected value: string | number = '';
  protected onChange: (value: string | number) => void = () => {};
  protected onTouched: () => void = () => {};

  protected onSelect(event: Event) {
    this.value = (event.target as HTMLSelectElement).value;
    this.onChange(this.value);
  }

  writeValue(value: string | number): void {
    this.value = value ?? '';
  }

  registerOnChange(fn: (value: string | number) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    // handled by disabled() input
  }
}
