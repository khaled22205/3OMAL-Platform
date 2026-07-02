import { Component, inject, signal, computed, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BookingService } from '../../core/services/booking.service';
import { WorkerService } from '../../core/services/worker.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { BookingResponse, BookingFilterRequest, BookingStatus } from '../../core/models/booking.models';
import { WorkerProfileResponse, WorkerProfileRequest } from '../../core/models/worker.models';
import { Booking, WorkerProfile } from '../../core/models/interfaces';
import AiDashboardPageComponent from '../ai-assistant/pages/ai-dashboard-page/ai-dashboard-page.component';

type Tab = 'stats' | 'bookings-pending' | 'bookings-active' | 'portfolio' | 'settings' | 'ai';

@Component({
  selector: 'app-worker-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, AiDashboardPageComponent],
  template: `
    <div
      class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 bg-slate-50 dark:bg-slate-900 transition-colors duration-300 min-h-screen"
    >
      <div class="grid grid-cols-1 lg:grid-cols-4 gap-8">
        <!-- SIDEBAR -->
        <div class="lg:col-span-1">
          <div
            class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm text-right space-y-6"
          >
            <div
              class="flex items-center gap-3 border-b border-slate-100 dark:border-slate-850 pb-4"
            >
              <img
                [src]="workerProfile()?.avatar || currentUser()?.avatar"
                class="w-12 h-12 rounded-xl object-cover"
              />
              <div class="flex flex-col text-right">
                <span class="text-sm font-extrabold text-slate-800 dark:text-white">{{
                  workerProfile()?.name || currentUser()?.name
                }}</span>
                <span class="text-[10px] text-primary font-bold"
                  >صنايعي: {{ workerProfile()?.profession || 'مهني' }}</span
                >
              </div>
            </div>
            <div class="flex flex-col gap-1">
              <button
                (click)="activeTab.set('stats')"
                [class]="
                  activeTab() === 'stats'
                    ? 'bg-primary/10 text-primary font-black'
                    : 'text-slate-600 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-900'
                "
                class="w-full text-right px-4 py-3 rounded-xl text-xs sm:text-sm font-bold transition-all cursor-pointer"
              >
                📊 لوحة التحكم
              </button>
              <button
                (click)="activeTab.set('bookings-pending')"
                [class]="
                  activeTab() === 'bookings-pending'
                    ? 'bg-primary/10 text-primary font-black'
                    : 'text-slate-600 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-900'
                "
                class="w-full text-right px-4 py-3 rounded-xl text-xs sm:text-sm font-bold transition-all cursor-pointer flex items-center justify-between"
              >
                @if (pendingBookings().length > 0) {
                  <span
                    class="px-2 py-0.5 bg-red-500 text-white font-extrabold text-[10px] rounded-full"
                    >{{ pendingBookings().length }}</span
                  >
                } @else {
                  <span></span>
                }
                <span>📥 الطلبات الواردة</span>
              </button>
              <button
                (click)="activeTab.set('bookings-active')"
                [class]="
                  activeTab() === 'bookings-active'
                    ? 'bg-primary/10 text-primary font-black'
                    : 'text-slate-600 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-900'
                "
                class="w-full text-right px-4 py-3 rounded-xl text-xs sm:text-sm font-bold transition-all cursor-pointer"
              >
                🛠️ الطلبات النشطة
              </button>
              <button
                (click)="activeTab.set('portfolio')"
                [class]="
                  activeTab() === 'portfolio'
                    ? 'bg-primary/10 text-primary font-black'
                    : 'text-slate-600 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-900'
                "
                class="w-full text-right px-4 py-3 rounded-xl text-xs sm:text-sm font-bold transition-all cursor-pointer"
              >
                🖼️ معرض الأعمال
              </button>
              <button
                (click)="activeTab.set('settings')"
                [class]="
                  activeTab() === 'settings'
                    ? 'bg-primary/10 text-primary font-black'
                    : 'text-slate-600 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-900'
                "
                class="w-full text-right px-4 py-3 rounded-xl text-xs sm:text-sm font-bold transition-all cursor-pointer"
              >
                ⚙️ إعدادات الحساب
              </button>
              <button
                (click)="activeTab.set('ai')"
                [class]="
                  activeTab() === 'ai'
                    ? 'bg-primary/10 text-primary font-black'
                    : 'text-slate-600 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-900'
                "
                class="w-full text-right px-4 py-3 rounded-xl text-xs sm:text-sm font-bold transition-all cursor-pointer"
              >
                🤖 المساعد الذكي
              </button>
            </div>
          </div>
        </div>

        <!-- MAIN -->
        <div class="lg:col-span-3 text-right space-y-6">
          @if (loading()) {
            <div class="flex justify-center items-center py-20">
              <div
                class="w-10 h-10 border-4 border-primary/30 border-t-primary rounded-full animate-spin"
              ></div>
            </div>
          } @else if (error()) {
            <div
              class="bg-red-50 dark:bg-red-950/20 p-8 rounded-2xl border border-red-200 dark:border-red-900 text-center space-y-3"
            >
              <span class="text-4xl block">⚠️</span>
              <p class="text-sm font-bold text-red-600 dark:text-red-400">{{ error() }}</p>
              <button
                (click)="loadData()"
                class="px-5 py-2 bg-red-500 hover:bg-red-600 text-white text-xs font-bold rounded-xl cursor-pointer"
              >
                إعادة المحاولة
              </button>
            </div>
          } @else {
            @if (activeTab() === 'stats') {
              <div class="space-y-6">
                <div class="grid grid-cols-2 sm:grid-cols-4 gap-4">
                  <div
                    class="bg-white dark:bg-slate-950 p-5 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-1"
                  >
                    <span class="text-[10px] text-slate-400 font-bold block">إجمالي الأرباح</span
                    ><span class="text-xl sm:text-2xl font-black text-slate-800 dark:text-white block"
                      >{{ earningsTotal() }} جنيه</span
                    >
                  </div>
                  <div
                    class="bg-white dark:bg-slate-950 p-5 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-1"
                  >
                    <span class="text-[10px] text-slate-400 font-bold block">الطلبات المكتملة</span
                    ><span class="text-xl sm:text-2xl font-black text-emerald-600 block"
                      >{{ completedBookings().length }} طلب</span
                    >
                  </div>
                  <div
                    class="bg-white dark:bg-slate-950 p-5 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-1"
                  >
                    <span class="text-[10px] text-slate-400 font-bold block">الطلبات الحالية</span
                    ><span class="text-xl sm:text-2xl font-black text-blue-600 block"
                      >{{ activeBookings().length }} طلب</span
                    >
                  </div>
                  <div
                    class="bg-white dark:bg-slate-950 p-5 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-1"
                  >
                    <span class="text-[10px] text-slate-400 font-bold block">تقييمك العام</span
                    ><span class="text-xl sm:text-2xl font-black text-amber-500 block"
                      >★ {{ workerProfile()?.rating }}</span
                    >
                  </div>
                </div>
                <div
                  class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-4"
                >
                  <h3
                    class="font-black text-slate-800 dark:text-white text-base border-r-2 border-primary pr-2"
                  >
                    سجل الأرباح والمقبوضات
                  </h3>
                  <div class="divide-y divide-slate-50 dark:divide-slate-900 text-xs">
                    @for (job of completedBookings(); track job.id) {
                      <div class="py-3 flex justify-between items-center gap-4">
                        <span class="text-emerald-600 font-black text-sm"
                          >+ {{ job.price }} جنيه</span
                        >
                        <div class="text-right">
                          <h4 class="font-bold text-slate-800 dark:text-slate-200">
                            صيانة كشف: {{ job.customerName }}
                          </h4>
                          <span class="text-[10px] text-slate-400">{{ job.date }}</span>
                        </div>
                      </div>
                    } @empty {
                      <p class="text-xs text-slate-400 text-center py-6">
                        لم يتم تسوية أرباح لأي عمليات مكتملة.
                      </p>
                    }
                  </div>
                </div>
              </div>
            }

            @if (activeTab() === 'bookings-pending') {
              <div
                class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-6"
              >
                <div class="pb-4 border-b border-slate-100 dark:border-slate-850">
                  <h2 class="text-lg font-black text-slate-850 dark:text-white">
                    الطلبات الواردة الجديدة
                  </h2>
                </div>
                <div class="space-y-4">
                  @for (b of pendingBookings(); track b.id) {
                    <div
                      class="p-5 rounded-2xl border border-slate-150 dark:border-slate-850 bg-slate-50/20 dark:bg-slate-900/10 space-y-4"
                    >
                      <div
                        class="flex justify-between items-center border-b border-slate-50 dark:border-slate-850 pb-3"
                      >
                        <span class="text-xs text-slate-400">{{ b.createdAt }}</span>
                        <h4 class="text-xs sm:text-sm font-black text-slate-800 dark:text-white">
                          صاحب الطلب: {{ b.customerName }}
                        </h4>
                      </div>
                      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 text-xs">
                        <div class="space-y-1">
                          <span class="font-bold text-slate-400 block">تاريخ الزيارة</span
                          ><span class="text-slate-800 dark:text-slate-200 font-semibold"
                            >{{ b.date }} | {{ b.time }}</span
                          >
                        </div>
                        <div class="space-y-1">
                          <span class="font-bold text-slate-400 block">العنوان</span
                          ><span class="text-slate-700 dark:text-slate-350">{{ b.address }}</span>
                        </div>
                        <div class="space-y-1">
                          <span class="font-bold text-slate-400 block">سعر الكشف</span
                          ><span class="text-primary font-black text-sm">{{ b.price }} جنيه</span>
                        </div>
                      </div>
                      <div
                        class="text-xs bg-white dark:bg-slate-950 p-3.5 rounded-xl border border-slate-100 dark:border-slate-850"
                      >
                        <span class="font-bold text-slate-400 block mb-1">تفاصيل المشكلة:</span>
                        <p class="text-slate-700 dark:text-slate-300 leading-relaxed font-medium">
                          {{ b.description }}
                        </p>
                      </div>
                      <div
                        class="flex items-center gap-2 justify-start pt-2 border-t border-slate-50 dark:border-slate-850/50"
                      >
                        <button
                          (click)="onAcceptBooking(b.id)"
                          class="px-5 py-2 text-xs font-bold bg-accent hover:bg-accent-hover text-white rounded-xl shadow-md cursor-pointer"
                        >
                          قبول طلب الحجز
                        </button>
                        <button
                          (click)="onRejectBooking(b.id)"
                          class="px-5 py-2 text-xs font-bold text-red-650 hover:bg-red-50 dark:hover:bg-red-950/20 border border-red-200 dark:border-red-900 rounded-xl transition-all cursor-pointer"
                        >
                          رفض الطلب
                        </button>
                        <a
                          [routerLink]="['/chat']"
                          [queryParams]="{ with: b.customerId }"
                          class="px-4 py-2 text-xs font-bold text-slate-600 dark:text-slate-400 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl transition-all cursor-pointer"
                          >مراسلة العميل</a
                        >
                      </div>
                    </div>
                  } @empty {
                    <div class="text-center py-16 space-y-3">
                      <span class="text-4xl block">📥</span>
                      <h3 class="text-sm font-bold text-slate-500">لا توجد طلبات جديدة معلقة.</h3>
                    </div>
                  }
                </div>
              </div>
            }

            @if (activeTab() === 'bookings-active') {
              <div
                class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-6"
              >
                <div class="pb-4 border-b border-slate-100 dark:border-slate-850">
                  <h2 class="text-lg font-black text-slate-850 dark:text-white">
                    طلبات الصيانة النشطة
                  </h2>
                </div>
                <div class="space-y-4">
                  @for (b of activeBookings(); track b.id) {
                    <div
                      class="p-5 rounded-2xl border border-slate-150 dark:border-slate-850 bg-slate-50/20 dark:bg-slate-900/10 space-y-4"
                    >
                      <div
                        class="flex justify-between items-center border-b border-slate-50 dark:border-slate-850 pb-3"
                      >
                        <span class="text-xs text-slate-400">رقم الحجز: {{ b.id }}</span>
                        <h4 class="text-xs sm:text-sm font-black text-slate-850 dark:text-white">
                          العميل: {{ b.customerName }}
                        </h4>
                      </div>
                      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 text-xs">
                        <div class="space-y-1">
                          <span class="font-bold text-slate-400 block">تاريخ الزيارة</span
                          ><span class="text-slate-800 dark:text-slate-200 font-semibold"
                            >{{ b.date }} | {{ b.time }}</span
                          >
                        </div>
                        <div class="space-y-1">
                          <span class="font-bold text-slate-400 block">العنوان</span
                          ><span class="text-slate-700 dark:text-slate-350">{{ b.address }}</span>
                        </div>
                        <div class="space-y-1">
                          <span class="font-bold text-slate-400 block">مبلغ الكشف</span
                          ><span class="text-primary font-black text-sm">{{ b.price }} جنيه</span>
                        </div>
                      </div>
                      <div
                        class="text-xs bg-white dark:bg-slate-950 p-3 rounded-xl border border-slate-100 dark:border-slate-850"
                      >
                        <span class="font-bold text-slate-400 block mb-1">تفاصيل العطل:</span>
                        <p class="text-slate-700 dark:text-slate-300 leading-relaxed font-semibold">
                          {{ b.description }}
                        </p>
                      </div>
                      <div
                        class="flex items-center gap-2 justify-start pt-2 border-t border-slate-50 dark:border-slate-850/50"
                      >
                        <button
                          (click)="onCompleteBooking(b.id)"
                          class="px-5 py-2 text-xs font-bold bg-accent hover:bg-accent-hover text-white rounded-xl shadow-md cursor-pointer"
                        >
                          تأكيد إتمام العمل
                        </button>
                        <a
                          [routerLink]="['/chat']"
                          [queryParams]="{ with: b.customerId }"
                          class="px-4 py-2 text-xs font-bold text-slate-600 dark:text-slate-400 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl transition-all cursor-pointer"
                          >شات العميل</a
                        >
                      </div>
                    </div>
                  } @empty {
                    <div class="text-center py-16 space-y-3">
                      <span class="text-4xl block">🛠️</span>
                      <h3 class="text-sm font-bold text-slate-500">لا توجد طلبات جارية حالياً.</h3>
                    </div>
                  }
                </div>
              </div>
            }

            @if (activeTab() === 'portfolio') {
              <div
                class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-6"
              >
                <div
                  class="flex justify-between items-center pb-4 border-b border-slate-100 dark:border-slate-850"
                >
                  <button
                    (click)="onAddPortfolioSimulate()"
                    class="px-4 py-2 bg-primary hover:bg-primary-hover text-white font-bold text-xs rounded-xl shadow-md cursor-pointer"
                  >
                    + إضافة عمل جديد
                  </button>
                  <h2 class="text-lg font-black text-slate-850 dark:text-white">
                    معرض أعمالي السابقة
                  </h2>
                </div>
                <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 pt-2">
                  @for (img of workerProfile()?.portfolio; track img) {
                    <div
                      class="rounded-xl overflow-hidden h-40 bg-slate-100 dark:bg-slate-900 shadow-sm relative group border border-slate-100 dark:border-slate-850"
                    >
                      <img
                        [src]="img"
                        class="w-full h-full object-cover transform group-hover:scale-105 transition-transform duration-500"
                      />
                      <button
                        (click)="onDeletePortfolioImg(img)"
                        class="absolute top-2 left-2 bg-red-655/90 text-white rounded-lg p-1 text-xs opacity-0 group-hover:opacity-100 transition-opacity hover:bg-red-700 cursor-pointer"
                      >
                        حذف
                      </button>
                    </div>
                  } @empty {
                    <p class="text-xs text-slate-400 text-center py-8 col-span-3">
                      لم تقم بإضافة صور أعمال بعد.
                    </p>
                  }
                </div>
              </div>
            }

            @if (activeTab() === 'settings') {
              <div
                class="bg-white dark:bg-slate-950 p-6 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-sm space-y-6"
              >
                <div class="pb-4 border-b border-slate-100 dark:border-slate-850">
                  <h2 class="text-lg font-black text-slate-850 dark:text-white">
                    إعدادات الحساب والخدمة
                  </h2>
                </div>
                <form (submit)="onSaveSettings()" class="space-y-5 max-w-xl">
                  <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div class="space-y-1">
                      <label class="text-xs font-bold text-slate-500">سعر الكشف (جنيه)</label
                      ><input
                        type="number"
                        [(ngModel)]="settingsPrice"
                        name="price"
                        class="w-full px-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-900 text-xs sm:text-sm outline-none text-right font-semibold"
                      />
                    </div>
                    <div class="space-y-1">
                      <label class="text-xs font-bold text-slate-500">سنوات الخبرة</label
                      ><input
                        type="number"
                        [(ngModel)]="settingsExperience"
                        name="experience"
                        class="w-full px-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-900 text-xs sm:text-sm outline-none text-right font-semibold"
                      />
                    </div>
                  </div>
                  <div class="space-y-1">
                    <label class="text-xs font-bold text-slate-500">نبذة تعريفية</label
                    ><textarea
                      [(ngModel)]="settingsBio"
                      name="bio"
                      rows="4"
                      class="w-full px-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-900 text-xs sm:text-sm outline-none text-right resize-none font-semibold"
                    ></textarea>
                  </div>
                  <button
                    type="submit"
                    class="px-6 py-2.5 bg-primary hover:bg-primary-hover text-white rounded-xl font-bold text-xs sm:text-sm shadow-md cursor-pointer"
                  >
                    حفظ الإعدادات
                  </button>
                </form>
              </div>
            }
            @if (activeTab() === 'ai') {
              <app-ai-dashboard-page />
            }
          }
        </div>
      </div>
    </div>
  `,
})
export default class WorkerDashboard implements OnInit {
  private bookingService = inject(BookingService);
  private workerService = inject(WorkerService);
  authService = inject(AuthService);
  private toast = inject(ToastService);
  private destroyRef = inject(DestroyRef);

