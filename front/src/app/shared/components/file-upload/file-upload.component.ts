import { Component, input, output, HostListener, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex flex-col gap-2">
      @if (label()) {
        <label class="text-sm font-bold text-slate-700 dark:text-slate-300 text-right">
          {{ label() }}
          @if (required()) { <span class="text-rose-500">*</span> }
        </label>
      }
      <div
        (click)="fileInput.click()"
        (dragover)="onDragOver($event)"
        (dragleave)="onDragLeave($event)"
        (drop)="onDrop($event)"
        class="relative border-2 border-dashed rounded-xl p-8 text-center transition-all cursor-pointer"
        [ngClass]="{
          'border-primary bg-primary/5': isDragging(),
          'border-slate-200 dark:border-slate-700 hover:border-primary dark:hover:border-primary hover:bg-slate-50 dark:hover:bg-slate-900/50': !isDragging() && !error(),
          'border-rose-300 dark:border-rose-700 bg-rose-50/50 dark:bg-rose-950/20': !!error(),
        }"
      >
        <input
          #fileInput
          type="file"
          [accept]="accept()"
          [multiple]="multiple()"
          (change)="onFileSelected($event)"
          class="hidden"
        />
        <div class="flex flex-col items-center gap-3">
          <svg class="w-10 h-10 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
          </svg>
          <div class="text-sm text-slate-500 dark:text-slate-400">
            <span class="text-primary font-bold">اضغط لاختيار الملفات</span>
            <span> أو اسحب وأفلت</span>
          </div>
          @if (hint()) {
            <p class="text-xs text-slate-400">{{ hint() }}</p>
          }
        </div>
      </div>
      @if (error()) {
        <p class="text-xs text-rose-500 text-right">{{ error() }}</p>
      }
      <!-- Preview -->
      @if (preview().length) {
        <div class="flex flex-wrap gap-3 mt-2" [ngClass]="{'flex-row-reverse': true}">
          @for (file of preview(); track file.name) {
            <div class="relative group">
              @if (file.type.startsWith('image/')) {
                <img
                  [src]="file.url"
                  [alt]="file.name"
                  class="w-20 h-20 rounded-xl object-cover border border-slate-200 dark:border-slate-700"
                />
              } @else {
                <div class="w-20 h-20 rounded-xl bg-slate-100 dark:bg-slate-800 flex items-center justify-center border border-slate-200 dark:border-slate-700">
                  <svg class="w-8 h-8 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z" />
                  </svg>
                </div>
              }
              <button
                (click)="removeFile.emit(file.name)"
                class="absolute -top-2 -right-2 w-5 h-5 bg-rose-500 text-white rounded-full flex items-center justify-center text-xs opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer"
              >
                ×
              </button>
            </div>
          }
        </div>
      }
    </div>
  `,
})
export class FileUploadComponent {
  readonly label = input<string>();
  readonly accept = input<string>('image/*');
  readonly multiple = input(false);
  readonly required = input(false);
  readonly error = input<string>();
  readonly hint = input<string>();
  readonly filesChange = output<File[]>();
  readonly removeFile = output<string>();

  protected isDragging = signal(false);
  protected preview = signal<{ name: string; type: string; url: string }[]>([]);

  @HostListener('document:Dragover', ['$event'])
  protected onDragOver(event: Event) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(true);
  }

  @HostListener('document:Dragleave', ['$event'])
  protected onDragLeave(event: Event) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
  }

  protected onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
    const files = event.dataTransfer?.files;
    if (files?.length) {
      this.handleFiles(Array.from(files));
    }
  }

  protected onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.handleFiles(Array.from(input.files));
    }
    input.value = '';
  }

  private handleFiles(files: File[]) {
    const previews = files.map((f) => ({
      name: f.name,
      type: f.type,
      url: URL.createObjectURL(f),
    }));
    if (this.multiple()) {
      this.preview.update((prev) => [...prev, ...previews]);
    } else {
      this.preview.set(previews);
    }
    this.filesChange.emit(files);
  }
}
