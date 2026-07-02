import { Component, inject, signal, computed, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, switchMap, takeUntil } from 'rxjs';
import { CategoryService } from '../../core/services/category.service';
import { WorkerService } from '../../core/services/worker.service';
import { CategoryTreeResponse } from '../../core/models/category.models';
import { WorkerSummaryResponse } from '../../core/models/worker.models';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div
      class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 bg-slate-50 dark:bg-slate-900 transition-colors duration-300 min-h-screen"
    >
      <!-- Page Title -->
      <div class="text-right space-y-2 mb-10">
        <h1 class="text-3xl font-black text-slate-850 dark:text-white">ابحث عن صنايعي</h1>
        <p class="text-sm text-slate-500">تصفح وقارن بين مئات الصنايعية المحترفين القريبين منك.</p>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-4 gap-8">
        <!-- SIDEBAR FILTERS (Col 1) -->
        <div class="lg:col-span-1 space-y-6">
          <div
            class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-5 text-right"
          >
            <div
              class="flex justify-between items-center pb-4 border-b border-slate-100 dark:border-slate-905"
            >
              <button
                (click)="resetFilters()"
                class="text-xs font-bold text-red-500 hover:text-red-650 cursor-pointer"
              >
                إعادة تعيين
              </button>
              <h3 class="font-black text-slate-800 dark:text-slate-150 text-base">تصفية النتائج</h3>
            </div>

            <!-- Service Filter -->
            <div class="space-y-1.5">
              <label class="text-xs font-bold text-slate-550 dark:text-slate-400"
                >نوع الخدمة / المهنة</label
              >
              <select
                [(ngModel)]="filterService"
                (change)="search()"
                class="w-full px-3 py-2.5 bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-slate-800 dark:text-white rounded-xl outline-none focus:border-primary text-right"
              >
                <option value="">كل الخدمات</option>
                  @for (cat of categories(); track cat.id) {
                    <option [value]="cat.id">{{ cat.name }}</option>
                  }
              </select>
            </div>

            <!-- Governorate Filter -->
            <div class="space-y-1.5">
              <label class="text-xs font-bold text-slate-550 dark:text-slate-400">المحافظة</label>
              <select
                [(ngModel)]="filterGov"
                (change)="onGovChange()"
                class="w-full px-3 py-2.5 bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-slate-800 dark:text-white rounded-xl outline-none focus:border-primary text-right"
              >
                <option value="">كل المحافظات</option>
                  @for (gov of governorates; track gov.id) {
                  <option [value]="gov.name">{{ gov.name }}</option>
                }
              </select>
            </div>

            <!-- Area Filter -->
            <div class="space-y-1.5">
              <label class="text-xs font-bold text-slate-550 dark:text-slate-400">المنطقة</label>
              <select
                [(ngModel)]="filterArea"
                [disabled]="!filterGov"
                (change)="search()"
                class="w-full px-3 py-2.5 bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-slate-800 dark:text-white rounded-xl outline-none focus:border-primary text-right disabled:opacity-50"
              >
                <option value="">كل المناطق</option>
                @for (area of availableAreas(); track area) {
                  <option [value]="area">{{ area }}</option>
                }
              </select>
            </div>

            <!-- Price Slider Filter -->
            <div class="space-y-1.5">
              <div class="flex justify-between items-center">
                <span class="text-xs font-bold text-slate-450 dark:text-slate-400"
                  >{{ filterPrice }} جنيه</span
                >
                <label class="text-xs font-bold text-slate-550 dark:text-slate-400"
                  >الحد الأقصى لسعر الكشف</label
                >
              </div>
              <input
                type="range"
                [(ngModel)]="filterPrice"
                min="50"
                max="300"
                step="10"
                (change)="search()"
                class="w-full h-1.5 bg-slate-200 dark:bg-slate-800 rounded-lg appearance-none cursor-pointer accent-primary"
              />
            </div>

            <!-- Rating Select Filter -->
            <div class="space-y-1.5">
              <label class="text-xs font-bold text-slate-550 dark:text-slate-400"
                >الحد الأدنى للتقييم</label
              >
              <select
                [(ngModel)]="filterRating"
                (change)="search()"
                class="w-full px-3 py-2.5 bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-slate-800 dark:text-white rounded-xl outline-none focus:border-primary text-right font-semibold"
              >
                <option [value]="0">أي تقييم</option>
                <option [value]="4.5">⭐ 4.5 فأكثر</option>
                <option [value]="4.0">⭐ 4.0 فأكثر</option>
                <option [value]="3.5">⭐ 3.5 فأكثر</option>
              </select>
            </div>
          </div>
        </div>

        <!-- WORKER RESULTS GRID (Col 2-4) -->
        <div class="lg:col-span-3 space-y-6">
          <!-- Top Results Bar -->
          <div
            class="flex flex-col sm:flex-row items-center justify-between bg-white dark:bg-slate-950 p-4 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm gap-4"
          >
            <!-- Count -->
            <div class="text-right text-sm text-slate-500 order-2 sm:order-1">
              عثرنا على
              <span class="font-extrabold text-primary">{{ totalCount() }}</span> صنايعي
              مطابقين لبحثك.
            </div>

            <!-- Sort dropdown -->
            <div class="flex items-center gap-2 order-1 sm:order-2">
              <select
                [(ngModel)]="sortOption"
                (change)="search()"
                class="px-3 py-1.5 bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-slate-800 dark:text-white text-xs font-bold rounded-lg outline-none pr-1 focus:border-primary cursor-pointer text-right"
              >
                <option value="rating">الأعلى تقييماً</option>
                <option value="price-low">الأقل كشفاً</option>
                <option value="experience">الأكثر خبرة</option>
              </select>
              <span class="text-xs font-bold text-slate-550 dark:text-slate-400">ترتيب حسب:</span>
            </div>
          </div>

          <!-- SKELETON LOADER STATE -->
          @if (isLoading()) {
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
              @for (item of [1, 2, 3, 4, 5, 6]; track item) {
                <div
                  class="flex flex-col bg-white dark:bg-slate-950 rounded-2xl border border-slate-100 dark:border-slate-850 p-5 space-y-4"
                >
                  <div class="flex flex-col items-center space-y-3">
                    <div class="w-20 h-20 rounded-full skeleton"></div>
                    <div class="w-32 h-4 rounded skeleton"></div>
                    <div class="w-20 h-3.5 rounded skeleton"></div>
                    <div class="w-16 h-3 rounded skeleton"></div>
                  </div>
                  <div class="border-t border-slate-50 dark:border-slate-900/50 pt-4 space-y-3.5">
                    <div class="flex justify-between">
                      <div class="w-12 h-3 rounded skeleton"></div>
                      <div class="w-20 h-3 rounded skeleton"></div>
                    </div>
                    <div class="flex justify-between">
                      <div class="w-12 h-3 rounded skeleton"></div>
                      <div class="w-14 h-3 rounded skeleton"></div>
                    </div>
                  </div>
                  <div class="w-full h-10 rounded-xl skeleton mt-4"></div>
                </div>
              }
            </div>
          } @else {
            <!-- EMPTY STATE -->
            @if (workers().length === 0 && !isLoading()) {
              <div
                class="flex flex-col items-center justify-center p-12 bg-white dark:bg-slate-950 rounded-3xl border border-slate-100 dark:border-slate-850 text-center space-y-6"
              >
                <div
                  class="w-20 h-20 rounded-full bg-slate-100 dark:bg-slate-900 flex items-center justify-center text-4xl"
                >
                  🔍
                </div>
                <h3 class="text-lg font-black text-slate-800 dark:text-slate-200">
                  لا يوجد صنايعية مطابقين لبحثك
                </h3>
                <p class="text-sm text-slate-400 max-w-sm leading-relaxed">
                  جرب تقلل شروط التصفية أو تغير اسم المنطقة أو سعر الكشف للحصول على نتائج أكثر.
                </p>
                <button
                  (click)="resetFilters()"
                  class="px-6 py-2.5 bg-primary text-white font-bold text-sm rounded-xl shadow-md cursor-pointer hover:bg-primary-hover transition-colors"
                >
                  إعادة تعيين فلاتر التصفية
                </button>
              </div>
            } @else {
              <!-- GRID RESULTS -->
              <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 animate-slide-up">
                @for (worker of workers(); track worker.id) {
                  <div
                    class="flex flex-col bg-white dark:bg-slate-950 rounded-2xl border border-slate-100 dark:border-slate-850 hover:shadow-xl hover:translate-y-[-2px] transition-all duration-300 text-right overflow-hidden relative"
                  >
                    <div class="h-16 bg-gradient-to-l from-primary/10 to-accent/10"></div>
                    <div
                      class="flex flex-col items-center -mt-10 px-5 pb-4 border-b border-slate-50 dark:border-slate-850"
                    >
                      <img
                        [src]="worker.photo || 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100'"
                        class="w-18 h-18 rounded-2xl object-cover border-4 border-white dark:border-slate-950 shadow-md"
                        [alt]="worker.firstName + ' ' + worker.lastName"
                      />
                      <h3
                        class="text-base font-black text-slate-850 dark:text-white mt-2 hover:text-primary transition-colors"
                      >
                        <a [routerLink]="['/profile', worker.id]">{{ worker.firstName }} {{ worker.lastName }}</a>
                      </h3>
                      <span
                        class="text-[10px] font-extrabold text-primary px-2.5 py-0.5 bg-primary/10 rounded-full mt-1"
                        >{{ worker.categories[0] || 'صنايعي' }}</span
                      >
                      <div class="flex items-center gap-1 mt-2.5">
                        <span class="text-amber-500 text-xs">★</span>
                        <span class="text-xs font-bold text-slate-700 dark:text-slate-200">{{
                          worker.averageRating
                        }}</span>
                        <span class="text-[10px] text-slate-450 dark:text-slate-400"
                          >({{ worker.completedJobs }} طلب)</span
                        >
                      </div>
                    </div>
                    <div class="p-5 flex-grow space-y-3.5 text-xs">
                      <div class="flex items-center justify-between text-slate-500">
                        <span class="font-bold">يغطي:</span>
                        <span class="text-slate-700 dark:text-slate-350"
                          >{{ worker.serviceAreas || 'مصر' }}</span
                        >
                      </div>
                      <div class="flex items-center justify-between text-slate-500">
                        <span class="font-bold">سعر الكشف:</span>
                        <span class="text-primary font-black text-sm">{{ worker.startingPrice }} جنيه</span>
                      </div>
                      <div class="flex items-center justify-between text-slate-500">
                        <span class="font-bold">سنوات الخبرة:</span>
                        <span class="text-slate-700 dark:text-slate-350"
                          >{{ worker.yearsOfExperience }} سنة</span
                        >
                      </div>
                      <p
                        class="text-[11px] text-slate-450 dark:text-slate-400 line-clamp-2 mt-2 leading-relaxed h-8"
                      >
                        {{ worker.biography }}
                      </p>
                    </div>
                    <div class="p-5 pt-0">
                      <a
                        [routerLink]="['/profile', worker.id]"
                        class="w-full py-2.5 bg-slate-50 hover:bg-primary dark:bg-slate-900/60 dark:hover:bg-primary text-slate-700 hover:text-white dark:text-slate-350 dark:hover:text-white border border-slate-100 hover:border-primary dark:border-slate-800 rounded-xl font-bold text-xs flex items-center justify-center transition-all cursor-pointer"
                      >
                        عرض الملف الكامل
                      </a>
                    </div>
                  </div>
                }
              </div>

              @if (totalPages() > 1) {
                <div class="flex items-center justify-center gap-2 pt-6">
                  <button
                    (click)="goToPage(currentPage - 1)"
                    [disabled]="currentPage <= 1"
                    class="px-3.5 py-1.5 rounded-lg border border-slate-200 dark:border-slate-850 text-xs font-bold text-slate-500 hover:bg-slate-50 disabled:opacity-40 cursor-pointer"
                  >
                    &rarr; السابق
                  </button>
                  @for (p of pageNumbers(); track p) {
                    <button
                      (click)="goToPage(p)"
                      class="px-3.5 py-1.5 rounded-lg text-xs font-bold cursor-pointer"
                      [ngClass]="{
                        'bg-primary text-white': p === currentPage,
                        'border border-slate-200 dark:border-slate-850 text-slate-500 hover:bg-slate-50': p !== currentPage,
                      }"
                    >
                      {{ p }}
                    </button>
                  }
                  <button
                    (click)="goToPage(currentPage + 1)"
                    [disabled]="currentPage >= totalPages()"
                    class="px-3.5 py-1.5 rounded-lg border border-slate-200 dark:border-slate-850 text-xs font-bold text-slate-500 hover:bg-slate-50 disabled:opacity-40 cursor-pointer"
                  >
                    التالي &larr;
                  </button>
                </div>
              }
            }
          }
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      @keyframes slide-up {
        from {
          transform: translateY(15px);
          opacity: 0;
        }
        to {
          transform: translateY(0);
          opacity: 1;
        }
      }
      .animate-slide-up {
        animation: slide-up 0.3s cubic-bezier(0.16, 1, 0.3, 1) forwards;
      }
    `,
  ],
})
export default class SearchComponent implements OnInit, OnDestroy {
  private categoryService = inject(CategoryService);
  private workerService = inject(WorkerService);
  private route = inject(ActivatedRoute);

  categories = signal<CategoryTreeResponse[]>([]);
  workers = signal<WorkerSummaryResponse[]>([]);
  isLoading = signal(false);
  totalCount = signal(0);
  currentPage = 1;
  pageSize = 9;

  filterService = '';
  filterGov = '';
  filterArea = '';
  filterPrice = 300;
  filterRating = 0;
  sortOption = 'rating';

  private searchSubject = new Subject<void>();
  private destroy$ = new Subject<void>();

  governorates = [
    { id: 'cairo', name: 'القاهرة', areas: ['المعادي', 'التجمع الخامس', 'مصر الجديدة', 'مدينة نصر', 'شبرا', 'حلوان', 'وسط البلد'] },
    { id: 'giza', name: 'الجيزة', areas: ['المهندسين', 'الدقي', 'الهرم', 'أكتوبر', 'الشيخ زايد', 'فيصل', 'العجوزة'] },
    { id: 'alexandria', name: 'الإسكندرية', areas: ['سموحة', 'سيدي بشر', 'المنتزة', 'محرم بك', 'لوران', 'العصافرة', 'الرمل'] },
  ];

  availableAreas = computed(() => {
    const gov = this.governorates.find((g) => g.name === this.filterGov);
    return gov ? gov.areas : [];
  });

  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));

  pageNumbers = computed(() => {
    const total = this.totalPages();
    const pages: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(total, this.currentPage + 2);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  });

  ngOnInit() {
    this.categoryService.getTree().pipe(takeUntil(this.destroy$)).subscribe({
      next: (cats) => this.categories.set(cats),
    });

    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      this.filterService = params['service'] || '';
      this.filterGov = params['gov'] || '';
      this.filterArea = params['area'] || '';
      this.currentPage = 1;
      this.search();
    });

    this.searchSubject.pipe(
      debounceTime(300),
      takeUntil(this.destroy$),
    ).subscribe(() => this.executeSearch());
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onGovChange() {
    this.filterArea = '';
    this.search();
  }

  search() {
    this.searchSubject.next();
  }

  private executeSearch() {
    this.isLoading.set(true);
    const request = {
      searchTerm: undefined as string | undefined,
      categoryId: this.filterService ? Number(this.filterService) : undefined,
      minRating: this.filterRating > 0 ? this.filterRating : undefined,
      maxPrice: this.filterPrice < 300 ? this.filterPrice : undefined,
      sortBy: this.sortOption === 'rating' ? 'rating' : this.sortOption === 'price-low' ? 'price' : 'experience',
      page: this.currentPage,
      pageSize: this.pageSize,
    };
    // Remove undefined keys
    const clean = Object.fromEntries(Object.entries(request).filter(([_, v]) => v !== undefined));
    this.workerService.search(clean as any).subscribe({
      next: (res) => {
        this.workers.set(res.items);
        this.totalCount.set(res.totalCount);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  goToPage(page: number) {
    if (page < 1 || page > this.totalPages()) return;
    this.currentPage = page;
    this.search();
  }

  resetFilters() {
    this.filterService = '';
    this.filterGov = '';
    this.filterArea = '';
    this.filterPrice = 300;
    this.filterRating = 0;
    this.sortOption = 'rating';
    this.currentPage = 1;
    this.search();
  }
}