  activeTab = signal<Tab>('stats');
  loading = signal(true);
  error = signal<string | null>(null);

  realProfile = signal<WorkerProfileResponse | null>(null);
  allBookings = signal<BookingResponse[]>([]);

  settingsPrice = 100;
  settingsExperience = 0;
  settingsBio = '';

  currentUser = this.authService.currentUser;

  workerProfile = computed<WorkerProfile | null>(() => {
    const p = this.realProfile();
    if (!p) return null;
    return {
      id: String(p.id),
      name: `${p.firstName} ${p.lastName}`.trim() || 'صنايعي',
      email: p.email,
      phone: p.phoneNumber || '',
      avatar: p.photo || 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100',
      profession: p.skills?.split(',')[0] || 'مهني',
      professionId: '',
      governorate: p.serviceAreas || '',
      area: '',
      experience: p.yearsOfExperience,
      rating: p.averageRating,
      reviewsCount: p.completedJobs,
      bio: p.biography || '',
      price: p.startingPrice,
      portfolio: p.portfolio.map((item) => item.mediaUrl),
      reviews: [],
      availableAreas: p.serviceAreas?.split(',').map((s) => s.trim()) || [],
    };
  });

  private adaptedBookings = computed<Booking[]>(() => {
    return this.allBookings().map((b) => ({
      id: String(b.id),
      customerId: String(b.customerId),
      customerName: b.customerName,
      workerId: String(b.workerProfileId),
      workerName: b.workerName,
      workerProfession: '',
      date: this.formatDate(b.scheduledAt),
      time: this.formatTime(b.scheduledAt),
      address: b.address || '',
      description: b.notes || '',
      images: [],
      status: this.mapOldStatus(b.status),
      price: b.totalPrice,
      createdAt: b.createdAt,
    }));
  });

