import { Component, input, output, computed, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';

@Component({
  selector: 'app-input',
  standalone: true,
  imports: [CommonModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputComponent),
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
      <input
        [type]="type()"
        [placeholder]="placeholder()"
        [value]="value"
        [disabled]="disabled()"
        (input)="onInput($event)"
        (blur)="onBlur()"
        class="w-full px-4 py-3 bg-slate-50 dark:bg-slate-900 border rounded-xl outline-none transition-all text-right font-bold text-slate-800 dark:text-white placeholder:text-slate-400 dark:placeholder:text-slate-500 disabled:opacity-50 disabled:cursor-not-allowed"
        [ngClass]="{
          'border-slate-200 dark:border-slate-700 focus:border-primary dark:focus:border-primary': !error(),
          'border-rose-300 dark:border-rose-700 focus:border-rose-500': !!error(),
        }"
      />
      @if (error()) {
        <p class="text-xs text-rose-500 text-right">{{ error() }}</p>
      }
      @if (hint() && !error()) {
        <p class="text-xs text-slate-400 text-right">{{ hint() }}</p>
      }
    </div>
  `,
})
export class InputComponent implements ControlValueAccessor {
  readonly label = input<string>();
  readonly placeholder = input<string>();
  readonly type = input<'text' | 'email' | 'password' | 'number' | 'tel' | 'url'>('text');
  readonly required = input(false);
  readonly disabled = input(false);
  readonly error = input<string>();
  readonly hint = input<string>();

  protected value = '';
  protected onChange: (value: string) => void = () => {};
  protected onTouched: () => void = () => {};

  protected onInput(event: Event) {
    this.value = (event.target as HTMLInputElement).value;
    this.onChange(this.value);
  }

  protected onBlur() {
    this.onTouched();
  }

  writeValue(value: string): void {
    this.value = value ?? '';
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    // handled by disabled() input
  }
}
