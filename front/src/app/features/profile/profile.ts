import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { WorkerService } from '../../core/services/worker.service';
import { ReviewService } from '../../core/services/review.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { WorkerProfileResponse } from '../../core/models/worker.models';
import { ReviewResponse } from '../../core/models/review.models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, RouterModule, DatePipe],
  template: `
    <div class="bg-slate-50 dark:bg-slate-900 transition-colors duration-300 pb-20 min-h-screen">
      @if (worker(); as w) {
        <!-- COVER IMAGE -->
        <div
          class="h-48 sm:h-64 md:h-72 w-full bg-cover bg-center relative"
          style="background-image: url('https://images.unsplash.com/photo-1621905251189-08b45d6a269e?w=1000');"
        >
          <div class="absolute inset-0 bg-slate-950/40"></div>
        </div>

        <!-- PROFILE HEADER -->
        <div
          class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 -mt-20 sm:-mt-24 relative z-10 text-right"
        >
          <div
            class="bg-white dark:bg-slate-950 p-6 rounded-3xl border border-slate-100 dark:border-slate-850 shadow-lg flex flex-col md:flex-row items-center md:items-start justify-between gap-6"
          >
            <!-- Avatar & Details -->
            <div
              class="flex flex-col md:flex-row items-center md:items-start gap-6 text-center md:text-right w-full"
            >
                <img
                  [src]="w.photo || 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100'"
                  class="w-32 h-32 rounded-2xl object-cover border-4 border-white dark:border-slate-900 shadow-md"
                  [alt]="w.firstName + ' ' + w.lastName"
                />

                <div class="space-y-3.5 mt-2 flex-grow">
                  <div class="flex flex-wrap items-center gap-2 justify-center md:justify-start">
                    <h1 class="text-2xl sm:text-3xl font-black text-slate-850 dark:text-white">
                      {{ w.firstName }} {{ w.lastName }}
                    </h1>
                  <span
                    class="px-2 py-0.5 bg-emerald-100 text-emerald-800 dark:bg-emerald-950/60 dark:text-emerald-350 text-[10px] font-bold rounded flex items-center gap-1"
                  >
                    <span>✓</span> هويّة موثقة
                  </span>
                </div>

                <div
                  class="flex flex-wrap items-center gap-3.5 justify-center md:justify-start text-sm"
                >
                    <span
                      class="text-primary font-bold px-3 py-1 bg-primary/10 rounded-full text-xs"
                      >{{ w.skills || 'صنايعي' }}</span
                    >
                    <span class="text-slate-400 dark:text-slate-500 font-bold"
                      >خبرة {{ w.yearsOfExperience }} سنة</span
                    >
                    <span class="text-slate-400 dark:text-slate-500 font-bold"
                      >📍 {{ w.serviceAreas || 'مصر' }}</span
                    >
                </div>

                <!-- Rating -->
                <div class="flex items-center gap-1.5 justify-center md:justify-start text-sm">
                  <span class="text-amber-500 text-base">★</span>
                  <span class="font-extrabold text-slate-800 dark:text-slate-100">{{
                    w.averageRating
                  }}</span>
                  <span class="text-slate-400">({{ reviews().length }} تقييم)</span>
                </div>
              </div>
            </div>

            <!-- Header Quick Actions (Tablet/Desktop) -->
            <div class="flex items-center gap-3 w-full md:w-auto md:flex-col justify-center">
              <div
                class="text-center md:text-left bg-slate-50 dark:bg-slate-900 px-5 py-3 rounded-xl border border-slate-150 dark:border-slate-850 w-full md:min-w-[180px]"
              >
                <span class="text-xs text-slate-450 dark:text-slate-500 font-bold block"
                  >سعر الكشف</span
                >
                  <span class="text-xl font-black text-primary">{{ w.startingPrice }} جنيه</span>
              </div>
            </div>
          </div>
        </div>

        <!-- MAIN LAYOUT -->
        <div
          class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 mt-8 grid grid-cols-1 lg:grid-cols-3 gap-8"
        >
          <!-- LEFT SIDEBAR: ACTION CARD & SERVICE AREAS (Col 1) -->
          <div class="lg:col-span-1 space-y-6 order-2 lg:order-1">
            <!-- Booking Card -->
            <div
              class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm text-right space-y-5"
            >
              <h3 class="font-black text-slate-800 dark:text-white text-base">احجز الآن</h3>
              <p class="text-xs text-slate-500 leading-relaxed">
                تقدر تحجز الصنايعي للمعاينة أو الصيانة فوراً. سعر الكشف محدد وهيتم الاتفاق على بقية
                تكلفة المصنعية بعد المعاينة.
              </p>

              <div class="space-y-3 pt-2">
                <button
                  (click)="onBookNow(w.id)"
                  class="w-full py-3.5 bg-primary hover:bg-primary-hover text-white rounded-xl font-bold shadow-md hover:shadow-lg transition-all text-sm cursor-pointer"
                >
                  احجز ميعاد
                </button>
                <button
                  (click)="onMessageNow(w)"
                  class="w-full py-3.5 bg-white dark:bg-slate-900 hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-300 rounded-xl font-bold border border-slate-200 dark:border-slate-800 transition-all text-sm flex items-center justify-center gap-2 cursor-pointer"
                >
                  <svg
                    class="w-4 h-4"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                    stroke-width="2"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"
                    />
                  </svg>
                  <span>راسل الصنايعي</span>
                </button>
              </div>
            </div>

            <!-- Covered Areas Card -->
            <div
              class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm text-right space-y-4"
            >
              <h3
                class="font-black text-slate-800 dark:text-white text-sm border-r-2 border-primary pr-2"
              >
                مناطق التغطية
              </h3>
              <div class="flex flex-wrap gap-2 justify-start pt-1">
                @for (area of availableAreasList(); track area) {
                  <span
                    class="px-3 py-1.5 bg-slate-50 dark:bg-slate-900 border border-slate-150 dark:border-slate-800 text-xs font-bold text-slate-600 dark:text-slate-350 rounded-lg"
                  >
                    {{ area }}
                  </span>
                }
              </div>
            </div>
          </div>

          <!-- RIGHT CONTENT: ABOUT, PORTFOLIO & REVIEWS (Col 2-3) -->
          <div class="lg:col-span-2 space-y-8 order-1 lg:order-2 text-right">
            <!-- About Section -->
            <div
              class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-4"
            >
              <h2
                class="text-lg font-black text-slate-850 dark:text-white border-r-2 border-primary pr-2"
              >
                من أنا؟
              </h2>
              <p class="text-sm text-slate-600 dark:text-slate-400 leading-relaxed">
                {{ w.biography || 'لم يضف الصنايعي سيرته الذاتية بعد.' }}
              </p>
            </div>

            <!-- Portfolio Section -->
            <div
              class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-4"
            >
              <h2
                class="text-lg font-black text-slate-850 dark:text-white border-r-2 border-primary pr-2"
              >
                معرض أعمالي السابقة
              </h2>

              @if (w.portfolio.length === 0) {
                <p class="text-xs text-slate-400">لا توجد صور أعمال مرفوعة حالياً.</p>
              } @else {
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-2">
                  @for (item of w.portfolio; track item.id) {
                    <div
                      class="rounded-xl overflow-hidden shadow-sm hover:shadow-md transition-shadow h-48 bg-slate-100 dark:bg-slate-900"
                    >
                      <img
                        [src]="item.mediaUrl"
                        class="w-full h-full object-cover transform hover:scale-105 transition-transform duration-500"
                        [alt]="item.title || 'صورة من أعمال الصنايعي'"
                      />
                    </div>
                  }
                </div>
              }
            </div>

            <!-- Reviews Section -->
            <div
              class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-6"
            >
              <h2
                class="text-lg font-black text-slate-850 dark:text-white border-r-2 border-primary pr-2"
              >
                تقييمات العملاء
              </h2>

              <!-- Star rating stats -->
              <div
                class="grid grid-cols-1 sm:grid-cols-3 gap-6 bg-slate-50 dark:bg-slate-900 p-5 rounded-2xl border border-slate-100 dark:border-slate-850 items-center"
              >
                <!-- Large score -->
                <div
                  class="text-center space-y-1 sm:border-l sm:border-slate-200 dark:sm:border-slate-800"
                >
                  <span class="text-4xl font-black text-slate-850 dark:text-white block">{{
                    w.averageRating
                  }}</span>
                  <div class="flex items-center justify-center gap-0.5 text-amber-500 text-base">
                    ★★★★★
                  </div>
                  <span class="text-[11px] text-slate-400 block mt-1"
                    >بناءً على {{ reviews().length }} تقييم</span
                  >
                </div>

                <!-- Rating breakdown lines -->
                <div class="sm:col-span-2 space-y-2 text-xs">
                  <div class="flex items-center gap-3">
                    <span class="w-10 text-slate-400 font-bold">٥ نجوم</span>
                    <div
                      class="flex-grow h-2 bg-slate-200 dark:bg-slate-800 rounded-full overflow-hidden"
                    >
                      <div class="h-full bg-amber-500 rounded-full" style="width: 85%"></div>
                    </div>
                    <span class="w-8 text-slate-400 text-left">٨٥٪</span>
                  </div>

                  <div class="flex items-center gap-3">
                    <span class="w-10 text-slate-400 font-bold">٤ نجوم</span>
                    <div
                      class="flex-grow h-2 bg-slate-200 dark:bg-slate-800 rounded-full overflow-hidden"
                    >
                      <div class="h-full bg-amber-500 rounded-full" style="width: 10%"></div>
                    </div>
                    <span class="w-8 text-slate-400 text-left">١٠٪</span>
                  </div>

                  <div class="flex items-center gap-3">
                    <span class="w-10 text-slate-400 font-bold">٣ نجوم</span>
                    <div
                      class="flex-grow h-2 bg-slate-200 dark:bg-slate-800 rounded-full overflow-hidden"
                    >
                      <div class="h-full bg-amber-500 rounded-full" style="width: 5%"></div>
                    </div>
                    <span class="w-8 text-slate-400 text-left">٥٪</span>
                  </div>
                </div>
              </div>

              <!-- Reviews List -->
              <div class="space-y-4 pt-2">
                @for (rev of reviews(); track rev.id) {
                  <div
                    class="p-4 bg-slate-50/50 dark:bg-slate-900/30 rounded-xl border border-slate-100/50 dark:border-slate-850 space-y-3"
                  >
                    <div class="flex items-center justify-between">
                      <div class="flex items-center gap-3">
                        <img [src]="rev.customerPhoto || 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100'" class="w-9 h-9 rounded-full object-cover" />
                        <div class="text-right">
                          <h4 class="text-xs font-black text-slate-800 dark:text-white">
                            {{ rev.customerName }}
                          </h4>
                          <span class="text-[9px] text-slate-400">{{ rev.createdAt | date:'d MMM yyyy' }}</span>
                        </div>
                      </div>
                      <div class="flex gap-0.5 text-amber-500 text-xs">
                        @for (star of [1, 2, 3, 4, 5]; track star) {
                          <span class="text-amber-500">{{ star <= rev.rating ? '★' : '☆' }}</span>
                        }
                      </div>
                    </div>
                    <p
                      class="text-sm text-slate-650 dark:text-slate-350 leading-relaxed font-semibold"
                    >
                      "{{ rev.comment }}"
                    </p>
                  </div>
                } @empty {
                  <p class="text-xs text-slate-400 text-center py-4">
                    لا توجد مراجعات مكتوبة بعد لهذا الصنايعي.
                  </p>
                }
              </div>
            </div>
          </div>
        </div>
      } @else {
        <!-- WORKER NOT FOUND EMPTY STATE -->
        <div class="max-w-md mx-auto px-4 py-24 text-center space-y-6">
          <div
            class="w-20 h-20 bg-slate-100 dark:bg-slate-950 rounded-full flex items-center justify-center text-4xl mx-auto shadow-md"
          >
            ❌
          </div>
          <h2 class="text-xl font-black text-slate-800 dark:text-slate-200">الصنايعي غير موجود</h2>
          <p class="text-sm text-slate-450 dark:text-slate-400">
            يبدو أن الملف الشخصي الذي تحاول عرضه غير متاح أو تم نقله.
          </p>
          <a
            routerLink="/search"
            class="inline-block px-6 py-3 bg-primary text-white font-bold rounded-xl shadow-md cursor-pointer hover:bg-primary-hover transition-colors"
            >ابحث عن صنايعية آخرين</a
          >
        </div>
      }
    </div>
  `,
})
export default class ProfileComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private workerService = inject(WorkerService);
  private reviewService = inject(ReviewService);
  private authService = inject(AuthService);
  private toast = inject(ToastService);

  worker = signal<WorkerProfileResponse | null>(null);
  reviews = signal<ReviewResponse[]>([]);
  loading = signal(true);

  availableAreasList = computed(() => {
    const areas = this.worker()?.serviceAreas;
    return areas ? areas.split(',').map((a) => a.trim()) : [];
  });

  ngOnInit() {
    this.route.paramMap.subscribe((params) => {
      const id = params.get('id');
      if (id) {
        this.loading.set(true);
        const workerId = Number(id);
        this.workerService.getById(workerId).subscribe({
          next: (w) => {
            this.worker.set(w);
            this.loading.set(false);
          },
          error: () => {
            this.worker.set(null);
            this.loading.set(false);
          },
        });
        this.reviewService.getWorkerReviews(workerId, 1, 10).subscribe({
          next: (res) => this.reviews.set(res.items),
        });
      }
    });
  }

  onBookNow(workerId: number) {
    const user = this.authService.currentUser();
    if (!user) {
      this.toast.show('من فضلك قم بتسجيل الدخول أولاً لإتمام الحجز.', 'warning');
      this.router.navigate(['/login']);
      return;
    }
    if (user.role === 'worker') {
      this.toast.show('الحسابات من نوع صنايعي لا يمكنها القيام بحجز خدمات.', 'error');
      return;
    }
    this.router.navigate(['/booking', workerId]);
  }

  onMessageNow(w: WorkerProfileResponse) {
    const user = this.authService.currentUser();
    if (!user) {
      this.toast.show('من فضلك قم بتسجيل الدخول أولاً لمراسلة الصنايعي.', 'warning');
      this.router.navigate(['/login']);
      return;
    }
    if (String(user.id) === String(w.userId)) {
      this.toast.show('لا يمكنك مراسلة نفسك!', 'error');
      return;
    }
    this.router.navigate(['/chat'], { queryParams: { with: w.userId } });
  }
}