  pendingBookings = computed(() => this.adaptedBookings().filter((b) => b.status === 'pending'));
  activeBookings = computed(() => this.adaptedBookings().filter((b) => b.status === 'accepted'));
  completedBookings = computed(() => this.adaptedBookings().filter((b) => b.status === 'completed'));
  earningsTotal = computed(() =>
    this.completedBookings().reduce((sum, b) => sum + b.price, 0),
  );

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.loading.set(true);
    this.error.set(null);
    this.workerService
      .getMyProfile()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (profile) => {
          this.realProfile.set(profile);
          this.settingsPrice = profile.startingPrice;
          this.settingsExperience = profile.yearsOfExperience;
          this.settingsBio = profile.biography || '';
          this.loadBookings(profile.id);
        },
        error: () => {
          this.error.set('فشل تحميل بيانات الملف الشخصي');
          this.loading.set(false);
          this.toast.show('فشل تحميل بيانات الملف الشخصي', 'error');
        },
      });
  }

  private loadBookings(workerProfileId: number) {
    const filter: BookingFilterRequest = { page: 1, pageSize: 50 };
    this.bookingService
      .getWorkerBookings(workerProfileId, filter)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.allBookings.set(result.items || []);
          this.loading.set(false);
        },
        error: () => {
          this.toast.show('فشل تحميل الحجوزات', 'error');
          this.loading.set(false);
        },
      });
  }

  onAcceptBooking(id: string) {
    const numericId = parseInt(id, 10);
    if (isNaN(numericId)) return;
    this.bookingService
      .accept(numericId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toast.show('تم قبول طلب الحجز بنجاح!', 'success');
          this.refreshBookings();
        },
        error: () => this.toast.show('فشل قبول الحجز', 'error'),
      });
  }

  onRejectBooking(id: string) {
    const numericId = parseInt(id, 10);
    if (isNaN(numericId)) return;
    this.bookingService
      .reject(numericId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toast.show('تم رفض طلب الحجز.', 'info');
          this.refreshBookings();
        },
        error: () => this.toast.show('فشل رفض الحجز', 'error'),
      });
  }

  onCompleteBooking(id: string) {
    const numericId = parseInt(id, 10);
    if (isNaN(numericId)) return;
    this.bookingService
      .completeJob(numericId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toast.show('تم إتمام العمل وتحصيل الرسوم بنجاح.', 'success');
          this.refreshBookings();
        },
        error: () => this.toast.show('فشل إتمام الحجز', 'error'),
      });
  }

  onAddPortfolioSimulate() {
    const profile = this.realProfile();
    if (!profile) return;
    const url = prompt('أدخل رابط الصورة:');
    if (!url) return;
    this.workerService
      .addPortfolioItem({ mediaType: 'Image', mediaUrl: url })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toast.show('تم إضافة عمل جديد لمعرض أعمالك!', 'success');
          this.refreshProfile();
        },
        error: () => this.toast.show('فشل إضافة الصورة', 'error'),
      });
  }

  onDeletePortfolioImg(imgUrl: string) {
    const profile = this.realProfile();
    if (!profile) return;
    const item = profile.portfolio.find((p) => p.mediaUrl === imgUrl);
    if (!item) return;
    this.workerService
      .removePortfolioItem(item.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toast.show('تم حذف الصورة.', 'info');
          this.refreshProfile();
        },
        error: () => this.toast.show('فشل حذف الصورة', 'error'),
      });
  }

  onSaveSettings() {
    const profile = this.realProfile();
    if (!profile) return;
    this.workerService
      .updateProfile({
        yearsOfExperience: this.settingsExperience,
        hourlyRate: profile.hourlyRate,
        startingPrice: this.settingsPrice,
        biography: this.settingsBio,
        photo: profile.photo,
        coverPhoto: profile.coverPhoto,
        skills: profile.skills,
        serviceAreas: profile.serviceAreas,
      } as WorkerProfileRequest)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.realProfile.set(updated);
          this.toast.show('تم تحديث الإعدادات بنجاح.', 'success');
        },
        error: () => this.toast.show('فشل حفظ الإعدادات', 'error'),
      });
  }

  private refreshBookings() {
    const profile = this.realProfile();
    if (profile) this.loadBookings(profile.id);
  }

  private refreshProfile() {
    this.workerService
      .getMyProfile()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (profile) => this.realProfile.set(profile),
      });
  }

  private formatDate(iso: string): string {
    if (!iso) return '';
    return new Date(iso).toLocaleDateString('ar-EG');
  }

  private formatTime(iso: string): string {
    if (!iso) return '';
    return new Date(iso).toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' });
  }

  private mapOldStatus(s: BookingStatus): 'pending' | 'accepted' | 'completed' | 'cancelled' {
    switch (s) {
      case 'Pending':
        return 'pending';
      case 'Accepted':
      case 'Scheduled':
      case 'OnTheWay':
      case 'Started':
      case 'Paused':
        return 'accepted';
      case 'Completed':
        return 'completed';
      default:
        return 'cancelled';
    }
  }
}
